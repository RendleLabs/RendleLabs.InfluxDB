using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    public class InfluxDBBuffer : IDisposable
    {
        private readonly IInfluxDBClient _client;
        private readonly Action<Exception> _errorCallback;
        private readonly string _path;
        private readonly BlockingCollection<WriteRequest> _requests = new BlockingCollection<WriteRequest>();
        private readonly Thread _thread;
        private readonly object _sync = new object();
        private readonly List<Task> _pending = new List<Task>();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Timer _timer;
        private byte[] _memory;
        private int _size;
        private int _bufferSize;
        private readonly int _maxBufferSize;

        private InfluxDBBuffer(IInfluxDBClient client, string database, string retentionPolicy, Action<Exception> errorCallback, int initialBufferSize,
            int maxBufferSize, CancellationTokenSource cancellationTokenSource, TimeSpan? forceFlushInterval)
        {
            _client = client;
            _errorCallback = errorCallback;

            _path = retentionPolicy == null
                ? $"write?db={Uri.EscapeDataString(database)}"
                : $"write?db={Uri.EscapeDataString(database)}&rp={Uri.EscapeDataString(retentionPolicy)}";

            _bufferSize = initialBufferSize;
            _maxBufferSize = maxBufferSize;
            _cancellationTokenSource = cancellationTokenSource;
            _memory = ArrayPool<byte>.Shared.Rent(initialBufferSize);

            if (forceFlushInterval != null)
            {
                _timer = new Timer(ForceFlush, null, TimeSpan.Zero, forceFlushInterval.Value);
            }

            _thread = new Thread(Run)
            {
                IsBackground = true
            };
        }

        public bool TryRequest(WriteRequest request) => _requests.TryAdd(request);

        private void ForceFlush(object state)
        {
            _requests.TryAdd(WriteRequest.FlushRequest);
        }

        public static InfluxDBBuffer Create(IInfluxDBClient client, string database, string retentionPolicy = null, Action<Exception> errorCallback = null,
            int initialBufferSize = 65536,
            int maxBufferSize = 1024 * 1024 * 64,
            TimeSpan? forceFlushInterval = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(database)) throw new ArgumentException("Value cannot be null or empty.", nameof(database));
            
            var cts = new CancellationTokenSource();
            var instance = new InfluxDBBuffer(client, database, retentionPolicy, errorCallback, initialBufferSize, maxBufferSize, cts, forceFlushInterval);
            instance.Start(cts.Token);
            return instance;
        }

        private void Start(CancellationToken token)
        {
            _thread.Start(token);
        }

        private void Run(object arg)
        {
            if (!(arg is CancellationToken token))
            {
                throw new InvalidOperationException();
            }

            while (!token.IsCancellationRequested)
            {
                var request = _requests.Take(token);
                if (!token.IsCancellationRequested)
                {
                    ProcessRequest(request);
                }
            }
        }

        private void ProcessRequest(WriteRequest request)
        {
            if (request.Flush && _size > 0)
            {
                SwapMemoryAndWrite();
                return;
            }
            
            var span = _memory.AsSpan(_size);
            if (request.Writer.TryWrite(span, request.Args, request.Timestamp, out int bytesWritten))
            {
                _size += bytesWritten;
                if (_bufferSize - _size < bytesWritten)
                {
                    SwapMemoryAndWrite();
                }
                return;
            }

            if (_size == 0)
            {
                if (_bufferSize >= _maxBufferSize) return;
                _bufferSize *= 2;
            }

            SwapMemoryAndWrite();
            
            // ReSharper disable once TailRecursiveCall
            // Because this tail call should be eliminated by RyuJIT(?)
            ProcessRequest(request);
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
                _memory = ArrayPool<byte>.Shared.Rent(_bufferSize);
                _size = 0;
            }
            
            Send(oldBuffer, oldSize);
        }

        private async void Send(byte[] buffer, int size)
        {
            var task = _client.Write(buffer, size, _path);
            _pending.Add(task);
            try
            {
                await task;
            }
            catch (Exception e)
            {
                _errorCallback?.Invoke(e);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
                _pending.Remove(task);
            }
        }

        public async void Dispose()
        {
            _timer?.Dispose();
            _requests?.Dispose();
            _cancellationTokenSource.Cancel();
            
            if (_size > 0)
            {
                Send(_memory, _size);
            }

            if (_pending.Count <= 0) return;
            
            try
            {
                await Task.WhenAll(_pending);
            }
            catch (Exception e)
            {
                _errorCallback?.Invoke(e);
            }
        }
    }
}