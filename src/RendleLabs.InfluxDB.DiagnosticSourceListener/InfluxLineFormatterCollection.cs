using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal class InfluxLineFormatterCollection
    {
        private readonly string _listenerName;
        private readonly DiagnosticListenerOptions _options;
        private readonly Func<string, string> _nameFixer;

        private readonly ConcurrentDictionary<(string, Type), InfluxLineFormatter> _formatters =
            new ConcurrentDictionary<(string, Type), InfluxLineFormatter>();

        public InfluxLineFormatterCollection(string listenerName, DiagnosticListenerOptions options)
        {
            _listenerName = listenerName;
            _options = options;
            _nameFixer = options.NameFixer ?? NameFixer.Default;
        }

        public InfluxLineFormatter GetOrAdd(string measurement, Type argsType)
        {
            return _formatters.GetOrAdd((measurement, argsType), Create);
        }

        private InfluxLineFormatter Create((string measurementName, Type type) args) =>
            new InfluxLineFormatter(_nameFixer($"{_listenerName}_{args.measurementName}"), args.type,
                _options.CustomFieldFormatters, _options.CustomTagFormatters, _options.DefaultTags);
    }
}