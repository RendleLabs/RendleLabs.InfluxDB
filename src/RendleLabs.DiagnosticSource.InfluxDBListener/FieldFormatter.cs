using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    internal sealed class FieldFormatter
    {
        private delegate bool Format(object value, Span<byte> span, out int written);

        private const byte TrueValue = (byte) 't';
        private const byte FalseValue = (byte) 'f';
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

        public bool TryWrite(object obj, ref Span<byte> span, out int bytesWritten)
        {
            if (span.Length < _nameLength + 2)
            {
                bytesWritten = 0;
                return false;
            }
            
            var value = _property.GetValue(obj);
            if (value == null)
            {
                bytesWritten = 0;
                return true;
            }
            
            var hold = span;
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

            span = span.Slice(written);
            bytesWritten = _nameLength + written + 1;
            return true;
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

        private static bool WriteBoolean(object value, Span<byte> span, out int written)
        {
            if ((bool) value)
            {
                span[0] = TrueValue;
            }
            else
            {
                span[0] = FalseValue;
            }

            written = 1;
            return true;
        }
        
        private static bool WriteByte(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((byte) value, span, out written);
        private static bool WriteDateTime(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((DateTime) value, span, out written);

        private static bool WriteDateTimeOffset(object value, Span<byte> span, out int written) =>
            Utf8Formatter.TryFormat((DateTimeOffset) value, span, out written);

        private static bool WriteDecimal(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((decimal) value, span, out written);
        private static bool WriteDouble(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((double) value, span, out written);
        private static bool WriteGuid(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((Guid) value, span, out written);
        private static bool WriteInt16(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((short) value, span, out written);
        private static bool WriteInt32(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((int) value, span, out written);
        private static bool WriteInt64(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((long) value, span, out written);
        private static bool WriteSByte(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((sbyte) value, span, out written);
        private static bool WriteSingle(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((float) value, span, out written);
        private static bool WriteTimeSpan(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((float) value, span, out written);
        private static bool WriteUInt16(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((ushort) value, span, out written);
        private static bool WriteUInt32(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((uint) value, span, out written);
        private static bool WriteUInt64(object value, Span<byte> span, out int written) => Utf8Formatter.TryFormat((ulong) value, span, out written);

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