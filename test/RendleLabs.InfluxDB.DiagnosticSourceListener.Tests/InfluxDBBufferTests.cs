using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    public class InfluxDBBufferTests
    {
        [Fact]
        public async Task InfluxLineFormatterWritesToBuffer()
        {
            var mockHttp = new MockHttpClient();
            var client = new InfluxDBClientBuilder(mockHttp, "test").Build();
            DiagnosticSourceInfluxDB.Listen(client, s => s == "tests");

            var source = new DiagnosticListener("tests");
            if (source.IsEnabled("test"))
            {
                var obj = new {size = 100, duration = 1000, url = "test.com", id = "42"};
                source.Write("test", obj);
                
                // Seems like we have to wait for DiagnosticSource to actually complete.
                await Task.Delay(16);
            }
            
            // Forces the client to complete outstanding requests
            ((InfluxDBClient)client).Flush();
            
            Assert.Equal("write?db=test&precision=ms", mockHttp.Path);
            Assert.NotNull(mockHttp.Bytes);
            Assert.StartsWith("tests_test", mockHttp.Text);
            Assert.Contains("size=100", mockHttp.Text);
            Assert.Contains("duration=1000", mockHttp.Text);
            Assert.Contains("url=test.com", mockHttp.Text);
            Assert.Contains("id=42", mockHttp.Text);
        }
    }
}