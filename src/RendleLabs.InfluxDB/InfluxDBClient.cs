using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    public class InfluxDBClient : IInfluxDBClient
    {
        private static readonly ConcurrentDictionary<Uri, InfluxDBClient> _clients = new ConcurrentDictionary<Uri, InfluxDBClient>();
        private readonly HttpClient _client;

        internal InfluxDBClient(Uri serverUri)
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

        public static InfluxDBClient Get(string serverUrl) => Get(new Uri(serverUrl));

        public static InfluxDBClient Get(Uri serverUri)
        {
            return _clients.GetOrAdd(serverUri, uri => new InfluxDBClient(uri));
        }
    }
}