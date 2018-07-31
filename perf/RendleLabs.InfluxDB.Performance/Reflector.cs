using System;
using System.Collections.Generic;

namespace RendleLabs.InfluxDB.Performance
{
    internal class Reflector
    {
        private readonly List<Action<object, IDictionary<string, object>>> _fieldGetters = new List<Action<object, IDictionary<string, object>>>();
        private readonly List<Action<object, IDictionary<string, string>>> _tagGetters = new List<Action<object, IDictionary<string, string>>>();
        
        public Reflector(Type type)
        {
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    _tagGetters.Add((args, dict) =>
                    {
                        var value = property.GetValue(args) as string;
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            dict[property.Name] = value;
                        }
                    });
                }
                else
                {
                    _fieldGetters.Add((args, dict) =>
                    {
                        var value = property.GetValue(args);
                        if (value != null)
                        {
                            dict[property.Name] = value;
                        }
                    });
                }
            }
        }

        public void Reflect(object args, IDictionary<string, object> fields, IDictionary<string, string> tags)
        {
            for (int i = 0; i < _fieldGetters.Count; i++)
            {
                _fieldGetters[i](args, fields);
            }
            for (int i = 0; i < _tagGetters.Count; i++)
            {
                _tagGetters[i](args, tags);
            }
        }
    }
}