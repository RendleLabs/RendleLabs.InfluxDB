using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal static class ActivityWriter
    {
        private const byte Space = (byte) ' ';
        private const byte Comma = (byte) ',';
        private const byte EqualSign = (byte) '=';
        private const byte BackSlash = (byte) '\\';

        private static readonly byte[] ActivityDurationFieldName = Encoding.UTF8.GetBytes("activity_duration");

        public static bool TryWriteDuration(Span<byte> span, TimeSpan duration, bool comma, out int written) =>
            FieldHelpers.Write(ActivityDurationFieldName, duration.TotalMilliseconds, comma, span, out written);

        public static bool TryWriteTags(Span<byte> span, IEnumerable<KeyValuePair<string, string>> tags,
            out int written)
        {
            written = 0;
            foreach (var tag in tags)
            {
                if (span.Length < 2)
                {
                    goto fail;
                }

                span[0] = Comma;
                span = span.Slice(1);

                if (!TryEscapeString(tag.Key.AsSpan(), span, out int keyBytes))
                {
                    goto fail;
                }

                span = span.Slice(keyBytes);
                if (span.Length == 0)
                {
                    goto fail;
                }

                span[0] = EqualSign;
                span = span.Slice(1);

                if (!TryEscapeString(tag.Value.AsSpan(), span, out int valueBytes))
                {
                    goto fail;
                }

                span = span.Slice(valueBytes);

                written += keyBytes + valueBytes + 2;
            }

            return true;

            fail:
            written = 0;
            return false;
        }

#if (NETCOREAPP2_1)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryEscapeString(ReadOnlySpan<char> chars, Span<byte> bytes, out int written)
        {
            written = Encoding.UTF8.GetByteCount(chars);
            if (written > bytes.Length)
            {
                goto fail;
            }

            Span<byte> encoded = stackalloc byte[written];
            Encoding.UTF8.GetBytes(chars, encoded);
            if (encoded.IndexOfAny(Space, Comma, EqualSign) > -1)
            {
                if (!TryEscapeString(encoded, bytes, ref written)) goto fail;
            }
            else
            {
                encoded.CopyTo(bytes);
                return true;
            }

            return true;

            fail:
            written = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryEscapeString(Span<byte> encoded, Span<byte> bytes, ref int written)
        {
            for (int i = 0; i < encoded.Length; i++)
            {
                switch (encoded[i])
                {
                    case Comma:
                    case Space:
                    case EqualSign:
                        if (bytes.Length < 2)
                        {
                            return false;
                        }

                        bytes[0] = BackSlash;
                        bytes[1] = encoded[i];
                        bytes = bytes.Slice(2);
                        ++written;
                        break;
                    default:
                        if (bytes.Length == 0)
                        {
                            return false;
                        }

                        bytes[0] = encoded[i];
                        bytes = bytes.Slice(1);
                        break;
                }
            }

            return true;
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe bool TryEscapeString(ReadOnlySpan<char> chars, Span<byte> bytes, out int written)
        {
            fixed (char* cp = chars)
            {
                written = Encoding.UTF8.GetByteCount(cp, chars.Length);
                if (written > bytes.Length)
                {
                    goto fail;
                }

                byte* buffer = stackalloc byte[written];
                Encoding.UTF8.GetBytes(cp, chars.Length, buffer, written);
                var encoded = new Span<byte>(buffer, written);
                if (encoded.IndexOfAny(Space, Comma, EqualSign) > -1)
                {
                    if (!TryEscapeString(encoded, bytes, ref written)) goto fail;
                }
                else
                {
                    encoded.CopyTo(bytes);
                }
            }

            return true;

            fail:
            written = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryEscapeString(Span<byte> encoded, Span<byte> bytes, ref int written)
        {
            do
            {
                switch (encoded[0])
                {
                    case Comma:
                    case Space:
                    case EqualSign:
                        if (bytes.Length < 2)
                        {
                            return false;
                        }

                        bytes[0] = BackSlash;
                        bytes[1] = encoded[0];
                        bytes = bytes.Slice(2);
                        ++written;
                        break;
                    default:
                        if (bytes.Length == 0)
                        {
                            return false;
                        }

                        bytes[0] = encoded[0];
                        bytes = bytes.Slice(1);
                        break;
                }

                encoded = encoded.Slice(1);
            } while (encoded.Length > 0);

            return true;
        }
#endif
    }
}