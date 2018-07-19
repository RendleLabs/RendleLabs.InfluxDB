using System;
using System.Collections.Generic;
using System.Linq;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    public sealed class ObjectFormatter
    {
        private const byte Comma = 44;
        private const byte Space = 32;

        private readonly FieldFormatter[] _fieldFormatters;
        private readonly int _fieldCount;
        private readonly TagFormatter[] _tagFormatters;
        private readonly int _tagCount;

        public ObjectFormatter(Type type)
        {
            var fieldFormatters = new List<FieldFormatter>();
            var tagFormatters = new List<TagFormatter>();
            foreach (var property in type.GetProperties().Where(p => p.CanRead))
            {
                if (FieldFormatter.IsFieldType(property.PropertyType))
                {
                    var formatter = FieldFormatter.TryCreate(property);
                    if (formatter != null)
                    {
                        fieldFormatters.Add(formatter);
                    }
                }
                else if (TagFormatter.IsTagType(property.PropertyType))
                {
                    var tagFormatter = TagFormatter.TryCreate(property);
                    if (tagFormatter != null)
                    {
                        tagFormatters.Add(tagFormatter);
                    }
                }
            }

            _fieldFormatters = fieldFormatters.ToArray();
            _fieldCount = _fieldFormatters.Length;
            _tagFormatters = tagFormatters.ToArray();
            _tagCount = _tagFormatters.Length;
        }

        public bool Write(object args, ref Span<byte> span, out int bytesWritten)
        {
            if (span.Length == 0) goto fail;

            bytesWritten = 0;

            bool comma = false;

            for (int i = 0; i < _tagCount; i++)
            {
                if (span.Length == 0) goto fail;

                if (!_tagFormatters[i].TryWrite(args, ref span, out int tagWritten)) goto fail;

                bytesWritten += tagWritten;
            }

            span[0] = Space;
            span = span.Slice(1);

            for (int i = 0; i < _fieldCount; i++)
            {
                if (span.Length == 0) goto fail;
                
                if (comma)
                {
                    span[0] = Comma;
                    span = span.Slice(1);
                    bytesWritten++;
                }

                if (!_fieldFormatters[i].TryWrite(args, ref span, out int fieldWritten)) goto fail;

                bytesWritten += fieldWritten;
                comma = comma || fieldWritten > 0;
            }
            return true;

            fail:
            bytesWritten = 0;
            return false;
        }
    }
}