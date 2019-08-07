using System.Diagnostics;
using Xunit;

namespace RendleLabs.Diagnostics.Tests
{
    public class IfEnabledTests
    {
        [Fact]
        public void WritesWhenEnabled()
        {
            string name = null;
            Args args = null;

            var listener = new DiagnosticListener("Tests");
            var observer = new Observer((n, a) => (name, args) = (n, a as Args));

            using (listener.Subscribe(observer))
            {
                listener.IfEnabled("Test")?.Write(new Args("Pass"));
            }

            Assert.Equal("Test", name);
            Assert.Equal("Pass", args?.Value);
        }

        [Fact]
        public void DoesNotCreateObjectWhenNotEnabled()
        {
            bool wasCalled = false;

            var listener = new DiagnosticListener("Tests");

            listener.IfEnabled("Test")?.Write(ShouldNotBeCalled(out wasCalled));

            Assert.False(wasCalled);
        }

        private static object ShouldNotBeCalled(out bool wasCalled)
        {
            wasCalled = true;
            return new object();
        }
    }
}
