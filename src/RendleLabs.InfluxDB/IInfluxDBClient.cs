namespace RendleLabs.InfluxDB
{
    /// <summary>
    /// Provides a single threaded mechanism to write data to InfluxDB
    /// </summary>
    public interface IInfluxDBClient
    {
        /// <summary>
        /// Try to add a <see cref="WriteRequest"/> to the queue.
        /// </summary>
        /// <param name="request">The request to be written.</param>
        /// <returns><c>true</c> if the request is queued successfully, otherwise, <c>false</c>.</returns>
        bool TryRequest(WriteRequest request);
    }
}