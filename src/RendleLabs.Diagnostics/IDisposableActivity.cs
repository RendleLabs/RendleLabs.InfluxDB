namespace RendleLabs.Diagnostics
{
    public interface IDisposableActivity
    {
        IDisposableActivity? IfEnabled(string name);
        IDisposableActivity AddTag(string key, string value);
        IDisposableActivity AddBaggage(string key, string value);
        void SetStopArgs(object args);
        void Dispose();
    }
}