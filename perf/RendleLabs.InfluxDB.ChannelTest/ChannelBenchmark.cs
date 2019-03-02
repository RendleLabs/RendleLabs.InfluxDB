using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RendleLabs.InfluxDB.DiagnosticSourceListener;

namespace RendleLabs.InfluxDB.ChannelTest
{
    [MemoryDiagnoser]
    public class ChannelBenchmark
    {
        private static readonly InfluxLineFormatter Formatter = 
            new InfluxLineFormatter("metric", typeof(Metric), null, null, Encoding.UTF8.GetBytes(",default_tag=quux"));
        private static readonly Metric[] Metrics = GenerateMetrics().ToArray();
        
        [Benchmark]
        public void WithChannel()
        {
            var httpClient = new HttpClient(new MockHttpMessageHandler()) {BaseAddress = new Uri("http://localhost:8086")};
            var influxHttpClient = new InfluxDBHttpClient(httpClient);
            using (var channel = InfluxDBChannel.Create(influxHttpClient, "foo", "autogen", _ => { }))
            {
                int size = 256;
                foreach (var metric in Metrics)
                {
                    var buffer = channel.GetBuffer(size);
                    int bytesWritten;
                    while (!Formatter.TryWrite(buffer.Span, metric, null, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), out bytesWritten))
                    {
                        size = buffer.Grow();
                    }
                    channel.TryAdd(buffer, bytesWritten);
                }
            }
        }
        
        [Benchmark]
        public void WithClient()
        {
            var httpClient = new HttpClient(new MockHttpMessageHandler()) {BaseAddress = new Uri("http://localhost:8086")};
            var influxHttpClient = new InfluxDBHttpClient(httpClient);
            using (var client = InfluxDBClient.Create(influxHttpClient, "foo", "autogen", _ => { }, 8192, 8192 * 32, null))
            {
                foreach (var metric in Metrics)
                {
                    client.TryRequest(new WriteRequest(Formatter, metric));
                }
            }
        }

        private static IEnumerable<Metric> GenerateMetrics(int count = 256 * 256)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new Metric {Value = i, Tag = Guid.NewGuid().ToString() };
            }
        }
    }
}