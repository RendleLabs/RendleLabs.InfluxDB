using System.Diagnostics;
using BenchmarkDotNet.Running;

namespace RendleLabs.InfluxDB.Performance
{
    public static class Program
    {
        public static void Main()
        {
            var summary = BenchmarkRunner.Run<FormatterBenchmark>();
            Debug.Assert(summary != null);
        }
    }
}
