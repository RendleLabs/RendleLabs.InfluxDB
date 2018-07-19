using System;
using System.Buffers;
using System.Text;
using Xunit;

namespace RendleLabs.DiagnosticSource.InfluxDBListener.Tests
{
    public class InfluxLineFormatterTests
    {
        [Fact]
        public void WritesTagsAndFields()
        {
            const string expected = "test,tag=foo foo=42\n";
            
            var obj = new {tag = "foo", foo = 42};
            var formatter = new InfluxLineFormatter("test", obj.GetType());

            var memory = ArrayPool<byte>.Shared.Rent(1024);
            var span = memory.AsSpan();
            int written = formatter.Write(obj, ref span);
            Assert.Equal(expected.Length, written);
            var str = Encoding.UTF8.GetString(memory, 0, written);
            Assert.Equal(expected, str);
            ArrayPool<byte>.Shared.Return(memory);
        }
    }
}