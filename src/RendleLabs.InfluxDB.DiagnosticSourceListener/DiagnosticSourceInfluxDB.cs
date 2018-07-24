using System;
using System.Collections.Generic;
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
        private readonly List<DiagnosticListenerObserver> _observers = new List<DiagnosticListenerObserver>();

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
                _observers.Add(new DiagnosticListenerObserver(value, _client, _nameFixer));
            }
        }

        /// <summary>
        /// Dispose of all subscriptions.
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in _observers)
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
        public static void Listen(IInfluxDBClient client, Func<string, bool> sourceNamePredicate, Func<string, string> nameFixer = null)
        {
            var listener = new DiagnosticSourceInfluxDB(sourceNamePredicate, client, nameFixer);
            DiagnosticListener.AllListeners.Subscribe(listener);
        }
    }

}
