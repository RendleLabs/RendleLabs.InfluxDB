using System;
using System.Buffers;
using System.Threading;

namespace RendleLabs.InfluxDB
{
    public struct LineBuffer
    {
        private readonly ArrayPool<byte> _pool;
        internal byte[] _line;
        internal int Length;

        internal LineBuffer(int size, ArrayPool<byte> pool)
        {
            _pool = pool;
            _line = _pool.Rent(size);
            Length = 0;
        }

        public int Grow() => Grow(_line.Length * 2);

        public int Grow(int newSize)
        {
            if (_line == null) throw new InvalidOperationException("Attempt to use uninitialized LineBuffer.");
            var oldLine = Interlocked.Exchange(ref _line, new byte[newSize]);
            _pool.Return(oldLine);
            return newSize;
        }

        // ReSharper disable once MergeConditionalExpression
        public Span<byte> Span => _line != null ? _line.AsSpan() : throw new InvalidOperationException("Attempt to use uninitialized LineBuffer.");

        public void Return()
        {
            var oldLine = Interlocked.Exchange(ref _line, null);
            _pool.Return(oldLine);
        }
    }
}