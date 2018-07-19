using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    public sealed class DiagnosticListenerSubscription : IObserver<KeyValuePair<string, object>>, IDisposable
    {
        private readonly InfluxLineFormatterCollection _formatters;
        private readonly DiagnosticListener _listener;
        private readonly PipeWriter _pipeWriter;
        private readonly Action<DiagnosticListener, Exception> _onError;
        private readonly IDisposable _subscription;

        public DiagnosticListenerSubscription(DiagnosticListener listener, PipeWriter pipeWriter, Func<string, string> nameFixer = null, Action<DiagnosticListener, Exception> onError = null)
        {
            _listener = listener;
            _formatters = new InfluxLineFormatterCollection(listener.Name, nameFixer);
            _pipeWriter = pipeWriter;
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
        }

        private void Write(string name, object args)
        {
            var formatter = _formatters.GetOrAdd(name, args.GetType());
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}