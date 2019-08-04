using System;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters
{
    internal class DateTimeTagFormatter : TypedFormatter<DateTime>, IFormatter
    {
        public DateTimeTagFormatter(PropertyInfo property, Func<string, string> propertyNameFormatter)
            : base(property, propertyNameFormatter)
        {
        }

        public bool TryWrite(object obj, Span<byte> span, bool commaPrefix, out int bytesWritten) =>
            TagHelpers.TryWriteDateTime(Name.AsSpan(), Getter(obj), span, out bytesWritten);
    }
}