using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal class InfluxLineFormatter : ILineWriter
    {
        private const byte Newline = (byte)'\n';
        private const byte Space = (byte)' ';
        
        private readonly byte[] _measurement;
        private readonly int _measurementLength;
        private readonly ObjectFormatter _objectFormatter;


        internal InfluxLineFormatter(string measurement, Type argsType, Dictionary<(string, Type), Func<PropertyInfo, IFormatter>> customFieldFormatters,
            Dictionary<(string, Type), Func<PropertyInfo, IFormatter>> customTagFormatters)
        {
            _measurement = InfluxName.Escape(measurement);
            _measurementLength = _measurement.Length;
            _objectFormatter = new ObjectFormatter(argsType, customFieldFormatters, customTagFormatters);
        }

        public bool TryWrite(Span<byte> span, object args, long requestTimestamp, out int bytesWritten)
        {
            _measurement.CopyTo(span);
            span = span.Slice(_measurementLength);
            if (!_objectFormatter.Write(args, span, out int written) || span.Length == 0)
            {
                bytesWritten = 0;
                return false;
            }

            span = span.Slice(written);
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