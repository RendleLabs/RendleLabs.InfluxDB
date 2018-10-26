using System.Buffers;
using System.Collections.Concurrent;

namespace RendleLabs.InfluxDB
{
    internal class LineCollectionPool
    {
        public static LineCollectionPool Shared { get; } = new LineCollectionPool();
        
        private readonly ConcurrentBag<LineCollection> _pool = new ConcurrentBag<LineCollection>();

        public LineCollection Rent()
        {
            if (_pool.TryTake(out var collection)) return collection;
            return new LineCollection(ArrayPool<byte>.Shared, this);
        }

        public LineCollection Rent(LineBuffer buffer)
        {
            var collection = Rent();
            collection.Add(buffer);
            return collection;
        }

        public void Return(LineCollection collection)
        {
            _pool.Add(collection);
        }
    }
}