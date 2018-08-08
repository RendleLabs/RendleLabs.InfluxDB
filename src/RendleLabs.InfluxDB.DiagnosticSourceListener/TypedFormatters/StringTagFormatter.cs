using System;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters
{
    internal class StringTagFormatter : TypedFormatter<string>, IFormatter
    {
        public StringTagFormatter(PropertyInfo property) : base(property)
        {
        }

        public bool TryWrite(object obj, Span<byte> span, bool commaPrefix, out int bytesWritten) =>
            TagHelpers.TryWriteString(Name.AsSpan(), Getter(obj), span, out bytesWritten);
    }
}