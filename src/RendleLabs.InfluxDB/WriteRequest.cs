using System;
using System.Diagnostics;

namespace RendleLabs.InfluxDB
{
    public readonly struct WriteRequest
    {
        internal static WriteRequest FlushRequest = new WriteRequest(true);
        public readonly ILineWriter Writer;
        public readonly object Args;
        public readonly Activity? Activity;
        public readonly long Timestamp;
        internal readonly bool FlushSentinel;

        public WriteRequest(ILineWriter writer, object args, Activity? activity = null, DateTimeOffset? timestamp = null)
        {
            Writer = writer;
            Args = args;
            Activity = activity;
            Timestamp = timestamp.GetValueOrDefault(DateTimeOffset.UtcNow).ToUnixTimeMilliseconds();
            FlushSentinel = false;
        }

        private WriteRequest(bool flushSentinel)
        {
            FlushSentinel = flushSentinel;
            Writer = new NullWriter();
            Args = new object();
            Activity = default;
            Timestamp = default;
        }

        private class NullWriter : ILineWriter
        {
            public bool TryWrite(Span<byte> buffer, object? args, Activity? activity, long requestTimestamp, out int bytesWritten)
            {
                throw new NotImplementedException();
            }

            public int LongestWritten { get; } = 0;
        }
    }
}