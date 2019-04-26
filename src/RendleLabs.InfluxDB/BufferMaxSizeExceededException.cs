using System;
using System.Runtime.Serialization;

namespace RendleLabs.InfluxDB
{
    [Serializable]
    public class BufferMaxSizeExceededException : Exception
    {
        public BufferMaxSizeExceededException()
        {
        }

        public BufferMaxSizeExceededException(string message) : base(message)
        {
        }

        public BufferMaxSizeExceededException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BufferMaxSizeExceededException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}