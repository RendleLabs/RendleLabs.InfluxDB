using System;
using System.Net;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB.Internal
{
    public static class TaskExtensions
    {
        public static ValueTask<T> Append<T>(this ValueTask<T> previous, ValueTask<T> next, bool suppressErrors)
        {
            if (previous.IsCompleted && (suppressErrors || previous.IsCompletedSuccessfully)) return next;
            
            return suppressErrors ? AwaitSuppress(previous, next) : Await(previous, next);
            
            async ValueTask<T> AwaitSuppress(ValueTask<T> p, ValueTask<T> n)
            {
                try { await p.ConfigureAwait(false); } catch { }

                try
                {
                    return await n.ConfigureAwait(false);
                }
                catch
                {
                    return default;
                }
            }
            
            async ValueTask<T> Await(ValueTask<T> p, ValueTask<T> n)
            {
                await p.ConfigureAwait(false);
                return await n.ConfigureAwait(false);
            }
        }
        
        public static ValueTask Append(this ValueTask previous, ValueTask next) => Append(previous, next, false);

        public static ValueTask Append(this ValueTask previous, ValueTask next, bool suppressErrors)
        {
            if (previous.IsCompleted && (suppressErrors || previous.IsCompletedSuccessfully)) return next;
            
            return suppressErrors ? AwaitSuppress(previous, next) : Await(previous, next);
            
            async ValueTask AwaitSuppress(ValueTask p, ValueTask n)
            {
                try { await p.ConfigureAwait(false); } catch { }
                try { await n.ConfigureAwait(false); } catch { }
            }
            
            async ValueTask Await(ValueTask p, ValueTask n)
            {
                await p.ConfigureAwait(false);
                await n.ConfigureAwait(false);
            }
        }
    }
}