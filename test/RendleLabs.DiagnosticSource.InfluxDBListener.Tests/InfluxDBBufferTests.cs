using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using RendleLabs.InfluxDB;
using Xunit;

namespace RendleLabs.DiagnosticSource.InfluxDBListener.Tests
{
    public class InfluxDBBufferTests
    {
        [Fact]
        public async Task InfluxLineFormatterWritesToBuffer()
        {
            var client = new MockClient();
            var buffer = InfluxDBBuffer.Create(client, "testdb", "testrp");
            var obj = new {size = 100, duration = 1000, url = "test.com", id = "42"};
            var formatter = new InfluxLineFormatter("test", );
        }
    }

    internal class MockClient : IInfluxDBClient
    {
        public string Text { get; private set; }
        public byte[] Bytes { get; private set; }
        public string Path { get; private set; }
        
        public Task Write(byte[] data, int size, string path)
        {
            Bytes = new byte[size];
            Array.Copy(data, Bytes, size);

            Text = Encoding.UTF8.GetString(data, 0, size);
            Path = path;
            return Task.CompletedTask;
        }
    }
}