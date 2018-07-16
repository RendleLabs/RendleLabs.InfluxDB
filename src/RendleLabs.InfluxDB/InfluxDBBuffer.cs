using System;
using System.Buffers;
using System.IO.Pipelines;

namespace RendleLabs.InfluxDB
{
    public class InfluxDBBuffer
    {
        private readonly Pipe _pipe = new Pipe();

        public PipeWriter Writer => _pipe.Writer;
        
        
    }
}