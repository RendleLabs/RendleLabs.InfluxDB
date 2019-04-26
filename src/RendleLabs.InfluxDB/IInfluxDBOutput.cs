namespace RendleLabs.InfluxDB
{
    internal interface IInfluxDBOutput
    {
        void Write(byte[] buffer, int size);
        void Finish();
    }
}