using System;
using System.Collections.Concurrent;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal class InfluxLineFormatterCollection
    {
        private readonly string _listenerName;
        private readonly Func<string, string> _nameFixer;

        private readonly ConcurrentDictionary<(string, Type), InfluxLineFormatter> _formatters =
            new ConcurrentDictionary<(string, Type), InfluxLineFormatter>();

        public InfluxLineFormatterCollection(string listenerName, Func<string, string> nameFixer)
        {
            _listenerName = listenerName;
            _nameFixer = nameFixer ?? (s => s);
        }

        public InfluxLineFormatter GetOrAdd(string measurement, Type argsType)
        {
            return _formatters.GetOrAdd((measurement, argsType), Create);
        }

        private InfluxLineFormatter Create((string measurementName, Type type) args) =>
            new InfluxLineFormatter(_nameFixer($"{_listenerName}.{args.measurementName}"), args.type);
    }
}