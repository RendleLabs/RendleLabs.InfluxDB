using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    public static class FireAndForgetExtension
    {
        public static async void FireAndForget(this Task task, Action<Exception> errorCallback = null) =>
            task.ConfigureAwait(false).FireAndForget(errorCallback);

        public static async void FireAndForget(this ConfiguredTaskAwaitable awaitable, Action<Exception> errorCallback = null)
        {
            try
            {
                await awaitable;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e);
            }
        }
    }
}