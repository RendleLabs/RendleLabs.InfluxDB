using System;
using System.Text;
using Xunit;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    public class FieldFormatterTests
    {
        [Fact]
        public void FormatsNullableInt16()
        {
            var obj = new {foo = (short?)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42i"), bytes);
        }
        
        [Fact]
        public void FormatsInt16()
        {
            var obj = new {foo = (short)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42i"), bytes);
        }
        
        [Fact]
        public void FormatsInt32()
        {
            var obj = new {foo = 42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42i"), bytes);
        }
        
        [Fact]
        public void FormatsInt64()
        {
            var obj = new {foo = 42L};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42i"), bytes);
        }
        
        [Fact]
        public void FormatsUInt16()
        {
            var obj = new {foo = (ushort)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42i"), bytes);
        }
        
        [Fact]
        public void FormatsUInt32()
        {
            var obj = new {foo = 42u};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42i"), bytes);
        }
        
        [Fact]
        public void FormatsUInt64()
        {
            var obj = new {foo = 42ul};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42i"), bytes);
        }
        
        [Fact]
        public void FormatsByte()
        {
            var obj = new {foo = (byte)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42i"), bytes);
        }
        
        [Fact]
        public void FormatsSByte()
        {
            var obj = new {foo = (sbyte)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=42i"), bytes);
        }
        
        [Fact]
        public void FormatsSingle()
        {
            var obj = new {foo = 4.2f};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=4.2"), bytes);
        }
        
        [Fact]
        public void FormatsDouble()
        {
            var obj = new {foo = 4.2};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=4.2"), bytes);
        }
        
        [Fact]
        public void FormatsDecimal()
        {
            var obj = new {foo = 4.2m};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=4.2"), bytes);
        }
        
        [Fact]
        public void FormatsTrue()
        {
            var obj = new {foo = true};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[5];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(5, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=t"), bytes);
        }
        
        [Fact]
        public void FormatsFalse()
        {
            var obj = new {foo = false};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property);
            var bytes = new byte[5];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(5, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes("foo=f"), bytes);
        }
    }
}
