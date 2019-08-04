using System;
using System.Collections.Generic;
using System.Reflection;
using RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal static class TagFormatter
    {
        public static IFormatter TryCreate(PropertyInfo property, Func<string, string> propertyNameFormatter)
        {
            if (property.PropertyType == typeof(string)) return new StringTagFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(DateTime)) return new DateTimeTagFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(DateTime?)) return new NullableDateTimeTagFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(DateTimeOffset)) return new DateTimeOffsetTagFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(DateTimeOffset?)) return new NullableDateTimeOffsetTagFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(Guid)) return new GuidTagFormatter(property, propertyNameFormatter);
            if (property.PropertyType == typeof(Guid?)) return new NullableGuidTagFormatter(property, propertyNameFormatter);
            return null;
        }

        internal static bool IsTagType(Type type) => FieldTypes.Contains(type);

        private static readonly HashSet<Type> FieldTypes = new HashSet<Type>(new[]
        {
            typeof(string),
            typeof(Guid),
            typeof(Guid?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
        });
    }
}