using System;
using System.Buffers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    public class InfluxDBChannel : IDisposable
    {
        private readonly IInfluxDBHttpClient _httpClient;
        private readonly Action<Exception> _errorCallback;
        private readonly string _path;
        private readonly TimeSpan _forceFlushInterval;
        private readonly Timer _timer;
        private readonly Task _completion;
        private readonly Channel<LineCollection> _channel;
        private LineCollection _lines;
        private bool _isDisposed;

        private InfluxDBChannel(IInfluxDBHttpClient httpClient, string database, string retentionPolicy, Action<Exception> errorCallback,
            TimeSpan? forceFlushInterval, int channelCapacity)
        {
            _httpClient = httpClient;
            _errorCallback = errorCallback;

            _path = retentionPolicy == null
                ? $"write?db={Uri.EscapeDataString(database)}&precision=ms"
                : $"write?db={Uri.EscapeDataString(database)}&precision=ms&rp={Uri.EscapeDataString(retentionPolicy)}";

            _channel = Channel.CreateBounded<LineCollection>(new BoundedChannelOptions(channelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true
            });

            _lines = LineCollectionPool.Shared.Rent();

            _completion = Run();

            if (forceFlushInterval != null)
            {
                _forceFlushInterval = forceFlushInterval.Value;
                _timer = new Timer(ForceFlush, null, _forceFlushInterval, _forceFlushInterval);
            }
        }

        private void ForceFlush(object state)
        {
            _channel.Writer.TryWrite(default);
        }

        public void TryAdd(LineBuffer buffer, int length)
        {
            try
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(InfluxDBChannel));
                buffer.Length = length;
                if (!_lines.TryAdd(buffer))
                {
                    SwapAndQueueBuffer(buffer);
                }
            }
            catch (Exception exception)
            {
                _errorCallback?.Invoke(exception);
            }
        }

        internal static InfluxDBChannel Create(IInfluxDBHttpClient httpClient, string database, string retentionPolicy,
            Action<Exception> errorCallback = null,
            TimeSpan? forceFlushInterval = null,
            int channelCapacity = 4096)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (string.IsNullOrEmpty(database)) throw new ArgumentException("Value cannot be null or empty.", nameof(database));

            var instance = new InfluxDBChannel(httpClient, database, retentionPolicy, errorCallback, forceFlushInterval,
                channelCapacity);
            return instance;
        }

        private async Task Run()
        {
            var reader = _channel.Reader;
            try
            {
                while (await reader.WaitToReadAsync().ConfigureAwait(false))
                {
                    await Read(reader).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                _errorCallback?.Invoke(exception);
            }
        }

        private Task Read(ChannelReader<LineCollection> reader)
        {
            async Task Write(LineCollection lines)
            {
                await _httpClient.Write(new InfluxHttpContent(lines), _path).ConfigureAwait(false);
                lines.Dispose();
            }

            try
            {
                while (reader.TryRead(out var item))
                {
                    if (item.Length > 0)
                    {
                        return Write(item);
                    }
                }
            }
            catch (Exception exception)
            {
                _errorCallback?.Invoke(exception);
            }

            return Task.CompletedTask;
        }

        private void SwapAndQueueBuffer(LineBuffer nextBuffer = default)
        {
            var lineCollection = nextBuffer.Length == 0
                ? LineCollectionPool.Shared.Rent()
                : LineCollectionPool.Shared.Rent(nextBuffer);

            try
            {
                using (var oldLines = Interlocked.Exchange(ref _lines, lineCollection))
                {
                    if (oldLines.Count > 0)
                    {
                        _timer?.Change(_forceFlushInterval, _forceFlushInterval);
                        _channel.Writer.TryWrite(oldLines);
                    }
                }
            }
            catch (Exception exception)
            {
                _errorCallback?.Invoke(exception);
            }
        }

        public LineBuffer GetBuffer(int size) => _lines.GetBuffer(size);

        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

        public async Task DisposeAsync()
        {
            try
            {
                _isDisposed = true;
                _channel.Writer.TryComplete();
                await FlushAsync();
                await _completion;
            }
            catch (Exception exception)
            {
                _errorCallback?.Invoke(exception);
            }
        }

        public Task FlushAsync()
        {
            if (_lines.Count > 0)
            {
                try
                {
                    SwapAndQueueBuffer();
                }
                catch (Exception exception)
                {
                    _errorCallback?.Invoke(exception);
                }
            }
            return Task.CompletedTask;
        }
    }
}