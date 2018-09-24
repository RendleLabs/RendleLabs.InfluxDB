using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RendleLabs.InfluxDB.DiagnosticSourceListener;
using Xunit;

namespace RendleLabs.InfluxDB.IntegrationTests
{
    public class EndToEnd
    {
        private static readonly DiagnosticSource Source = new DiagnosticListener(typeof(EndToEnd).FullName);
        
        [Fact]
        public async Task WritesToInflux()
        {
            var random = new Random(42);
            using (var client = new InfluxDBClientBuilder("http://localhost:8086", "carbon-dev")
                .ForceFlushInterval(TimeSpan.FromSeconds(1))
                .Build())
            using (DiagnosticSourceInfluxDB.Listen(client, _ => true))
            {
                for (int i = 0; i < 1024; i++)
                {
                    Source.Write("test", new { value = random.Next(64), source = $"id{random.Next(8)}"});
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            
            Assert.True(true);
        }
    }
}
