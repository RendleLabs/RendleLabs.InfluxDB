using System;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters
{
    internal class GuidTagFormatter : TypedFormatter<Guid>, IFormatter
    {
        public GuidTagFormatter(PropertyInfo property, Func<string, string> propertyNameFormatter)
            : base(property, propertyNameFormatter)
        {
        }

        public bool TryWrite(object obj, Span<byte> span, bool commaPrefix, out int bytesWritten) =>
            TagHelpers.TryWriteGuid(Name.AsSpan(), Getter(obj), span, out bytesWritten);
    }
}