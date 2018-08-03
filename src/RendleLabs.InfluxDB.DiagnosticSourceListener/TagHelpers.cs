using System;
using System.Buffers.Text;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    public static class TagHelpers
    {
        private const byte Comma = (byte) ',';
        private const byte EqualSign = (byte) '=';
        
        public static bool TryWriteString(Span<byte> name, string value, Span<byte> target, out int bytesWritten)
        {
            if (!TryWriteName(name, ref target, out bytesWritten)) return false;

            if (value.TryWriteEscapedUTF8(target, out bytesWritten))
            {
                bytesWritten += name.Length + 2;
                return true;
            }

            bytesWritten = 0;
            return false;
        }

        public static bool TryWriteDateTimeOffset(Span<byte> name, DateTimeOffset value, Span<byte> target, out int bytesWritten)
        {
            if (!TryWriteName(name, ref target, out bytesWritten)) return false;

            if (Utf8Formatter.TryFormat(value, target, out bytesWritten, 'O'))
            {
                bytesWritten += name.Length + 2;
                return true;
            }

            bytesWritten = 0;
            return false;
        }
        
        public static bool TryWriteDateTime(Span<byte> name, DateTime value, Span<byte> target, out int bytesWritten)
        {
            if (!TryWriteName(name, ref target, out bytesWritten)) return false;

            if (Utf8Formatter.TryFormat(value, target, out bytesWritten, 'O'))
            {
                bytesWritten += name.Length + 2;
                return true;
            }

            bytesWritten = 0;
            return false;
        }
        
        public static bool TryWriteGuid(Span<byte> name, Guid value, Span<byte> target, out int bytesWritten)
        {
            if (!TryWriteName(name, ref target, out bytesWritten)) return false;

            if (Utf8Formatter.TryFormat(value, target, out bytesWritten, 'N'))
            {
                bytesWritten += name.Length + 2;
                return true;
            }

            bytesWritten = 0;
            return false;
        }
        
        private static bool TryWriteName(Span<byte> name, ref Span<byte> target, out int bytesWritten)
        {
            bytesWritten = 0;
            if (target.Length < name.Length + 3)
            {
                return false;
            }

            target[0] = Comma;
            target = target.Slice(1);
            name.CopyTo(target);
            target = target.Slice(name.Length);
            target[0] = EqualSign;
            target = target.Slice(1);
            return true;
        }
    }
}