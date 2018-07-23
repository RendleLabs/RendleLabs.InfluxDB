using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RendleLabs.InfluxDB;

namespace RendleLabs.DiagnosticSource.InfluxDBListener
{
    public sealed class DiagnosticSourceInfluxDB : IObserver<DiagnosticListener>, IDisposable
    {
        private static readonly ConcurrentBag<IDisposable> AllListenersListeners = new ConcurrentBag<IDisposable>();
        private readonly Func<string, bool> _sourceNamePredicate;
        private readonly IInfluxDBClient _client;
        private readonly Func<string, string> _nameFixer;
        private readonly List<DiagnosticListenerSubscription> _subscriptions = new List<DiagnosticListenerSubscription>();

        private DiagnosticSourceInfluxDB(Func<string, bool> sourceNamePredicate, IInfluxDBClient client, Func<string, string> nameFixer = null)
        {
            _sourceNamePredicate = sourceNamePredicate;
            _client = client;
            _nameFixer = nameFixer ?? (name => name.Replace('.', '_'));
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
                _subscriptions.Add(new DiagnosticListenerSubscription(value, _client, _nameFixer));
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

        public static void Listen(IInfluxDBClient client, Func<string, bool> sourceNamePredicate, Func<string, string> nameFixer = null)
        {
            var listener = new DiagnosticSourceInfluxDB(sourceNamePredicate, client, nameFixer);
            DiagnosticListener.AllListeners.Subscribe(listener);
        }
    }

}
