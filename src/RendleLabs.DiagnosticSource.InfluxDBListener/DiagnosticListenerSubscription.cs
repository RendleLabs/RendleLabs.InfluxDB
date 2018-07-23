using System;
using System.Collections.Generic;
using System.Diagnostics;
using RendleLabs.InfluxDB;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    public sealed class DiagnosticListenerSubscription : IObserver<KeyValuePair<string, object>>, IDisposable
    {
        private readonly InfluxLineFormatterCollection _formatters;
        private readonly DiagnosticListener _listener;
        private readonly IInfluxDBClient _client;
        private readonly Action<DiagnosticListener, Exception> _onError;
        private readonly IDisposable _subscription;

        public DiagnosticListenerSubscription(DiagnosticListener listener, IInfluxDBClient client, Func<string, string> nameFixer = null, Action<DiagnosticListener, Exception> onError = null)
        {
            _listener = listener;
            _client = client;
            _formatters = new InfluxLineFormatterCollection(listener.Name, nameFixer);
            _onError = onError;
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
            var formatter = _formatters.GetOrAdd(name, args.GetType());
            _client.TryRequest(new WriteRequest(formatter, args));
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}