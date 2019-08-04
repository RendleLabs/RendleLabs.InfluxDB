using System;
using System.Collections.Concurrent;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal class InfluxLineWriterCollection
    {
        private readonly string _listenerName;
        private readonly DiagnosticListenerOptions _options;
        private readonly Func<string, string> _nameFixer;

        private readonly ConcurrentDictionary<(string, Type), InfluxLineWriter> _writers =
            new ConcurrentDictionary<(string, Type), InfluxLineWriter>();

        public InfluxLineWriterCollection(string listenerName, DiagnosticListenerOptions options)
        {
            _listenerName = listenerName;
            _options = options;
            _nameFixer = options.NameFixer ?? NameFixer.Default;
        }

        public InfluxLineWriter GetOrAdd(string measurement, Type argsType)
        {
            return _writers.GetOrAdd((measurement, argsType), Create);
        }

        private InfluxLineWriter Create((string measurementName, Type type) args) =>
            new InfluxLineWriter(_nameFixer($"{_listenerName}_{args.measurementName}"), args.type, _options);
    }
}