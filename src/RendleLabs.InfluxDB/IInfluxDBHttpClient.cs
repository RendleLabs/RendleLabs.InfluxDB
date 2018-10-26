using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    internal interface IInfluxDBHttpClient : IDisposable
    {
        Task Write(byte[] data, int size, string path);
        Task Write(HttpContent content, string path);
    }
}