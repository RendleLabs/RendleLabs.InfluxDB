using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    public class ActivityWriterTests
    {
        [Fact]
        public void WritesSimpleKeyValuePair()
        {
            var target = new byte[8];
            var tags = new Dictionary<string, string> {["foo"] = "bar"};
            ActivityWriter.TryWriteTags(target.AsSpan(), tags, out int written);
            Assert.Equal(8, written);
            Assert.Equal(",foo=bar", Encoding.UTF8.GetString(target));
        }
        
        [Fact]
        public void WritesTwoKeyValuePairs()
        {
            var target = new byte[16];
            var tags = new Dictionary<string, string> {["foo"] = "bar", ["wib"] = "qux"};
            ActivityWriter.TryWriteTags(target.AsSpan(), tags, out int written);
            Assert.Equal(16, written);
            Assert.Equal(",foo=bar,wib=qux", Encoding.UTF8.GetString(target));
        }

        [Fact]
        public void EscapesSpacesInValues()
        {
            var target = new byte[9];
            var tags = new Dictionary<string, string> {["foo"] = "a b"};
            ActivityWriter.TryWriteTags(target.AsSpan(), tags, out int written);
            Assert.Equal(9, written);
            Assert.Equal(@",foo=a\ b", Encoding.UTF8.GetString(target));
        }

        [Fact]
        public void EscapesCommasInValues()
        {
            var target = new byte[9];
            var tags = new Dictionary<string, string> {["foo"] = "a,b"};
            ActivityWriter.TryWriteTags(target.AsSpan(), tags, out int written);
            Assert.Equal(9, written);
            Assert.Equal(@",foo=a\,b", Encoding.UTF8.GetString(target));
        }

        [Fact]
        public void EscapesEqualSignsInValues()
        {
            var target = new byte[9];
            var tags = new Dictionary<string, string> {["foo"] = "a=b"};
            ActivityWriter.TryWriteTags(target.AsSpan(), tags, out int written);
            Assert.Equal(9, written);
            Assert.Equal(@",foo=a\=b", Encoding.UTF8.GetString(target));
        }

        [Fact]
        public void EscapesSpacesInKeys()
        {
            var target = new byte[9];
            var tags = new Dictionary<string, string> {["f o"] = "abc"};
            ActivityWriter.TryWriteTags(target.AsSpan(), tags, out int written);
            Assert.Equal(9, written);
            Assert.Equal(@",f\ o=abc", Encoding.UTF8.GetString(target));
        }

        [Fact]
        public void EscapesCommasInKeys()
        {
            var target = new byte[9];
            var tags = new Dictionary<string, string> {["f,o"] = "abc"};
            ActivityWriter.TryWriteTags(target.AsSpan(), tags, out int written);
            Assert.Equal(9, written);
            Assert.Equal(@",f\,o=abc", Encoding.UTF8.GetString(target));
        }

        [Fact]
        public void EscapesEqualSignsInKeys()
        {
            var target = new byte[9];
            var tags = new Dictionary<string, string> {["f=o"] = "abc"};
            ActivityWriter.TryWriteTags(target.AsSpan(), tags, out int written);
            Assert.Equal(9, written);
            Assert.Equal(@",f\=o=abc", Encoding.UTF8.GetString(target));
        }
    }
}