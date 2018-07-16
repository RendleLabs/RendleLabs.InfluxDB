using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    public sealed class SourcesListener : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly Func<string, bool> _sourceNamePredicate;
        private readonly List<DiagnosticListenerSubscription> _subscriptions = new List<DiagnosticListenerSubscription>();

        public SourcesListener(Func<string, bool> sourceNamePredicate)
        {
            _sourceNamePredicate = sourceNamePredicate;
        }

        public void OnCompleted()
        {
            Dispose();
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (_sourceNamePredicate(value.Name))
            {
                _subscriptions.Add(new DiagnosticListenerSubscription(value));
            }
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                try
                {
                    subscription.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
    
    public sealed class DiagnosticListenerSubscription : IObserver<KeyValuePair<string, object>>, IDisposable
    {
        private readonly DiagnosticListener _listener;
        private readonly Action<DiagnosticListener, Exception> _onError;
        private readonly IDisposable _subscription;

        public DiagnosticListenerSubscription(DiagnosticListener listener, Action<DiagnosticListener, Exception> onError = null)
        {
            _listener = listener;
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
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
