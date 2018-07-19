using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    internal sealed class TagFormatter
    {
        private delegate bool Format(object value, Span<byte> span, out int bytesWritten);

        private const byte Backslash = 92;
        private const byte EqualSign = 61;
        private const byte Comma = 44;
        private const byte Space = 32;

        private readonly byte[] _name;
        private readonly int _nameLength;
        private readonly PropertyInfo _property;
        private readonly Format _format;

        private TagFormatter(PropertyInfo property, Format format)
        {
            _property = property;
            _format = format;
            _name = InfluxName.Escape(property.Name);
            _nameLength = _name.Length;
        }

        public static TagFormatter TryCreate(PropertyInfo property)
        {
            var format = ChooseFormat(property.PropertyType);
            if (format == null) return null;
            return new TagFormatter(property, format);
        }

        public bool TryWrite(object obj, ref Span<byte> span, out int bytesWritten)
        {
            var value = _property.GetValue(obj);
            if (value == null || value.Equals(string.Empty))
            {
                bytesWritten = 0;
                return true;
            }

            var hold = span;
            span[0] = Comma;
            span = span.Slice(1);
            _name.CopyTo(span);
            span = span.Slice(_name.Length);
            span[0] = EqualSign;
            span = span.Slice(1);

            if (!_format(value, span, out int written))
            {
                span = hold;
                bytesWritten = 0;
                return false;
            }

            if (written == 0)
            {
                span = hold;
                bytesWritten = 0;
                return true;
            }

            span = span.Slice(written);
            bytesWritten = _nameLength + written + 2;
            return true;
        }

        internal static bool IsTagType(Type type) => FieldTypes.Contains(type);

        private static Format ChooseFormat(Type type)
        {
            if (type == typeof(DateTime)) return WriteDateTime;
            if (type == typeof(DateTimeOffset)) return WriteDateTimeOffset;
            if (type == typeof(Guid)) return WriteGuid;
            if (type == typeof(string)) return WriteString;
            return null;
        }

        private static bool WriteString(object value, Span<byte> span, out int bytesWritten)
        {
            var str = (string) value;

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
            var buffer = ArrayPool<byte>.Shared.Rent(length);

            Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 0);
            for (int i = 0; i < length; i++)
            {
                if (span.Length == 0)
                {
                    bytesWritten = 0;
                    return false;
                }

                switch (buffer[i])
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
                    span[0] = buffer[i];
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

        private static bool WriteDateTime(object value, Span<byte> span, out int bytesWritten) =>
            Utf8Formatter.TryFormat((DateTime) value, span, out bytesWritten);

        private static bool WriteDateTimeOffset(object value, Span<byte> span, out int bytesWritten) =>
            Utf8Formatter.TryFormat((DateTimeOffset) value, span, out bytesWritten);

        private static bool WriteGuid(object value, Span<byte> span, out int bytesWritten) =>
            Utf8Formatter.TryFormat((Guid) value, span, out bytesWritten);

        private static readonly HashSet<Type> FieldTypes = new HashSet<Type>(new[]
        {
            typeof(string),
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
        });
    }
}