using System;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters
{
    internal class NullableDateTimeOffsetTagFormatter : TypedFormatter<DateTimeOffset?>, IFormatter
    {
        public NullableDateTimeOffsetTagFormatter(PropertyInfo property) : base(property)
        {
        }

        public bool TryWrite(object obj, Span<byte> span, bool commaPrefix, out int bytesWritten) =>
            TagHelpers.TryWriteDateTimeOffset(Name.AsSpan(), Getter(obj).GetValueOrDefault(), span, out bytesWritten);
    }
}