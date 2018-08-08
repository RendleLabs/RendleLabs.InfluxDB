using System;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters
{
    internal class NullableGuidTagFormatter : TypedFormatter<Guid?>, IFormatter
    {
        public NullableGuidTagFormatter(PropertyInfo property) : base(property)
        {
        }

        public bool TryWrite(object obj, Span<byte> span, bool commaPrefix, out int bytesWritten) =>
            TagHelpers.TryWriteGuid(Name.AsSpan(), Getter(obj).GetValueOrDefault(), span, out bytesWritten);
    }
}