using System;
using System.Buffers;
using System.Text;
using Xunit;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    public class ObjectFormatterTests
    {
        [Fact]
        public void WritesTagsAndFields()
        {
            const string expected = ",tag=foo foo=42";
            
            var obj = new {tag = "foo", foo = 42};
            var formatter = new ObjectFormatter(obj.GetType());

            var memory = ArrayPool<byte>.Shared.Rent(1024);
            var span = memory.AsSpan();
            formatter.Write(obj, ref span, out int written);
            var str = Encoding.UTF8.GetString(memory, 0, written);
            Assert.Equal(expected.Length, written);
            Assert.Equal(expected, str);
            ArrayPool<byte>.Shared.Return(memory);
        }
    }
}