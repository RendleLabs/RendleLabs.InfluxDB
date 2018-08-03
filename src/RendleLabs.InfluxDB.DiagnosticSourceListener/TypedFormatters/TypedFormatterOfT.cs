using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters
{
    internal abstract class TypedFormatter<T>
    {
        protected readonly Func<object, T> Getter;
        protected readonly byte[] Name;

        protected TypedFormatter(PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (property.DeclaringType == null) throw new InvalidOperationException();

            var parameter = Expression.Parameter(typeof(object));
            var get = Expression.Property(Expression.Convert(parameter, property.DeclaringType), property);
            Getter = Expression.Lambda<Func<object, T>>(get, parameter).Compile();
            Name = InfluxName.Escape(property.Name);
        }
    }
}