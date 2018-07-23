namespace RendleLabs.InfluxDB
{
    public interface IInfluxDBClient
    {
        bool TryRequest(WriteRequest request);
    }
}