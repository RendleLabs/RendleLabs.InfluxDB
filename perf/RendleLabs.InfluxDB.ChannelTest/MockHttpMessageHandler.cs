using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB.ChannelTest
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private static readonly MemoryStream Buffer = new MemoryStream(new byte[128 * 1024]);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Buffer.Position = 0;
            await request.Content.CopyToAsync(Buffer);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}