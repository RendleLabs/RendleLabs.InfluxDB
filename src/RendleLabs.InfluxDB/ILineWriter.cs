using System;

namespace RendleLabs.InfluxDB
{
    /// <summary>
    /// Defines methods to write data to a <c>byte</c> <see cref="Span{T}"/>.
    /// </summary>
    public interface ILineWriter
    {
        /// <summary>
        /// Writes a standard InfluxDB HTTP API line to a provided chunk of memory.
        /// </summary>
        /// <param name="buffer">A chunk of memory to write to.</param>
        /// <param name="args">The data to encode.</param>
        /// <param name="requestTimestamp">The timestamp, in UNIX milliseconds.</param>
        /// <param name="bytesWritten">The number of bytes that were written, if any.</param>
        /// <returns><c>true</c> if the memory provided was large enough and the data was written successfully; otherwise, <c>false</c>.</returns>
        /// <remarks>If the method returns <c>false</c>, <c>bytesWritten</c> should be zero.</remarks>
        bool TryWrite(Span<byte> buffer, object args, long requestTimestamp, out int bytesWritten);
    }
}