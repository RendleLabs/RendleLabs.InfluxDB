using System;
using System.Text;
using System.Threading.Tasks;
using RendleLabs.InfluxDB;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    internal class MockHttpClient : IInfluxDBHttpClient
    {
        public string Text { get; private set; }
        public byte[] Bytes { get; private set; }
        public string Path { get; private set; }
        
        public Task Write(byte[] data, int size, string path)
        {
            Bytes = new byte[size];
            Array.Copy(data, Bytes, size);

            Text = Encoding.UTF8.GetString(data, 0, size);
            Path = path;
            return Task.CompletedTask;
        }
    }
}