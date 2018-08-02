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
            const string expected = ",tag=foo foo=42i";
            
            var obj = new {tag = "foo", foo = 42};
            var formatter = new ObjectFormatter(obj.GetType(), null, null);

            var memory = ArrayPool<byte>.Shared.Rent(1024);
            var span = memory.AsSpan();
            formatter.Write(obj, span, out int written);
            var str = Encoding.UTF8.GetString(memory, 0, written);
            Assert.Equal(expected.Length, written);
            Assert.Equal(expected, str);
            ArrayPool<byte>.Shared.Return(memory);
        }
        
        [Fact]
        public void WritesFields()
        {
            const string expected = " foo=42i";
            
            var obj = new {foo = 42};
            var formatter = new ObjectFormatter(obj.GetType(), null, null);

            var memory = ArrayPool<byte>.Shared.Rent(1024);
            var span = memory.AsSpan();
            formatter.Write(obj, span, out int written);
            var str = Encoding.UTF8.GetString(memory, 0, written);
            Assert.Equal(expected.Length, written);
            Assert.Equal(expected, str);
            ArrayPool<byte>.Shared.Return(memory);
        }
    }
}