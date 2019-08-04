using System;
using System.Threading.Tasks;
using RendleLabs.InfluxDB.Internal;
using Xunit;

namespace RendleLabs.InfluxDB.Tests
{
    public class TaskExtensionTests
    {
        [Fact]
        public async Task AppendCombinesTasks()
        {
            var t1 = Async();
            var t2 = Async();
            var c = t1.Append(t2);
            await c;
            Assert.True(t1.IsCompletedSuccessfully && t2.IsCompletedSuccessfully);
        }

        private static async Task Async()
        {
            await Task.Delay(100);
        }
    }
}
