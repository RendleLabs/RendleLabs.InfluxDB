using System;
using System.Threading;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    internal class DefaultTags
    {
        private byte[] _defaultTags = Array.Empty<byte>();

        public byte[] Bytes => _defaultTags;
        
        public void Add(string name, string value)
        {
            var nameBytes = InfluxName.Escape(name);
            var valueBytes = InfluxName.Escape(value);
            
            var offset = _defaultTags.Length;
            var size = offset + nameBytes.Length + valueBytes.Length + 2;
            var newArray = new byte[size];
            _defaultTags.CopyTo(newArray, 0);
            
            newArray[offset] = (byte) ',';
            offset++;
            nameBytes.CopyTo(newArray, offset);

            offset += nameBytes.Length;
            newArray[offset] = (byte) '=';
            offset++;
            
            valueBytes.CopyTo(newArray, offset);

            Interlocked.Exchange(ref _defaultTags, newArray);
        }
    }
}