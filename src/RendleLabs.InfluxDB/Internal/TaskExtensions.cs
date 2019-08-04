using System;
using System.Net;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB.Internal
{
    public static class TaskExtensions
    {
        public static Task Append(this Task previous, Task next)
        {
            return previous.IsCompleted ? next : Await(previous, next);

            static async Task Await(Task a, Task b)
            {
                await a.ConfigureAwait(false);
                await b.ConfigureAwait(false);
            }
        }
    }
}