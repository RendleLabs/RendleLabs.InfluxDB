namespace RendleLabs.Diagnostics.Internal
{
    internal class NullDisposableActivity : IDisposableActivity
    {
        public IDisposableActivity IfEnabled(string name) => this;

        public IDisposableActivity AddTag(string key, string value) => this;

        public IDisposableActivity AddBaggage(string key, string value) => this;

        public void SetStopArgs(object args)
        {
        }

        public void Dispose()
        {
        }
    }
}