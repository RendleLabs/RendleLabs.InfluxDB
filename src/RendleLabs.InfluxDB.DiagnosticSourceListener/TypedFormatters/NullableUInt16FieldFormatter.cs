using System;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.TypedFormatters
{
    internal class NullableUInt16FieldFormatter : TypedFormatter<ushort?>, IFormatter
    {
        public NullableUInt16FieldFormatter(PropertyInfo property, Func<string, string> propertyNameFormatter)
            : base(property, propertyNameFormatter)
        {
        }

        public bool TryWrite(object obj, Span<byte> span, bool commaPrefix, out int bytesWritten)
        {
            return FieldHelpers.Write(Name.AsSpan(), Getter(obj).GetValueOrDefault(), commaPrefix, span, out bytesWritten);
        }
    }
}