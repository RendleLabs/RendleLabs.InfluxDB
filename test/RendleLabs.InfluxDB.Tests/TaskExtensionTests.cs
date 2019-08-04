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
            var t1 = AsyncInt(1);
            var t2 = AsyncInt(2);
            var c = t1.Append(t2, false);
            await c;
            Assert.True(t1.IsCompletedSuccessfully && t2.IsCompletedSuccessfully);
        }

        private static async ValueTask<int> AsyncInt(int value)
        {
            await Task.Delay(100);
            return value;
        }
    }
}
