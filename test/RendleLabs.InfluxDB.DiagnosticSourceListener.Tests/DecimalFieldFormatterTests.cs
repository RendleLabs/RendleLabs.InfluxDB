using System;
using System.Text;
using Xunit;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    public class DecimalFieldFormatterTests
    {
        [Fact]
        public void FormatsDecimalCorrectly()
        {
            var args = new {foo = 42m};
            var formatter = FieldFormatter.TryCreate(args.GetType().GetProperty("foo"), NameFixer.Identity);
            var buffer = new byte[64];
            Assert.True(formatter.TryWrite(args, buffer.AsSpan(), false, out int written));
            Assert.Equal(6, written);
            var text = Encoding.UTF8.GetString(buffer, 0, written);
            Assert.Equal("foo=42", text);
        }
        
        [Fact]
        public void FormatsNullableDecimalCorrectly()
        {
            decimal? foo = 42m;
            var args = new {foo};
            var formatter = FieldFormatter.TryCreate(args.GetType().GetProperty("foo"), NameFixer.Identity);
            var buffer = new byte[64];
            Assert.True(formatter.TryWrite(args, buffer.AsSpan(), false, out int written));
            Assert.Equal(6, written);
            var text = Encoding.UTF8.GetString(buffer, 0, written);
            Assert.Equal("foo=42", text);
        }
    }
}