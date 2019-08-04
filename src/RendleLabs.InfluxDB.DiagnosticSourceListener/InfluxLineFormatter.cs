using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
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

        internal InfluxLineFormatter(string measurement, Type argsType, DiagnosticListenerOptions options)
        {
            _measurement = InfluxName.Escape(measurement);
            _measurementLength = _measurement.Length;
            _objectFormatter = new ObjectFormatter(argsType, options);
            _defaultTags = options.DefaultTags;
            _hasDefaultTags = _defaultTags != null && (_defaultTagsLength = _defaultTags.Length) > 0;

            _baseLength = _measurementLength + _defaultTagsLength + 2;
        }

        public bool TryWrite(Span<byte> span, object args, Activity activity, long requestTimestamp,
            out int bytesWritten)
        {
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

            if (!_objectFormatter.Write(args, activity, span, out int written) || span.Length == 0)
            {
                goto fail;
            }

            bytesWritten += written;

            span = span.Slice(written);
            span[0] = Space;
            span = span.Slice(1);

            if (!Utf8Formatter.TryFormat(requestTimestamp, span, out int timestampWritten))
            {
                goto fail;
            }


            bytesWritten += timestampWritten;

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

