using System;
using System.Buffers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    public class InfluxDBClient : IInfluxDBClient
    {
        private readonly ArrayPool<byte> _arrayPool;

        private readonly Channel<WriteRequest> _requests = Channel.CreateBounded<WriteRequest>(
            new BoundedChannelOptions(8192)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true
            });

        private readonly Action<Exception>? _errorCallback;
        private readonly Task _task;
        private readonly object _sync = new object();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly TimeSpan _forceFlushInterval;
        private readonly Timer? _timer;
        private readonly InfluxDBOutput _output;
        private bool _isDisposed;
        private byte[] _memory;
        private int _size;
        private int _bufferSize;
        private readonly int _maxBufferSize;

        private InfluxDBClient(IInfluxDBHttpClient httpClient, string database, string? retentionPolicy,
            Action<Exception>? errorCallback, int initialBufferSize,
            int maxBufferSize, CancellationTokenSource cancellationTokenSource, TimeSpan? forceFlushInterval,
            ArrayPool<byte> arrayPool)
        {
            _errorCallback = errorCallback;

            var path = retentionPolicy == null
                ? $"write?db={Uri.EscapeDataString(database)}&precision=ms"
                : $"write?db={Uri.EscapeDataString(database)}&precision=ms&rp={Uri.EscapeDataString(retentionPolicy)}";

            _bufferSize = initialBufferSize;
            _maxBufferSize = maxBufferSize;
            _arrayPool = arrayPool;
            _cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
            _memory = _arrayPool.Rent(initialBufferSize);

            if (forceFlushInterval != null)
            {
                _forceFlushInterval = forceFlushInterval.Value;
                _timer = new Timer(ForceFlush, null, _forceFlushInterval, _forceFlushInterval);
            }

            _output = new InfluxDBOutput(httpClient, path);
            _task = Run(_cancellationTokenSource.Token);
        }

        public bool TryRequest(WriteRequest request) => _requests.Writer.TryWrite(request);

        public ValueTask RequestAsync(WriteRequest request, CancellationToken token = default) =>
            _requests.Writer.WriteAsync(request, token);

        private void ForceFlush(object state)
        {
            _requests.Writer.TryWrite(WriteRequest.FlushRequest);
        }

        internal static InfluxDBClient Create(IInfluxDBHttpClient httpClient, string database, string? retentionPolicy,
            Action<Exception>? errorCallback,
            int initialBufferSize,
            int maxBufferSize,
            TimeSpan? forceFlushInterval,
            ArrayPool<byte>? arrayPool = null)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (string.IsNullOrEmpty(database))
                throw new ArgumentException("Value cannot be null or empty.", nameof(database));

            var cts = new CancellationTokenSource();
            var instance = new InfluxDBClient(httpClient, database, retentionPolicy, errorCallback, initialBufferSize,
                maxBufferSize, cts, forceFlushInterval, arrayPool ?? ArrayPool<byte>.Shared);
            return instance;
        }

        private async Task Run(CancellationToken token)
        {
            var reader = _requests.Reader;
            try
            {
                while (!token.IsCancellationRequested && await reader.WaitToReadAsync(token))
                {
                    while (reader.TryRead(out var request))
                    {
                        ProcessRequest(request);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }

            if (_size > 0)
            {
                Send(_memory, _size);
            }
        }

        private void ProcessRequest(WriteRequest request)
        {
            if (request.FlushSentinel && _size > 0)
            {
                SwapMemoryAndWrite();
                return;
            }

            if (_bufferSize - _size < request.Writer.LongestWritten)
            {
                SwapMemoryAndWrite();
            }

            try
            {
                var span = _memory.AsSpan(_size);

                if (request.Writer.TryWrite(span, request.Args, request.Activity, request.Timestamp,
                    out int bytesWritten))
                {
                    _size += bytesWritten;
                    return;
                }

                IncreaseNextBufferSize();
                SwapMemoryAndWrite();
            }
            catch (Exception ex)
            {
                _errorCallback?.Invoke(ex);
            }

            // ReSharper disable once TailRecursiveCall
            // Because this tail call should be eliminated by RyuJIT(?)
            ProcessRequest(request);
        }

        private void IncreaseNextBufferSize()
        {
            if (_size == 0)
            {
                if (_bufferSize >= _maxBufferSize)
                {
                    throw new BufferMaxSizeExceededException();
                }

                _bufferSize *= 2;
            }
        }

        private void SwapMemoryAndWrite()
        {
            byte[] oldBuffer;
            int oldSize;

            // Theoretically we should only be running on a single thread, but let's be safe.
            lock (_sync)
            {
                oldBuffer = _memory;
                oldSize = _size;
                _memory = _arrayPool.Rent(_bufferSize);
                _size = 0;
                if (!_isDisposed)
                {
                    _timer?.Change(_forceFlushInterval, _forceFlushInterval);
                }
            }

            Send(oldBuffer, oldSize);
        }

        private void Send(byte[] buffer, int size)
        {
            _output.Write(buffer, size);
        }

        public void Dispose()
        {
            _isDisposed = true;
            _timer?.Dispose();
            _requests.Writer.TryComplete();
            Flush();
            _output.Finish();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _task.Dispose();
        }

        public void Flush()
        {
            if (_size > 0)
            {
                SwapMemoryAndWrite();
            }
        }
    }
}