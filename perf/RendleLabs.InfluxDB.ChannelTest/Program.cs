using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace RendleLabs.InfluxDB.ChannelTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                var benchmark = new ChannelBenchmark();
                benchmark.WithChannel();
                benchmark.WithClient();
                return;
            }
            
            var summary = BenchmarkRunner.Run<ChannelBenchmark>();
            Debug.Assert(summary != null);
        }
    }
}
