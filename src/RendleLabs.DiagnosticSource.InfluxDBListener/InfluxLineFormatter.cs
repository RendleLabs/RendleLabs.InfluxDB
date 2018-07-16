using System;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    public class InfluxLineFormatter
    {
        private const byte Newline = 10;
        
        private static readonly ConcurrentDictionary<string, InfluxLineFormatter> Formatters =
            new ConcurrentDictionary<string, InfluxLineFormatter>();
        private readonly byte[] _measurement;
        private readonly int _measurementLength;
        private readonly ObjectFormatter _objectFormatter;
        

        public InfluxLineFormatter(string measurement, Type argsType)
        {
            _measurement = InfluxName.Escape(measurement);
            _measurementLength = _measurement.Length;
            _objectFormatter = new ObjectFormatter(argsType);
        }

        public int Write(object args, ref Span<byte> span)
        {
            _measurement.CopyTo(span);
            span = span.Slice(_measurementLength);
            int written = _objectFormatter.Write(args, ref span);
            span[0] = Newline;
            span = span.Slice(1);
            return _measurementLength + written + 1;
        }

        public static InfluxLineFormatter GetOrAdd(string measurement, Type argsType)
        {
            return Formatters.GetOrAdd(measurement, m => new InfluxLineFormatter(m, argsType));
        }
    }
}