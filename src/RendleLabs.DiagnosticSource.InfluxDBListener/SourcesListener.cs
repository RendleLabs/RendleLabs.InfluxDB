using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    public sealed class SourcesListener : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly Func<string, bool> _sourceNamePredicate;
        private readonly Func<string, string> _nameFixer;
        private readonly PipeWriter _writer;
        private readonly List<DiagnosticListenerSubscription> _subscriptions = new List<DiagnosticListenerSubscription>();

        public SourcesListener(Func<string, bool> sourceNamePredicate, PipeWriter writer, Func<string, string> nameFixer = null)
        {
            _sourceNamePredicate = sourceNamePredicate;
            _writer = writer;
            _nameFixer = nameFixer;
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
                _subscriptions.Add(new DiagnosticListenerSubscription(value, _writer));
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
}
