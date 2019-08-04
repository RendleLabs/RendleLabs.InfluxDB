using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    /// <summary>
    /// Observer for <see cref="DiagnosticSource"/> that writes to InfluxDB.
    /// </summary>
    internal sealed class DiagnosticListenerObserver : IObserver<KeyValuePair<string, object>>, IDisposable
    {
        private readonly InfluxLineWriterCollection _writers;
        private readonly DiagnosticListener _listener;
        private readonly IInfluxDBClient _client;
        private readonly Action<DiagnosticListener, Exception>? _onError;
        private readonly IDisposable _subscription;
        private (Type type, InfluxLineWriter formatter) _last;

        internal DiagnosticListenerObserver(DiagnosticListener listener, IInfluxDBClient client,
            DiagnosticListenerOptions options)
        {
            _listener = listener;
            _client = client;
            _writers = new InfluxLineWriterCollection(listener.Name, options);
            _onError = options.OnError;
            _subscription = listener.Subscribe(this);
        }

        public void OnCompleted() => Dispose();

        public void OnError(Exception error)
        {
            _onError?.Invoke(_listener, error);
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (string.IsNullOrEmpty(value.Key)) return;
            if (value.Value == null) return;
            Write(value.Key, value.Value);
        }

        private void Write(string name, object args)
        {
            if (args != null)
            {
                var argsType = args.GetType();
                var last = _last;
                if (last != default)
                {
                    if (last.type == argsType)
                    {
                        _client.TryRequest(new WriteRequest(last.formatter, args, Activity.Current));
                        return;
                    }
                }
                var formatter = _writers.GetOrAdd(name, argsType);
                _last = (argsType, formatter);
                _client.TryRequest(new WriteRequest(formatter, args, Activity.Current));
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}