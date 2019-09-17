using System.Diagnostics;
using Microsoft.Extensions.ObjectPool;

namespace RendleLabs.Diagnostics.Internal
{
    internal class DisposableActivity : IDisposableActivity
    {
        private ObjectPool<DisposableActivity>? _pool;
        private Activity? _activity;
        private DiagnosticSource? _source;
        private object? _stopArgs;

        internal DisposableActivity Initialize(ObjectPool<DisposableActivity> pool, Activity activity, DiagnosticSource source)
        {
            _pool = pool;
            _activity = activity;
            _source = source;
            return this;
        }

        public IDisposableActivity? IfEnabled(string name) => _source!.IsEnabled(name) ? this : null;

        public IDisposableActivity AddTag(string key, string value)
        {
            _activity!.AddTag(key, value);
            return this;
        }

        public IDisposableActivity AddBaggage(string key, string value)
        {
            _activity!.AddBaggage(key, value);
            return this;
        }

        public void SetStopArgs(object? args)
        {
            _stopArgs = args;
        }

        public void Dispose()
        {
            _source!.StopActivity(_activity, _stopArgs);
            Reset();
            _pool!.Return(this);
        }

        private void Reset()
        {
            _activity = null;
            _stopArgs = null;
            _source = null;
        }
    }
}