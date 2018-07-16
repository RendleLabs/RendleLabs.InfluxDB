using System;
using System.Buffers;
using System.Text;
using Xunit;

namespace RendleLabs.DiagnosticSource.InfluxDBListener.Tests
{
    public class ObjectFormatterTests
    {
        [Fact]
        public void WritesTagsAndFields()
        {
            var obj = new {tag = "foo", foo = 42};
            var formatter = new ObjectFormatter(obj.GetType());

            var memory = ArrayPool<byte>.Shared.Rent(1024);
            var span = memory.AsSpan();
            int written = formatter.Write(obj, ref span);
            var str = Encoding.UTF8.GetString(memory, 0, written);
            Assert.Equal(",tag=foo foo=42", str);
            ArrayPool<byte>.Shared.Return(memory);
        }
    }
}