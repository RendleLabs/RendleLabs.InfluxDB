using System;
using System.Collections.Concurrent;

namespace RendleLabs.InfluxDB.Performance
{
    internal static class ReflectorCollection
    {
        private static readonly ConcurrentDictionary<(string, Type), Reflector> Reflectors = new ConcurrentDictionary<(string, Type), Reflector>();

        public static Reflector GetOrAdd(string name, Type type)
        {
            return Reflectors.GetOrAdd((name, type), Create);
        }

        private static Reflector Create((string name, Type type) args)
        {
            return new Reflector(args.type);
        }
    }
}