using System;
using System.Text;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    /// <summary>
    /// Provides methods to escape special characters in InfluxDB field names.
    /// </summary>
    internal static class InfluxName
    {
        private const byte Backslash = 92;
        
        internal static byte[] Escape(string name)
        {
            char[] chars = name.ToCharArray();
            int byteCount = Encoding.UTF8.GetByteCount(chars);
            byte[] bytes = new byte[byteCount * 2];
            int index = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                switch (chars[i])
                {
                    case ' ':
                    case ',':
                    case '=':
                        bytes[index++] = Backslash;
                        break;
                }

                var characterBytes = Encoding.UTF8.GetBytes(chars, i, 1, bytes, index);
                index += characterBytes;
            }
            
            Array.Resize(ref bytes, index);
            return bytes;
        }
    }
}