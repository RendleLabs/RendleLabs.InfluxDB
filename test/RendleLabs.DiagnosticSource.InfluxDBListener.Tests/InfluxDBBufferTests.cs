using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using RendleLabs.InfluxDB;
using Xunit;

namespace RendleLabs.DiagnosticSource.InfluxDBListener.Tests
{
    public class InfluxDBBufferTests
    {
        [Fact]
        public void InfluxLineFormatterWritesToBuffer()
        {
            var mockHttp = new MockHttpClient();
            var client = new InfluxDBClientBuilder(mockHttp, "test").Build();
            DiagnosticSourceInfluxDB.Listen(client, s => s == "tests");

            var source = new DiagnosticListener("tests");
            if (source.IsEnabled("test"))
            {
                var obj = new {size = 100, duration = 1000, url = "test.com", id = "42"};
                source.Write("test", obj);
            }
            
            // Forces the client to complete outstanding requests
            ((InfluxDBClient)client).Dispose();
            
            Assert.Equal("write?db=test", mockHttp.Path);
            Assert.NotNull(mockHttp.Bytes);
            Assert.StartsWith("tests_test", mockHttp.Text);
            Assert.Contains("size=100", mockHttp.Text);
            Assert.Contains("duration=1000", mockHttp.Text);
            Assert.Contains("url=test.com", mockHttp.Text);
            Assert.Contains("id=42", mockHttp.Text);
        }
    }
}