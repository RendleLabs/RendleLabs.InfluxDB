using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    internal class InfluxDBHttpClient : IInfluxDBHttpClient
    {
        private static readonly ConcurrentDictionary<Uri, InfluxDBHttpClient> Clients = new ConcurrentDictionary<Uri, InfluxDBHttpClient>();
        private readonly HttpClient _client;

        private InfluxDBHttpClient(Uri serverUri)
        {
            _client = new HttpClient
            {
                BaseAddress = serverUri
            };
        }

        public async Task Write(byte[] data, int size, string path)
        {
            var content = new ByteArrayContent(data, 0, size);
            await _client.PostAsync(path, content).ConfigureAwait(false);
        }

        public static InfluxDBHttpClient Get(string serverUrl) => Get(new Uri(serverUrl));

        public static InfluxDBHttpClient Get(Uri serverUri)
        {
            return Clients.GetOrAdd(serverUri, uri => new InfluxDBHttpClient(uri));
        }
    }
}