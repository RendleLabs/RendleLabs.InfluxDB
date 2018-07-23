using System;

namespace RendleLabs.InfluxDB
{
    public class InfluxDBClientBuilder
    {
        private const int DefaultInitialBufferSize = 65536;
        private const int DefaultMaxBufferSize = 1024 * 1024 * 8;
        private readonly IInfluxDBHttpClient _httpClient;
        private readonly string _database;
        private readonly string _retentionPolicy;
        private readonly Action<Exception> _errorCallback;
        private readonly int _initialBufferSize;
        private readonly int _maxBufferSize;
        private readonly TimeSpan? _forceFlushInterval;

        public InfluxDBClientBuilder(string serverUri, string database)
            : this(InfluxDBHttpClient.Get(serverUri), database, null, null, DefaultInitialBufferSize, DefaultMaxBufferSize, null)
        {
        }

        public InfluxDBClientBuilder(Uri serverUri, string database)
            : this(InfluxDBHttpClient.Get(serverUri), database, null, null, DefaultInitialBufferSize, DefaultMaxBufferSize, null)
        {
        }

        internal InfluxDBClientBuilder(IInfluxDBHttpClient httpClient, string database)
            : this(httpClient, database, null, null, DefaultInitialBufferSize, DefaultMaxBufferSize, null)
        {
        }
        
        private InfluxDBClientBuilder(IInfluxDBHttpClient httpClient, string database, string retentionPolicy,
            Action<Exception> errorCallback, int initialBufferSize, int maxBufferSize, TimeSpan? forceFlushInterval)
        {
            _httpClient = httpClient;
            _database = database;
            _retentionPolicy = retentionPolicy;
            _errorCallback = errorCallback;
            _initialBufferSize = initialBufferSize;
            _maxBufferSize = maxBufferSize;
            _forceFlushInterval = forceFlushInterval;
        }

        public InfluxDBClientBuilder UseRetentionPolicy(string retentionPolicy)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                retentionPolicy ?? throw new ArgumentNullException(nameof(retentionPolicy)), _errorCallback, _initialBufferSize, _maxBufferSize,
                _forceFlushInterval);
        }

        public InfluxDBClientBuilder OnError(Action<Exception> errorCallback)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                _retentionPolicy, errorCallback ?? throw new ArgumentNullException(nameof(errorCallback)),
                _initialBufferSize, _maxBufferSize, _forceFlushInterval);
        }

        public InfluxDBClientBuilder InitialBufferSize(int initialBufferSize)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                _retentionPolicy, _errorCallback, initialBufferSize, _maxBufferSize,
                _forceFlushInterval);
        }

        public InfluxDBClientBuilder MaxBufferSize(int maxBufferSize)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                _retentionPolicy, _errorCallback, _initialBufferSize, maxBufferSize,
                _forceFlushInterval);
        }

        public InfluxDBClientBuilder ForceFlushInterval(TimeSpan forceFlushInterval)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                _retentionPolicy, _errorCallback, _initialBufferSize, _maxBufferSize,
                forceFlushInterval);
        }

        public IInfluxDBClient Build()
        {
            return InfluxDBClient.Create(_httpClient, _database, _retentionPolicy, _errorCallback, _initialBufferSize, _maxBufferSize, _forceFlushInterval);
        }
    }
}