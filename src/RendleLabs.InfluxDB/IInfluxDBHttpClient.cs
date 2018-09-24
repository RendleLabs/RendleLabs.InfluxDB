using System;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    internal interface IInfluxDBHttpClient : IDisposable
    {
        Task Write(byte[] data, int size, string path);
    }
}