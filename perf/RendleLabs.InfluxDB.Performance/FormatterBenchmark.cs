using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using InfluxDB.Collector.Pipeline;
using RendleLabs.InfluxDB.DiagnosticSourceListener;

namespace RendleLabs.InfluxDB.Performance
{
    [MemoryDiagnoser]
    public class FormatterBenchmark
    {
        private static readonly InfluxLineFormatterCollection Formatters = new InfluxLineFormatterCollection("perf", s => s);
        private static readonly byte[] Buffer = new byte[1024];
        private Args[] args;

        [Benchmark]
        public int[] LineFormatter()
        {
            var values = new int[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                var formatter = Formatters.GetOrAdd("perf", args[i].GetType());
                formatter.TryWrite(Buffer.AsSpan(), args[i], DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), out int written);
                values[i] = written;
            }

            return values;
        }

        [Benchmark]
        public PointData[] PointData()
        {
            var values = new PointData[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var reflector = ReflectorCollection.GetOrAdd("perf", args[i].GetType());
                var fields = new Dictionary<string, object>();
                var tags = new Dictionary<string, string>();
                reflector.Reflect(args[i], fields, tags);
                values[i] = new PointData("perf", fields, tags, DateTime.UtcNow);
            }

            return values;
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            args = GenerateArgs();
        }

        private static Args[] GenerateArgs()
        {
            var random = new Random(42);
            return Enumerable.Range(0, 128).Select(i => new Args {value = random.NextDouble(), tag = $"benchmark_{i}"}).ToArray();
        }
    }

    public class Args
    {
        public double value { get; set; }
        public string tag { get; set; }
    }
}