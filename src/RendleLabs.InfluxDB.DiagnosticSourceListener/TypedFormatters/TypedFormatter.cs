using System;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters
{
    internal static class TypedFormatter
    {
        internal static IFormatter Create(PropertyInfo property)
        {
            if (property.PropertyType == typeof(bool)) return new BooleanFieldFormatter(property);
            if (property.PropertyType == typeof(bool?)) return new NullableBooleanFieldFormatter(property);
            if (property.PropertyType == typeof(byte)) return new ByteFieldFormatter(property);
            if (property.PropertyType == typeof(byte?)) return new NullableByteFieldFormatter(property);
            if (property.PropertyType == typeof(decimal)) return new DecimalFieldFormatter(property);
            if (property.PropertyType == typeof(decimal?)) return new NullableDecimalFieldFormatter(property);
            if (property.PropertyType == typeof(double)) return new DoubleFieldFormatter(property);
            if (property.PropertyType == typeof(double?)) return new NullableDoubleFieldFormatter(property);
            if (property.PropertyType == typeof(Guid)) return new GuidFieldFormatter(property);
            if (property.PropertyType == typeof(Guid?)) return new NullableGuidFieldFormatter(property);
            if (property.PropertyType == typeof(short)) return new Int16FieldFormatter(property);
            if (property.PropertyType == typeof(short?)) return new NullableInt16FieldFormatter(property);
            if (property.PropertyType == typeof(int)) return new Int32FieldFormatter(property);
            if (property.PropertyType == typeof(int?)) return new NullableInt32FieldFormatter(property);
            if (property.PropertyType == typeof(long)) return new Int64FieldFormatter(property);
            if (property.PropertyType == typeof(long?)) return new NullableInt64FieldFormatter(property);
            if (property.PropertyType == typeof(ushort)) return new UInt16FieldFormatter(property);
            if (property.PropertyType == typeof(ushort?)) return new NullableUInt16FieldFormatter(property);
            if (property.PropertyType == typeof(uint)) return new UInt32FieldFormatter(property);
            if (property.PropertyType == typeof(uint?)) return new NullableUInt32FieldFormatter(property);
            if (property.PropertyType == typeof(ulong)) return new UInt64FieldFormatter(property);
            if (property.PropertyType == typeof(ulong?)) return new NullableUInt64FieldFormatter(property);
            if (property.PropertyType == typeof(sbyte)) return new SByteFieldFormatter(property);
            if (property.PropertyType == typeof(sbyte?)) return new NullableSByteFieldFormatter(property);
            if (property.PropertyType == typeof(float)) return new SingleFieldFormatter(property);
            if (property.PropertyType == typeof(float?)) return new NullableSingleFieldFormatter(property);
            if (property.PropertyType == typeof(TimeSpan)) return new TimeSpanFieldFormatter(property);
            if (property.PropertyType == typeof(TimeSpan?)) return new NullableTimeSpanFieldFormatter(property);
            return null;
        }
    }
}