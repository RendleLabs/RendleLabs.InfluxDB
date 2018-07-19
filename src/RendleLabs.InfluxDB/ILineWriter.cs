using System;

namespace RendleLabs.InfluxDB
{
    public interface ILineWriter
    {
        bool TryWrite(Span<byte> buffer, object args, long requestTimestamp, out int bytesWritten);
    }
}