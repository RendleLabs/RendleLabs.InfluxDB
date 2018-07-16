using System;
using System.Text;
using Xunit;

namespace RendleLabs.DiagnosticSource.InfluxDBListener.Tests
{
    public class InfluxNameTests
    {
        [Fact]
        public void JustReturnsBytes()
        {
            var actual = InfluxName.Escape("Foo");
            Assert.Equal(Encoding.UTF8.GetBytes("Foo"), actual);
        }
    }
}