using System.Text;
using Xunit;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    public class DiagnosticListenerOptionsTests
    {
        [Fact]
        public void AddsDefaultTags()
        {
            var options = new DiagnosticListenerOptions();
            options.AddDefaultTag("foo", "one");
            options.AddDefaultTag("bar", "two");
            var text = Encoding.UTF8.GetString(options.DefaultTags);
            Assert.Equal(",foo=one,bar=two", text);
        }
    }
}