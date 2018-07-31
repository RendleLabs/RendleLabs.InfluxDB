using System;
using System.Buffers;
using System.Text;
using Xunit;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    public class InfluxLineFormatterTests
    {
        [Fact]
        public void WritesTagsAndFields()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var expected = $"test,tag=foo foo=42 {now}\n";
            
            var obj = new {tag = "foo", foo = 42};
            var formatter = new InfluxLineFormatter("test", obj.GetType());

            var memory = ArrayPool<byte>.Shared.Rent(1024);
            var span = memory.AsSpan();
            formatter.TryWrite(span, obj, now, out int written);
            var str = Encoding.UTF8.GetString(memory, 0, written);
            Assert.Equal(expected, str);
            ArrayPool<byte>.Shared.Return(memory);
        }

        [Fact]
        public void WritesJustFields()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var expected = $"test foo=42 {now}\n";
            
            var obj = new {foo = 42};
            var formatter = new InfluxLineFormatter("test", obj.GetType());

            var memory = ArrayPool<byte>.Shared.Rent(1024);
            var span = memory.AsSpan();
            formatter.TryWrite(span, obj, now, out int written);
            var str = Encoding.UTF8.GetString(memory, 0, written);
            Assert.Equal(expected, str);
            ArrayPool<byte>.Shared.Return(memory);
            
        }
    }
}