using System;
using System.Linq;
using System.Threading;
using RendleLabs.InfluxDB.DiagnosticSourceListener;

namespace RendleLabs.InfluxDB.Allocations
{
    class Program
    {
        static void Main()
        {
            var args = GenerateArgs(128);
            var result = Runner.LineFormatter128(args);
            Console.WriteLine(result.Length);
            Console.WriteLine("Press ENTER");
            Console.ReadLine();
        }

        private static object[] GenerateArgs(int howMany)
        {
            var random = new Random(42);
            return Enumerable.Range(0, howMany).Select(i => new {value = random.NextDouble(), tag = $"benchmark_{i}"}).ToArray();
        }
    }

    class Runner
    {
        private static readonly InfluxLineFormatterCollection Formatters = new InfluxLineFormatterCollection("perf", new DiagnosticListenerOptions());
        private static readonly byte[] Buffer = new byte[8192];

        public static int[] LineFormatter128(object[] args128)
        {
            var values = new int[args128.Length];

            for (int i = 0; i < args128.Length; i++)
            {
                var formatter = Formatters.GetOrAdd("perf", args128[i].GetType());
                formatter.TryWrite(Buffer.AsSpan(), args128[i], null, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), out int written);
                values[i] = written;
            }

            return values;
        }
    }
}
