using System;
using System.Buffers;
using System.Text;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    public static class StringExtensions
    {
        private const byte Backslash = 92;
        private const byte EqualSign = 61;
        private const byte Comma = 44;
        private const byte Space = 32;
        
        public static bool TryWriteEscapedUTF8(this string str, Span<byte> span, out int bytesWritten)
        {
            var length = Encoding.UTF8.GetByteCount(str);

            if (length > span.Length)
            {
                bytesWritten = 0;
                return false;
            }

            if (length == str.Length)
            {
                return FastWriteString(span, length, str, out bytesWritten);
            }

            return UnsafeWriteString(span, str, out bytesWritten);
        }

        private static bool FastWriteString(Span<byte> span, int length, string str, out int bytesWritten)
        {
            var written = length;

            for (int i = 0; i < length; i++)
            {
                if (span.Length == 0)
                {
                    bytesWritten = 0;
                    return false;
                }

                byte ch = (byte) str[i];

                switch (ch)
                {
                    case Space:
                    case Comma:
                    case EqualSign:
                        span[0] = Backslash;
                        span = span.Slice(1);
                        written++;
                        break;
                }

                if (span.Length > 0)
                {
                    span[0] = ch;
                    span = span.Slice(1);
                }
            }

            bytesWritten = written;
            return true;
        }

        private static unsafe bool UnsafeWriteString(Span<byte> span, string str, out int bytesWritten)
        {
            int written = 0;
            byte* charBytes = stackalloc byte[8];
            int index = 0;
            fixed (char* c = str)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (span.Length == 0)
                    {
                        bytesWritten = 0;
                        return false;
                    }

                    switch (*c + i)
                    {
                        case ' ':
                        case ',':
                        case '=':
                            span[index++] = Backslash;
                            span = span.Slice(1);
                            written++;
                            break;
                    }

                    if (span.Length > 0)
                    {
                        int byteCount = Encoding.UTF8.GetBytes(c + i, 1, charBytes, 8);
                        new ReadOnlySpan<byte>(charBytes, byteCount).CopyTo(span);
                        span = span.Slice(byteCount);
                        written += byteCount;
                    }
                }
            }

            bytesWritten = written;
            return true;
        }
    }
}