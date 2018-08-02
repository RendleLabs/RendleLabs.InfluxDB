using System;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    public interface IFormatter
    {
        bool TryWrite(object obj, Span<byte> span, bool commaPrefix, out int bytesWritten);
    }
}