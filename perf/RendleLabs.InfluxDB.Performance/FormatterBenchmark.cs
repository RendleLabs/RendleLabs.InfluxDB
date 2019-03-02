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
        private static readonly InfluxLineFormatterCollection Formatters = new InfluxLineFormatterCollection("perf", new DiagnosticListenerOptions());
        private static readonly byte[] Buffer = new byte[8192];
        private Args[] _args128;
        private Args[] _args1024;

        [Benchmark]
        public int[] LineFormatter128()
        {
            var values = new int[_args128.Length];

            for (int i = 0; i < _args128.Length; i++)
            {
                var formatter = Formatters.GetOrAdd("perf", _args128[i].GetType());
                formatter.TryWrite(Buffer.AsSpan(), _args128[i], null, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), out int written);
                values[i] = written;
            }

            return values;
        }

        [Benchmark]
        public PointData[] PointData128()
        {
            var values = new PointData[_args128.Length];
            for (int i = 0; i < _args128.Length; i++)
            {
                var reflector = ReflectorCollection.GetOrAdd("perf", _args128[i].GetType());
                var fields = new Dictionary<string, object>();
                var tags = new Dictionary<string, string>();
                reflector.Reflect(_args128[i], fields, tags);
                values[i] = new PointData("perf", fields, tags, DateTime.UtcNow);
            }

            return values;
        }

        [Benchmark]
        public int[] LineFormatter1024()
        {
            var values = new int[_args1024.Length];

            for (int i = 0; i < _args1024.Length; i++)
            {
                var formatter = Formatters.GetOrAdd("perf", _args1024[i].GetType());
                formatter.TryWrite(Buffer.AsSpan(), _args1024[i], null, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), out int written);
                values[i] = written;
            }

            return values;
        }

        [Benchmark]
        public PointData[] PointData1024()
        {
            var values = new PointData[_args1024.Length];
            for (int i = 0; i < _args1024.Length; i++)
            {
                var reflector = ReflectorCollection.GetOrAdd("perf", _args1024[i].GetType());
                var fields = new Dictionary<string, object>();
                var tags = new Dictionary<string, string>();
                reflector.Reflect(_args1024[i], fields, tags);
                values[i] = new PointData("perf", fields, tags, DateTime.UtcNow);
            }

            return values;
        }
        [GlobalSetup]
        public void GlobalSetup()
        {
            _args128 = GenerateArgs(128);
            _args1024 = GenerateArgs(1024);
        }

        private static Args[] GenerateArgs(int howMany)
        {
            var random = new Random(42);
            return Enumerable.Range(0, howMany).Select(i => new Args {value = random.NextDouble(), tag = $"benchmark_{i}"}).ToArray();
        }
    }

    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class Args
    {
        public double value { get; set; }
        public string tag { get; set; }
    }
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}