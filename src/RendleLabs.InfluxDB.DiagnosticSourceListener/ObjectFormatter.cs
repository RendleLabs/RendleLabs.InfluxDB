using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    using CustomDict = Dictionary<(string, Type), Func<PropertyInfo, IFormatter>>;
    
    internal sealed class ObjectFormatter
    {
        private const byte Space = (byte) ' ';

        private readonly IFormatter[] _fieldFormatters;
        private readonly int _fieldCount;
        private readonly IFormatter[] _tagFormatters;
        private readonly int _tagCount;

        public ObjectFormatter(Type type, DiagnosticListenerOptions options)
        {
            var (fieldFormatters, tagFormatters) = CreateFormatters(type, options);

            _fieldFormatters = fieldFormatters.ToArray();
            _fieldCount = _fieldFormatters.Length;
            _tagFormatters = tagFormatters.ToArray();
            _tagCount = _tagFormatters.Length;
        }

        public bool Write(object args, Activity activity, Span<byte> span, out int bytesWritten)
        {
            if (span.Length == 0) goto fail;

            bytesWritten = 0;

            for (int i = 0; i < _tagCount; i++)
            {
                if (span.Length == 0) goto fail;

                if (!_tagFormatters[i].TryWrite(args, span, true, out int tagWritten)) goto fail;

                span = span.Slice(tagWritten);
                bytesWritten += tagWritten;
            }

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

            bool comma = false;
            for (int i = 0; i < _fieldCount; i++)
            {
                if (span.Length == 0) goto fail;

                if (!_fieldFormatters[i].TryWrite(args, span, comma, out int fieldWritten)) goto fail;

                span = span.Slice(fieldWritten);
                bytesWritten += fieldWritten;
                comma = comma || fieldWritten > 0;
            }

            if (activity != null && activity.Duration.Ticks > 0L)
            {
                if (!ActivityWriter.TryWriteDuration(span, activity.Duration, comma, out int durationWritten))
                {
                    goto fail;
                }

                bytesWritten += durationWritten;
            }

            return true;

            fail:
            bytesWritten = 0;
            return false;
        }

        private static (List<IFormatter> fieldFormatters, List<IFormatter> tagFormatters) CreateFormatters(Type type,
            DiagnosticListenerOptions options)
        {
            var fieldFormatters = new List<IFormatter>();
            var tagFormatters = new List<IFormatter>();
            foreach (var property in type.GetProperties().Where(p => p.CanRead))
            {
                if (CheckCustomFormatters(options.CustomFieldFormatters, options.CustomTagFormatters, property, fieldFormatters, tagFormatters))
                {
                    continue;
                }

                if (FieldFormatter.IsFieldType(property.PropertyType))
                {
                    var formatter = FieldFormatter.TryCreate(property, options.FieldNameFormatter ?? NameFixer.Identity);
                    if (formatter != null)
                    {
                        fieldFormatters.Add(formatter);
                    }
                }
                else if (TagFormatter.IsTagType(property.PropertyType))
                {
                    var tagFormatter = TagFormatter.TryCreate(property, options.TagNameFormatter ?? NameFixer.Identity);
                    if (tagFormatter != null)
                    {
                        tagFormatters.Add(tagFormatter);
                    }
                }
            }

            return (fieldFormatters, tagFormatters);
        }

        private static bool CheckCustomFormatters(CustomDict customFieldFormatters, CustomDict customTagFormatters, PropertyInfo property,
            List<IFormatter> fieldFormatters, List<IFormatter> tagFormatters)
        {
            bool custom = false;

            if (customFieldFormatters != null && customFieldFormatters.TryGetValue((property.Name, property.PropertyType), out var cf))
            {
                fieldFormatters.Add(cf(property));
                custom = true;
            }

            if (customTagFormatters != null && customTagFormatters.TryGetValue((property.Name, property.PropertyType), out cf))
            {
                tagFormatters.Add(cf(property));
                custom = true;
            }

            return custom;
        }
    }
}