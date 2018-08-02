using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    /// <summary>
    /// Listens on <see cref="DiagnosticListener.AllListeners"/> and subscribes to relevant sources.
    /// </summary>
    public sealed class DiagnosticSourceInfluxDB : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly Func<string, bool> _sourceNamePredicate;
        private readonly IInfluxDBClient _client;
        private readonly ConcurrentBag<DiagnosticListenerObserver> _observers = new ConcurrentBag<DiagnosticListenerObserver>();
        private readonly DiagnosticListenerOptions _listenerOptions;

        private DiagnosticSourceInfluxDB(IInfluxDBClient client, Func<string, bool> sourceNamePredicate, Action<DiagnosticListenerOptions> optionsCallback)
        {
            _sourceNamePredicate = sourceNamePredicate;
            _client = client;
            _listenerOptions = new DiagnosticListenerOptions();
            optionsCallback?.Invoke(_listenerOptions);
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
                _observers.Add(new DiagnosticListenerObserver(value, _client, _listenerOptions));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose of all subscriptions.
        /// </summary>
        [SuppressMessage("ReSharper", "ERP022")]
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
        /// <param name="optionsCallback">Configuration callback</param>
        public static IDisposable Listen(IInfluxDBClient client, Func<string, bool> sourceNamePredicate, Action<DiagnosticListenerOptions> optionsCallback = null)
        {
            var listener = new DiagnosticSourceInfluxDB(client, sourceNamePredicate, optionsCallback);
            DiagnosticListener.AllListeners.Subscribe(listener);
            return listener;
        }
    }

}
