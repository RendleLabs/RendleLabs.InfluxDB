using System;

namespace RendleLabs.InfluxDB
{
    /// <summary>
    /// Used to build instances of <see cref="IInfluxDBClient"/>.
    /// </summary>
    public class InfluxDBClientBuilder
    {
        private const int DefaultInitialBufferSize = 1024 * 64;
        private const int DefaultMaxBufferSize = 1024 * 1024 * 8;
        private readonly IInfluxDBHttpClient _httpClient;
        private readonly string _database;
        private readonly string _retentionPolicy;
        private readonly Action<Exception> _errorCallback;
        private readonly int _initialBufferSize;
        private readonly int _maxBufferSize;
        private readonly TimeSpan? _forceFlushInterval;

        /// <summary>
        /// Constructs a new instance of <see cref="InfluxDBClientBuilder"/>
        /// </summary>
        /// <param name="serverUri">The InfluxDB server URI, e.g. <c>http://localhost:8086</c></param>
        /// <param name="database">The InfluxDB database</param>
        public InfluxDBClientBuilder(string serverUri, string database)
            : this(InfluxDBHttpClient.Get(serverUri), database, null, null, DefaultInitialBufferSize, DefaultMaxBufferSize, null)
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="InfluxDBClientBuilder"/>
        /// </summary>
        /// <param name="serverUri">The InfluxDB server URI, e.g. <c>http://localhost:8086</c></param>
        /// <param name="database">The InfluxDB database</param>
        public InfluxDBClientBuilder(Uri serverUri, string database)
            : this(InfluxDBHttpClient.Get(serverUri), database, null, null, DefaultInitialBufferSize, DefaultMaxBufferSize, null)
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="InfluxDBClientBuilder"/>. For testing purposes only.
        /// </summary>
        /// <param name="httpClient">An <see cref="IInfluxDBHttpClient"/></param>
        /// <param name="database">The InfluxDB database</param>
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

        /// <summary>
        /// Use a specific retention policy.
        /// </summary>
        /// <param name="retentionPolicy">The InfluxDB retention policy to write to.</param>
        /// <returns>The builder.</returns>
        public InfluxDBClientBuilder UseRetentionPolicy(string retentionPolicy)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                retentionPolicy ?? throw new ArgumentNullException(nameof(retentionPolicy)), _errorCallback, _initialBufferSize, _maxBufferSize,
                _forceFlushInterval);
        }

        /// <summary>
        /// Use a callback to handle exceptions.
        /// </summary>
        /// <param name="errorCallback">An exception <see cref="Action{T}"/>.</param>
        /// <returns>The builder.</returns>
        public InfluxDBClientBuilder OnError(Action<Exception> errorCallback)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                _retentionPolicy, errorCallback ?? throw new ArgumentNullException(nameof(errorCallback)),
                _initialBufferSize, _maxBufferSize, _forceFlushInterval);
        }

        /// <summary>
        /// Set the initial size for memory buffers to hold Influx data.
        /// </summary>
        /// <param name="initialBufferSize">The initial buffer size. Don't make this too big.</param>
        /// <returns>The builder.</returns>
        /// <remarks>Defaults to 64KB</remarks>
        public InfluxDBClientBuilder InitialBufferSize(int initialBufferSize)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                _retentionPolicy, _errorCallback, initialBufferSize, _maxBufferSize,
                _forceFlushInterval);
        }

        /// <summary>
        /// Sets the maximum size for memory buffers to hold Influx data.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size. Definitely don't make this too big.</param>
        /// <returns>Defaults to 8MB.</returns>
        public InfluxDBClientBuilder MaxBufferSize(int maxBufferSize)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                _retentionPolicy, _errorCallback, _initialBufferSize, maxBufferSize,
                _forceFlushInterval);
        }

        /// <summary>
        /// Sets an interval at which data should be flushed regardless of buffer size.
        /// </summary>
        /// <param name="forceFlushInterval">The interval.</param>
        /// <returns>The builder.</returns>
        public InfluxDBClientBuilder ForceFlushInterval(TimeSpan forceFlushInterval)
        {
            return new InfluxDBClientBuilder(_httpClient, _database,
                _retentionPolicy, _errorCallback, _initialBufferSize, _maxBufferSize,
                forceFlushInterval);
        }

        /// <summary>
        /// Builds and starts the <see cref="IInfluxDBClient"/>.
        /// </summary>
        /// <returns>The client.</returns>
        public IInfluxDBClient Build()
        {
            return InfluxDBClient.Create(_httpClient, _database, _retentionPolicy, _errorCallback, _initialBufferSize, _maxBufferSize, _forceFlushInterval);
        }
    }
}