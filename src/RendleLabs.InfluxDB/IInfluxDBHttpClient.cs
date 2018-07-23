using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    internal interface IInfluxDBHttpClient
    {
        Task Write(byte[] data, int size, string path);
    }
}