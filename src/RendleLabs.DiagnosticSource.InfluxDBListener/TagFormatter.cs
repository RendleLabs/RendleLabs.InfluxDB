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
        private delegate int Format(object value, Span<byte> span);

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

        public int Write(object obj, ref Span<byte> span)
        {
            var value = _property.GetValue(obj);
            if (value == null) return 0;
            
            var hold = span;
            span[0] = Comma;
            span = span.Slice(1);
            _name.CopyTo(span);
            span = span.Slice(_name.Length);
            span[0] = EqualSign;
            span = span.Slice(1);
            int written = _format(value, span);
            if (written == 0)
            {
                span = hold;
                return 0;
            }
            span = span.Slice(written);
            return _nameLength + written + 2;
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

        private static int WriteString(object value, Span<byte> span)
        {
            var str = (string) value;
            if (str.Length == 0) return 0;
            
            int written = 0;
            
            var length = Encoding.UTF8.GetByteCount(str);
            
            if (length == str.Length)
            {
                written = length;
                var buffer = ArrayPool<byte>.Shared.Rent(length);
                
                Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 0);
                for (int i = 0; i < length; i++)
                {
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

                    span[0] = buffer[i];
                    span = span.Slice(1);
                }

                return written;
            }
            
            unsafe
            {
                byte* charBytes = stackalloc byte[8];
                int index = 0;
                fixed (char* c = str)
                {
                    for (int i = 0; i < str.Length; i++)
                    {
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

                        int byteCount = Encoding.UTF8.GetBytes(c + i, 1, charBytes, 8);
                        new ReadOnlySpan<byte>(charBytes, byteCount).CopyTo(span);
                        span = span.Slice(byteCount);
                        written += byteCount;
                    }
                }
            }

            return written;
        }

        private static int WriteDateTime(object value, Span<byte> span) => Utf8Formatter.TryFormat((DateTime) value, span, out int written, 'O') ? written : 0;

        private static int WriteDateTimeOffset(object value, Span<byte> span) =>
            Utf8Formatter.TryFormat((DateTimeOffset) value, span, out int written, 'O') ? written : 0;

        private static int WriteGuid(object value, Span<byte> span) => Utf8Formatter.TryFormat((Guid) value, span, out int written, 'N') ? written : 0;

        private static readonly HashSet<Type> FieldTypes = new HashSet<Type>(new[]
        {
            typeof(string),
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
        });
    }
}