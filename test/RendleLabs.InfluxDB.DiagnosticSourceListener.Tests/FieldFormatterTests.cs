using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    public class FieldFormatterTests
    {
        public static IEnumerable<object[]> TestNameFormatters = new[] { new[] { NameFixer.Identity }, new[] { (Func<string, string>)(n => n.ToUpperInvariant()) } };

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsNullableInt16(Func<string, string> formatter)
        {
            var obj = new {foo = (short?)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=42i"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsInt16(Func<string, string> formatter)
        {
            var obj = new {foo = (short)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=42i"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsInt32(Func<string, string> formatter)
        {
            var obj = new {foo = 42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=42i"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsInt64(Func<string, string> formatter)
        {
            var obj = new {foo = 42L};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=42i"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsUInt16(Func<string, string> formatter)
        {
            var obj = new {foo = (ushort)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=42i"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsUInt32(Func<string, string> formatter)
        {
            var obj = new {foo = 42u};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=42i"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsUInt64(Func<string, string> formatter)
        {
            var obj = new {foo = 42ul};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=42i"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsByte(Func<string, string> formatter)
        {
            var obj = new {foo = (byte)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=42i"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsSByte(Func<string, string> formatter)
        {
            var obj = new {foo = (sbyte)42};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=42i"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsSingle(Func<string, string> formatter)
        {
            var obj = new {foo = 4.2f};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=4.2"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsDouble(Func<string, string> formatter)
        {
            var obj = new {foo = 4.2};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=4.2"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsDecimal(Func<string, string> formatter)
        {
            var obj = new {foo = 4.2m};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[7];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(7, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=4.2"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsTrue(Func<string, string> formatter)
        {
            var obj = new {foo = true};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[5];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(5, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=t"), bytes);
        }

        [Theory]
        [MemberData(nameof(TestNameFormatters))]
        public void FormatsFalse(Func<string, string> formatter)
        {
            var obj = new {foo = false};
            var property = obj.GetType().GetProperty("foo");
            var target = FieldFormatter.TryCreate(property, formatter);
            var bytes = new byte[5];
            var span = bytes.AsSpan();
            Assert.True(target.TryWrite(obj, span, false, out int bytesWritten));
            Assert.Equal(5, bytesWritten);
            Assert.Equal(Encoding.UTF8.GetBytes($"{formatter("foo")}=f"), bytes);
        }
    }
}
