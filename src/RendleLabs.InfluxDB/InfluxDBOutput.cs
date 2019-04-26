using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RendleLabs.InfluxDB
{
    internal class InfluxDBOutput : IInfluxDBOutput
    {
        private readonly ArrayPool<byte> _arrayPool;
        private readonly IInfluxDBHttpClient _influxDBHttpClient;
        private readonly string _path;
        private readonly Channel<Data> _data = Channel.CreateBounded<Data>(new BoundedChannelOptions(128)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });

        private readonly Task _task;
        private readonly List<Task> _writes = new List<Task>();
        
        private int _stopped;

        public InfluxDBOutput(IInfluxDBHttpClient influxDBHttpClient, string path) : this(influxDBHttpClient, path, ArrayPool<byte>.Shared)
        {
        }

        public InfluxDBOutput(IInfluxDBHttpClient influxDBHttpClient, string path, ArrayPool<byte> arrayPool)
        {
            _influxDBHttpClient = influxDBHttpClient;
            _path = path;
            _arrayPool = arrayPool;
            _task = Read();
        }

        public void Write(byte[] buffer, int size)
        {
            _data.Writer.TryWrite(new Data(buffer, size));
        }

        private async Task Read()
        {
            var reader = _data.Reader;
            try
            {
                while (await reader.WaitToReadAsync())
                {
                    try
                    {
                        while (reader.TryRead(out var data))
                        {
                            _writes.Add(Send(data.Buffer, data.Size));
                        }

                        if (_writes.Count == 1)
                        {
                            await _writes[0];
                        }
                        else
                        {
                            await Task.WhenAll(_writes);
                        }
                    }
                    finally
                    {
                        _writes.Clear();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
        }

        private async Task Send(byte[] buffer, int size)
        {
            try
            {
                await _influxDBHttpClient.Write(buffer, size, _path);
            }
            finally
            {
                _arrayPool.Return(buffer);
            }
        }

        public void Finish()
        {
            if (Interlocked.Exchange(ref _stopped, 1) == 0)
            {
                _data.Writer.TryComplete();
                // Block on outstanding requests
                _task.GetAwaiter().GetResult();
            }
        }

        private readonly struct Data
        {
            public readonly byte[] Buffer;
            public readonly int Size;
            
            public Data(byte[] buffer, int size)
            {
                Buffer = buffer;
                Size = size;
            }
        }
    }
}