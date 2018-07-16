using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    internal sealed class FieldFormatter
    {
        private delegate int Format(object value, Span<byte> span);

        private const byte Backslash = 92;
        private const byte EqualSign = 61;
        private readonly byte[] _name;
        private readonly int _nameLength;
        private readonly PropertyInfo _property;
        private readonly Format _format;

        private FieldFormatter(PropertyInfo property, Format format)
        {
            _property = property;
            _format = format;
            _name = InfluxName.Escape(property.Name);
            _nameLength = _name.Length;
        }

        public static FieldFormatter TryCreate(PropertyInfo property)
        {
            var format = ChooseFormat(property.PropertyType);
            if (format == null) return null;
            return new FieldFormatter(property, format);
        }

        public int Write(object obj, ref Span<byte> span)
        {
            var value = _property.GetValue(obj);
            if (value == null) return 0;
            var hold = span;
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
            return _nameLength + written + 1;
        }

        internal static bool IsFieldType(Type type) => FieldTypes.Contains(type);

        private static Format ChooseFormat(Type type)
        {
            if (type == typeof(bool)) return WriteBoolean;
            if (type == typeof(byte)) return WriteByte;
            if (type == typeof(DateTime)) return WriteDateTime;
            if (type == typeof(DateTimeOffset)) return WriteDateTimeOffset;
            if (type == typeof(decimal)) return WriteDecimal;
            if (type == typeof(double)) return WriteDouble;
            if (type == typeof(Guid)) return WriteGuid;
            if (type == typeof(short)) return WriteInt16;
            if (type == typeof(int)) return WriteInt32;
            if (type == typeof(long)) return WriteInt64;
            if (type == typeof(sbyte)) return WriteSByte;
            if (type == typeof(float)) return WriteSingle;
            if (type == typeof(TimeSpan)) return WriteTimeSpan;
            if (type == typeof(ushort)) return WriteUInt16;
            if (type == typeof(uint)) return WriteUInt32;
            if (type == typeof(ulong)) return WriteUInt64;
            return null;
        }

        private static int WriteBoolean(object value, Span<byte> span) => Utf8Formatter.TryFormat((bool) value, span, out int written) ? written : 0;
        private static int WriteByte(object value, Span<byte> span) => Utf8Formatter.TryFormat((byte) value, span, out int written) ? written : 0;
        private static int WriteDateTime(object value, Span<byte> span) => Utf8Formatter.TryFormat((DateTime) value, span, out int written) ? written : 0;

        private static int WriteDateTimeOffset(object value, Span<byte> span) =>
            Utf8Formatter.TryFormat((DateTimeOffset) value, span, out int written) ? written : 0;

        private static int WriteDecimal(object value, Span<byte> span) => Utf8Formatter.TryFormat((decimal) value, span, out int written) ? written : 0;
        private static int WriteDouble(object value, Span<byte> span) => Utf8Formatter.TryFormat((double) value, span, out int written) ? written : 0;
        private static int WriteGuid(object value, Span<byte> span) => Utf8Formatter.TryFormat((Guid) value, span, out int written) ? written : 0;
        private static int WriteInt16(object value, Span<byte> span) => Utf8Formatter.TryFormat((short) value, span, out int written) ? written : 0;
        private static int WriteInt32(object value, Span<byte> span) => Utf8Formatter.TryFormat((int) value, span, out int written) ? written : 0;
        private static int WriteInt64(object value, Span<byte> span) => Utf8Formatter.TryFormat((long) value, span, out int written) ? written : 0;
        private static int WriteSByte(object value, Span<byte> span) => Utf8Formatter.TryFormat((sbyte) value, span, out int written) ? written : 0;
        private static int WriteSingle(object value, Span<byte> span) => Utf8Formatter.TryFormat((float) value, span, out int written) ? written : 0;
        private static int WriteTimeSpan(object value, Span<byte> span) => Utf8Formatter.TryFormat((float) value, span, out int written) ? written : 0;
        private static int WriteUInt16(object value, Span<byte> span) => Utf8Formatter.TryFormat((ushort) value, span, out int written) ? written : 0;
        private static int WriteUInt32(object value, Span<byte> span) => Utf8Formatter.TryFormat((uint) value, span, out int written) ? written : 0;
        private static int WriteUInt64(object value, Span<byte> span) => Utf8Formatter.TryFormat((ulong) value, span, out int written) ? written : 0;

        private static readonly HashSet<Type> FieldTypes = new HashSet<Type>(new[]
        {
            typeof(bool),
            typeof(byte),
            typeof(decimal),
            typeof(double),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(sbyte),
            typeof(float),
            typeof(TimeSpan),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
        });
    }
}