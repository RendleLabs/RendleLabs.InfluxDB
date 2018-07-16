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
        
        public int Write(object args, ref Span<byte> span)
        {
            int written = 1;
            bool comma = false;

            for (int i = 0; i < _tagCount; i++)
            {
                written += _tagFormatters[i].Write(args, ref span);
            }

            span[0] = Space;
            span = span.Slice(1);
            
            for (int i = 0; i < _fieldCount; i++)
            {
                if (comma)
                {
                    span[0] = Comma;
                    span = span.Slice(1);
                }

                int fieldWritten = _fieldFormatters[i].Write(args, ref span);
                written += fieldWritten;
                comma = comma || fieldWritten > 0;
            }

            return written;
        }
    }
}