using System;
using System.Text;
using Xunit;

namespace RendleLabs.DiagnosticSource.InfluxDBListener.Tests
{
    public class FieldFormatterTests
    {
        [Fact]
        public void FormatsInt()
        {
            var obj = new {foo = 42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[6];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, ref span, out int bytesWritten));
            Assert.Equal(6, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42"), bytes);
        }
    }
}
