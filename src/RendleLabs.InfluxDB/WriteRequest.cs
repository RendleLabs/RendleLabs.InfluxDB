using System;
using System.Diagnostics;

namespace RendleLabs.InfluxDB
{
    public struct WriteRequest
    {
        internal static WriteRequest FlushRequest = new WriteRequest(true);
        public readonly ILineWriter Writer;
        public readonly object Args;
        public readonly Activity Activity;
        public readonly long Timestamp;
        internal readonly bool Flush;

        public WriteRequest(ILineWriter writer, object args, Activity activity = null, DateTimeOffset? timestamp = null)
        {
            Writer = writer;
            Args = args;
            Activity = activity;
            Timestamp = timestamp.GetValueOrDefault(DateTimeOffset.UtcNow).ToUnixTimeMilliseconds();
            Flush = false;
        }

        private WriteRequest(bool flush)
        {
            Flush = flush;
            Writer = default;
            Args = default;
            Activity = default;
            Timestamp = default;
        }
    }
}