using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    internal class InfluxHttpContent : HttpContent
    {
        private readonly LineCollection _lines;

        public InfluxHttpContent(LineCollection lines)
        {
            _lines = lines ?? throw new ArgumentNullException(nameof(lines));
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            foreach (var line in _lines)
            {
                await stream.WriteAsync(line.Bytes, 0, line.Length);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _lines.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            _lines.Dispose();
        }
    }
}