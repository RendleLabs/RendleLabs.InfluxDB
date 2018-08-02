using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal sealed class TagFormatter : IFormatter
    {
        private delegate bool Format(object value, Span<byte> span, out int bytesWritten);

        private const byte EqualSign = 61;
        private const byte Comma = 44;

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

        public bool TryWrite(object obj, Span<byte> span, bool comma, out int bytesWritten)
        {
            var value = _property.GetValue(obj);
            if (value == null || value.Equals(string.Empty))
            {
                bytesWritten = 0;
                return true;
            }

            span[0] = Comma;
            span = span.Slice(1);
            _name.CopyTo(span);
            span = span.Slice(_name.Length);
            span[0] = EqualSign;
            span = span.Slice(1);

            if (!_format(value, span, out int written))
            {
                bytesWritten = 0;
                return false;
            }

            if (written == 0)
            {
                bytesWritten = 0;
                return true;
            }

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
            return str.TryWriteEscapedUTF8(span, out bytesWritten);
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