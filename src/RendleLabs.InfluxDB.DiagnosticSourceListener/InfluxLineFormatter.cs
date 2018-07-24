using System;
using System.Buffers.Text;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal class InfluxLineFormatter : ILineWriter
    {
        private const byte Newline = (byte)'\n';
        private const byte Space = (byte)' ';
        
        private readonly byte[] _measurement;
        private readonly int _measurementLength;
        private readonly ObjectFormatter _objectFormatter;


        internal InfluxLineFormatter(string measurement, Type argsType)
        {
            _measurement = InfluxName.Escape(measurement);
            _measurementLength = _measurement.Length;
            _objectFormatter = new ObjectFormatter(argsType);
        }

        public bool TryWrite(Span<byte> span, object args, long requestTimestamp, out int bytesWritten)
        {
            _measurement.CopyTo(span);
            span = span.Slice(_measurementLength);
            if (!_objectFormatter.Write(args, ref span, out int written) || span.Length == 0)
            {
                bytesWritten = 0;
                return false;
            }

            span[0] = Space;
            span = span.Slice(1);
            
            if (!Utf8Formatter.TryFormat(requestTimestamp, span, out int timestampWritten) || span.Length == timestampWritten)
            {
                bytesWritten = 0;
                return false;
            }

            span = span.Slice(timestampWritten);
            
            span[0] = Newline;
            bytesWritten = _measurementLength + written + timestampWritten + 2;
            return true;
        }
    }
}