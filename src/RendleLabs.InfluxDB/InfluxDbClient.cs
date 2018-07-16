using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    public class InfluxDbClient
    {
        private readonly HttpClient _client;

        public InfluxDbClient(string serverUrl, string database)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(serverUrl)
            };
        }

        public async Task Write(ReadOnlySequence<byte> data)
        {
            var request = new HttpRequestMessage();
            request.Content = CreateContent(data);
        }

        private static HttpContent CreateContent(ReadOnlySequence<byte> data) => data.Length > int.MaxValue
            ? (HttpContent) CreateStreamContent(data)
            : CreateByteArrayContent(data);

        private static StreamContent CreateStreamContent(ReadOnlySequence<byte> data)
        {
            var stream = new MemoryStream(int.MaxValue);
            foreach (var buffer in data)
            {
                var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
                try
                {
                    buffer.Span.CopyTo(rented.AsSpan());
                    stream.Write(rented, 0, buffer.Length);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }

            return new StreamContent(stream);
        }

        private static ByteArrayContent CreateByteArrayContent(ReadOnlySequence<byte> data)
        {
            int length = (int) data.Length;
            var bytes = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                data.CopyTo(bytes.AsSpan());
                var content = new ByteArrayContent(bytes);
                content.Headers.ContentLength = data.Length;
                return content;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }
    }
}