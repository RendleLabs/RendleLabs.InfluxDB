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
        private readonly byte[] _defaultTags;
        private readonly int _defaultTagsLength;
        private readonly bool _hasDefaultTags;
        private readonly int _baseLength;

        internal InfluxLineFormatter(string measurement, Type argsType, Dictionary<(string, Type), Func<PropertyInfo, IFormatter>> customFieldFormatters,
            Dictionary<(string, Type), Func<PropertyInfo, IFormatter>> customTagFormatters, byte[] optionsDefaultTags)
        {
            _measurement = InfluxName.Escape(measurement);
            _measurementLength = _measurement.Length;
            _objectFormatter = new ObjectFormatter(argsType, customFieldFormatters, customTagFormatters);
            _defaultTags = optionsDefaultTags;
            _hasDefaultTags = _defaultTags != null && (_defaultTagsLength = _defaultTags.Length) > 0;

            _baseLength = _measurementLength + _defaultTagsLength + 2;
        }

        public bool TryWrite(Span<byte> span, object args, long requestTimestamp, out int bytesWritten)
        {
            _measurement.CopyTo(span);
            span = span.Slice(_measurementLength);

            if (_hasDefaultTags)
            {
                if (span.Length < _defaultTags.Length)
                {
                    goto fail;
                }
                _defaultTags.CopyTo(span);
            }

            span = span.Slice(_defaultTagsLength);
            
            if (!_objectFormatter.Write(args, span, out int written) || span.Length == 0)
            {
                goto fail;
            }

            span = span.Slice(written);
            span[0] = Space;
            span = span.Slice(1);
            
            if (!Utf8Formatter.TryFormat(requestTimestamp, span, out int timestampWritten))
            {
                goto fail;
            }

            span = span.Slice(timestampWritten);
            if (span.Length == 0)
            {
                goto fail;
            }
            
            span[0] = Newline;
            bytesWritten = _baseLength + written + timestampWritten;
            
            LongestWritten = Math.Max(LongestWritten, bytesWritten);
            
            return true;
            
            fail:
                bytesWritten = 0;
                return false;
        }

        public int LongestWritten { get; private set; }
    }
}