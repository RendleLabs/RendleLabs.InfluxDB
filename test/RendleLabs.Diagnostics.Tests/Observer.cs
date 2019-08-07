using System;
using System.Collections.Generic;

namespace RendleLabs.Diagnostics.Tests
{
    internal class Observer : IObserver<KeyValuePair<string, object>>
    {
        private readonly Action<string, object> _onNext;
        private readonly Action _onCompleted;
        private readonly Action<Exception> _onError;

        public Observer(Action<string, object> onNext, Action onCompleted = null, Action<Exception> onError = null)
        {
            _onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
            _onCompleted = onCompleted;
            _onError = onError;
        }

        public void OnCompleted()
        {
            _onCompleted?.Invoke();
        }

        public void OnError(Exception error)
        {
            _onError?.Invoke(error);
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            _onNext(value.Key, value.Value);
        }
    }
}