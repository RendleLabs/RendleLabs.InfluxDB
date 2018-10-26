using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RendleLabs.InfluxDB
{
    internal sealed class LineCollection : IDisposable
    {
        private const int MaxItems = 2048;
        private const int ArraySize = 4096;
        private readonly ArrayPool<byte> _bytePool;
        private readonly LineCollectionPool _lineCollectionPool;
        private readonly byte[][] _lines = ArrayPool<byte[]>.Shared.Rent(ArraySize);
        private readonly int[] _lengths = ArrayPool<int>.Shared.Rent(ArraySize);
        private int _count;
        private int _length;
        private bool _completed;

        public LineCollection() : this(ArrayPool<byte>.Shared, null)
        {
        }

        public LineCollection(LineBuffer firstLine) : this()
        {
            _count = 1;
            _lines[0] = firstLine._line;
            _length = _lengths[0] = firstLine.Length;
        }

        public LineCollection(ArrayPool<byte> bytePool, LineCollectionPool lineCollectionPool)
        {
            _bytePool = bytePool;
            _lineCollectionPool = lineCollectionPool;
        }

        public LineBuffer GetBuffer(int size) => new LineBuffer(size, _bytePool);

        public bool TryAdd(LineBuffer buffer)
        {
            if (_completed) return false;
            int count = Interlocked.Increment(ref _count);

            if (count > MaxItems)
            {
                Complete();
                return false;
            }

            if (count == MaxItems)
            {
                Complete();
            }

            --count;

            _lines[count] = buffer._line;
            _lengths[count] = buffer.Length;
            Interlocked.Add(ref _length, buffer.Length);
            return true;
        }

        internal void Add(LineBuffer buffer)
        {
            int index = Interlocked.Increment(ref _count) - 1;
            _lines[index] = buffer._line;
            _lengths[index] = buffer.Length;
            Interlocked.Add(ref _length, buffer.Length);
        }

        public long Length => _length;

        public void Complete() => _completed = true;

        public bool IsCompleted => _completed;

        public int Count => _count;

        public void Dispose()
        {
            for (int i = 0, l = _count; i < l; i++)
            {
                _bytePool.Return(_lines[i]);
            }

            if (_lineCollectionPool != null)
            {
                _count = 0;
                _length = 0;
                _lineCollectionPool.Return(this);
                return;
            }

            ArrayPool<byte[]>.Shared.Return(_lines);
            ArrayPool<int>.Shared.Return(_lengths);
        }

        internal struct LineCollectionEnumerator : IEnumerator<Line>
        {
            private readonly LineCollection _lines;
            private int _current;

            public LineCollectionEnumerator(LineCollection lines)
            {
                _lines = lines;
                _current = -1;
            }

            public bool MoveNext()
            {
                ++_current;
                return _current < _lines.Count;
            }

            public void Reset()
            {
                _current = -1;
            }

            public Line Current => _current < _lines.Count
                ? new Line(_lines._lines[_current], _lines._lengths[_current])
                : throw new InvalidOperationException();

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        internal struct Line
        {
            public readonly byte[] Bytes;
            public readonly int Length;

            public Line(byte[] bytes, int length)
            {
                Bytes = bytes;
                Length = length;
            }
        }

        public LineCollectionEnumerator GetEnumerator()
        {
            return new LineCollectionEnumerator(this);
        }
    }
}