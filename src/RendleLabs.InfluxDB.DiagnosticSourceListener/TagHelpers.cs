using System;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    public static class TagHelpers
    {
        private const byte Comma = (byte) ',';
        private const byte EqualSign = (byte) '=';
        
        public static bool TryWriteString(Span<byte> name, string value, Span<byte> target, out int bytesWritten)
        {
            if (target.Length < name.Length + 3)
            {
                bytesWritten = 0;
                return false;
            }

            target[0] = Comma;
            target = target.Slice(1);
            name.CopyTo(target);
            target = target.Slice(name.Length);
            target[0] = EqualSign;
            target = target.Slice(1);

            if (value.TryWriteEscapedUTF8(target, out bytesWritten))
            {
                bytesWritten += name.Length + 2;
                return true;
            }

            bytesWritten = 0;
            return false;
        }
    }
}