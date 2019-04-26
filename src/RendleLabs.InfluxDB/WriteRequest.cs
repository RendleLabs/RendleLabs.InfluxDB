using System;
using System.Diagnostics;

namespace RendleLabs.InfluxDB
{
    public struct WriteRequest
    {
        internal static WriteRequest FlushRequest = new WriteRequest(true);
        public ILineWriter Writer { get; }
        public object Args { get; }
        public Activity Activity { get; }
        public long Timestamp { get; }
        internal bool Flush { get; }

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