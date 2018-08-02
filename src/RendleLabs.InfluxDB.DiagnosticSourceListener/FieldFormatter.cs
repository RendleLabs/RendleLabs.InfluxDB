using System;
using System.Collections.Generic;
using System.Reflection;
using RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal static class FieldFormatter // : IFormatter
    {
        public static IFormatter TryCreate(PropertyInfo property)
        {
            return TypedFormatter.Create(property);
        }
        internal static bool IsFieldType(Type type) => FieldTypes.Contains(type);


        private static readonly HashSet<Type> FieldTypes = new HashSet<Type>(new[]
        {
            typeof(bool),
            typeof(bool?),
            typeof(byte),
            typeof(byte?),
            typeof(decimal),
            typeof(decimal?),
            typeof(double),
            typeof(double?),
            typeof(short),
            typeof(short?),
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?),
            typeof(sbyte),
            typeof(sbyte?),
            typeof(float),
            typeof(float?),
            typeof(TimeSpan),
            typeof(TimeSpan?),
            typeof(ushort),
            typeof(ushort?),
            typeof(uint),
            typeof(uint?),
            typeof(ulong),
            typeof(ulong?),
        });
    }
}