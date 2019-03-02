using System;
using System.Buffers.Text;
using System.Diagnostics;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal class InfluxNullLineFormatter : ILineWriter
    {
        private const byte Newline = (byte)'\n';
        private const byte Space = (byte)' ';
        
        private readonly byte[] _measurement;
        private readonly int _measurementLength;
        private readonly byte[] _defaultTags;
        private readonly int _defaultTagsLength;
        private readonly bool _hasDefaultTags;
        private readonly int _baseLength;

        internal InfluxNullLineFormatter(string measurement, byte[] optionsDefaultTags)
        {
            _measurement = InfluxName.Escape(measurement);
            _measurementLength = _measurement.Length;
            _defaultTags = optionsDefaultTags;
            _hasDefaultTags = _defaultTags != null && (_defaultTagsLength = _defaultTags.Length) > 0;

            _baseLength = _measurementLength + _defaultTagsLength + 2;
        }

        public bool TryWrite(Span<byte> span, object args, Activity activity, long requestTimestamp, out int bytesWritten)
        {
            if (span.Length < _measurementLength)
            {
                goto fail;
            }

            bytesWritten = _baseLength;
            
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
            
            if (activity != null)
            {
                if (!ActivityWriter.TryWriteTags(span, activity.Tags, out int tagsWritten))
                {
                    goto fail;
                }

                bytesWritten += tagsWritten;
            }

            span[0] = Space;
            span = span.Slice(1);
            bytesWritten++;

            if (activity != null && activity.Duration.Ticks > 0L)
            {
                if (!ActivityWriter.TryWriteDuration(span, activity.Duration, false, out int durationWritten))
                {
                    goto fail;
                }

                bytesWritten += durationWritten;
            }

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
            bytesWritten = _baseLength + timestampWritten;
            return true;
            
            fail:
            bytesWritten = 0;
            return false;
        }
    }
}