using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace RendleLabs.InfluxDB.DiagnosticSourceListener.Tests
{
    public class CustomFieldFormatterTests
    {
        [Fact]
        public void FormatsCustomField()
        {
            var args = new {custom = new CustomObject {Value = 42, Thing = "foo"}};
            Dictionary<(string, Type), Func<PropertyInfo, IFormatter>> cff = new Dictionary<(string, Type), Func<PropertyInfo, IFormatter>>
            {
                [("custom", typeof(CustomObject))] = p => new CustomObjectFieldFormatter(p)
            };
            var objectFormatter = new ObjectFormatter(args.GetType(), cff, null);
            var buffer = new byte[128];
            var span = buffer.AsSpan();
            Assert.True(objectFormatter.Write(args, null, span, out int written));
            var text = Encoding.UTF8.GetString(buffer, 0, written);
            Assert.Equal(" v=42", text);
        }
        
        [Fact]
        public void FormatsCustomFieldAndTag()
        {
            var args = new {custom = new CustomObject {Value = 42, Thing = "foo"}};
            Dictionary<(string, Type), Func<PropertyInfo, IFormatter>> cff = new Dictionary<(string, Type), Func<PropertyInfo, IFormatter>>
            {
                [("custom", typeof(CustomObject))] = p => new CustomObjectFieldFormatter(p)
            };
            Dictionary<(string, Type), Func<PropertyInfo, IFormatter>> ctf = new Dictionary<(string, Type), Func<PropertyInfo, IFormatter>>
            {
                [("custom", typeof(CustomObject))] = p => new CustomObjectTagFormatter(p)
            };
            var objectFormatter = new ObjectFormatter(args.GetType(), cff, ctf);
            var buffer = new byte[128];
            var span = buffer.AsSpan();
            Assert.True(objectFormatter.Write(args, null, span, out int written));
            var text = Encoding.UTF8.GetString(buffer, 0, written);
            Assert.Equal(",t=foo v=42", text);
        }
        
        private class CustomObject
        {
            public double Value { get; set; }
            public string Thing { get; set; }
        }
        
        private class CustomObjectFieldFormatter : IFormatter
        {
            private readonly PropertyInfo _property;
            private static readonly byte[] ValueEquals = Encoding.UTF8.GetBytes("v");

            internal CustomObjectFieldFormatter(PropertyInfo property)
            {
                _property = property;
            }

            public bool TryWrite(object obj, Span<byte> span, bool commaPrefix, out int bytesWritten)
            {
                if (_property.GetValue(obj) is CustomObject co)
                {
                    return FieldHelpers.Write(ValueEquals, co.Value, commaPrefix, span, out bytesWritten);
                }

                bytesWritten = 0;
                return true;
            }
        }

        private class CustomObjectTagFormatter : IFormatter
        {
            private static readonly byte[] ThingEquals = Encoding.UTF8.GetBytes("t");
            private readonly PropertyInfo _property;

            public CustomObjectTagFormatter(PropertyInfo property)
            {
                _property = property;
            }

            public bool TryWrite(object obj, Span<byte> span, bool _, out int bytesWritten)
            {
                if (!(_property.GetValue(obj) is CustomObject co))
                {
                    bytesWritten = 0;
                    return true;
                }

                return TagHelpers.TryWriteString(ThingEquals, co.Thing, span, out bytesWritten);
            }
        }
    }
}