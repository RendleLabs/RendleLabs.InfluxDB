using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    /// <summary>
    /// Listens on <see cref="DiagnosticListener.AllListeners"/> and subscribes to relevant sources.
    /// </summary>
    public sealed class DiagnosticSourceInfluxDB : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly Func<string, bool> _sourceNamePredicate;
        private readonly IInfluxDBClient _client;
        private readonly Func<string, string> _nameFixer;
        private readonly ConcurrentBag<DiagnosticListenerObserver> _observers = new ConcurrentBag<DiagnosticListenerObserver>();

        private DiagnosticSourceInfluxDB(IInfluxDBClient client, Func<string, bool> sourceNamePredicate, Func<string, string> nameFixer = null)
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
                _observers.Add(new DiagnosticListenerObserver(value, _client, _nameFixer));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose of all subscriptions.
        /// </summary>
        public void Dispose()
        {
            while (_observers.TryTake(out var subscription))
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

        /// <summary>
        /// Add a listener on sources that match a given Name predicate.
        /// </summary>
        /// <param name="client">The <see cref="IInfluxDBClient"/> to write data to.</param>
        /// <param name="sourceNamePredicate">A predicate to match <see cref="DiagnosticSource"/> names.</param>
        /// <param name="nameFixer">(Optional) A function to mutate names, e.g. to convert dotted names to underscored.</param>
        public static IDisposable Listen(IInfluxDBClient client, Func<string, bool> sourceNamePredicate, Func<string, string> nameFixer = null)
        {
            var listener = new DiagnosticSourceInfluxDB(client, sourceNamePredicate, nameFixer);
            DiagnosticListener.AllListeners.Subscribe(listener);
            return listener;
        }
    }

}
