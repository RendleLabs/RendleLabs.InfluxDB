using System;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters
{
    internal static class TypedFormatter
    {
        internal static IFormatter Create(PropertyInfo property, Func<string, string> propertyNameFormatter)
        {
            if (property.PropertyType == typeof(bool)) return new BooleanFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(bool?)) return new NullableBooleanFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(byte)) return new ByteFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(byte?)) return new NullableByteFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(decimal)) return new DecimalFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(decimal?)) return new NullableDecimalFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(double)) return new DoubleFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(double?)) return new NullableDoubleFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(Guid)) return new GuidFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(Guid?)) return new NullableGuidFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(short)) return new Int16FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(short?)) return new NullableInt16FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(int)) return new Int32FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(int?)) return new NullableInt32FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(long)) return new Int64FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(long?)) return new NullableInt64FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(ushort)) return new UInt16FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(ushort?)) return new NullableUInt16FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(uint)) return new UInt32FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(uint?)) return new NullableUInt32FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(ulong)) return new UInt64FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(ulong?)) return new NullableUInt64FieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(sbyte)) return new SByteFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(sbyte?)) return new NullableSByteFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(float)) return new SingleFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(float?)) return new NullableSingleFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(TimeSpan)) return new TimeSpanFieldFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(TimeSpan?)) return new NullableTimeSpanFieldFormatter(property, propertyNameFormatter);
            return null;
        }
    }
}