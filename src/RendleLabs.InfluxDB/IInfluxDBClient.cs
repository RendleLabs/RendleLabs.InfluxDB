using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    public interface IInfluxDBClient
    {
        Task Write(byte[] data, int size, string path);
    }
}