using System;
using System.Buffers.Text;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener
{
    public static class FieldHelpers
    {
        private const byte TrueValue = (byte) 't';
        private const byte FalseValue = (byte) 'f';
        private const byte IntegerSuffix = (byte) 'i';
        private const byte EqualSign = (byte) '=';
        private const byte Comma = (byte) ',';

        private static bool WriteFieldName(Span<byte> name, bool commaPrefix, ref Span<byte> target, out int written)
        {
            written = name.Length + (commaPrefix ? 2 : 1);
            
            if (target.Length < written + 1)
            {
                written = 0;
                return false;
            }

            if (commaPrefix)
            {
                target[0] = Comma;
                target = target.Slice(1);
            }
            
            name.CopyTo(target);
            target = target.Slice(name.Length);
            target[0] = EqualSign;
            target = target.Slice(1);
            
            return true;
        }
        
        public static bool Write(Span<byte> name, bool value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written))
            {
                return false;
            }

            if (value)
            {
                span[0] = TrueValue;
            }
            else
            {
                span[0] = FalseValue;
            }

            written += 1;
            return true;
        }

        public static bool Write(Span<byte> name, bool? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, byte value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written))
            {
                return false;
            }

            if (WriteInteger(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }

        public static bool Write(Span<byte> name, byte? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, Guid value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written)) return false;

            if (Utf8Formatter.TryFormat(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, Guid? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, short value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written)) return false;

            if (WriteInteger(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, short? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, int value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written)) return false;

            if (WriteInteger(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, int? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, long value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written)) return false;

            if (WriteInteger(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, long? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, ushort value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written)) return false;

            if (WriteUnsignedInteger(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, ushort? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, uint value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written)) return false;

            if (WriteUnsignedInteger(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, uint? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, ulong value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written)) return false;

            if (WriteUnsignedInteger(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, ulong? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, sbyte value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written)) return false;

            if (WriteInteger(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, sbyte? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, decimal value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written))
            {
                return false;
            }

            if (Utf8Formatter.TryFormat(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }

        public static bool Write(Span<byte> name, decimal? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, double value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written))
            {
                return false;
            }

            if (Utf8Formatter.TryFormat(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, double? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, float value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written))
            {
                return false;
            }

            if (Utf8Formatter.TryFormat(value, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, float? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        public static bool Write(Span<byte> name, TimeSpan value, bool commaPrefix, Span<byte> span, out int written)
        {
            if (!WriteFieldName(name, commaPrefix, ref span, out written))
            {
                return false;
            }

            if (Utf8Formatter.TryFormat(value.TotalMilliseconds, span, out int valueWritten))
            {
                written += valueWritten;
                return true;
            }

            written = 0;
            return false;
        }
        public static bool Write(Span<byte> name, TimeSpan? value, bool commaPrefix, Span<byte> span, out int written) =>
            Write(name, value.GetValueOrDefault(), commaPrefix, span, out written);
        
        private static bool WriteInteger(long value, Span<byte> span, out int written)
        {
            if (Utf8Formatter.TryFormat(value, span, out int formatted))
            {
                if (span.Length > formatted)
                {
                    span[formatted] = IntegerSuffix;
                    written = formatted + 1;
                    return true;
                }
            }

            written = 0;
            return false;
        }

        private static bool WriteUnsignedInteger(ulong value, Span<byte> span, out int written)
        {
            if (Utf8Formatter.TryFormat(value, span, out int formatted))
            {
                if (span.Length > formatted)
                {
                    span[formatted] = IntegerSuffix;
                    written = formatted + 1;
                    return true;
                }
            }

            written = 0;
            return false;
        }
    }
}