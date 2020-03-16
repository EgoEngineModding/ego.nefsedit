// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Utility
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for <see cref="Stream"/>.
    /// </summary>
    public static class StreamExtensions
    {
        private const int BufferSize = 4096;

        /// <summary>
        /// Copies part of a stream to a destination stream.
        /// </summary>
        /// <param name="stream">The input stream to copy from.</param>
        /// <param name="destination">The destination stream to write to.</param>
        /// <param name="length">The number of bytes to copy.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>An async task.</returns>
        public static async Task CopyPartialAsync(
            this Stream stream,
            Stream destination,
            long length,
            CancellationToken cancelToken)
        {
            // Use a temporary buffer to transfer chunks of data at a time
            var buffer = new byte[BufferSize];
            var bytesRemaining = length;

            while (bytesRemaining > 0)
            {
                // Read from input stream
                var bytesToRead = Math.Min(length, BufferSize);
                var bytesRead = await stream.ReadAsync(buffer, 0, (int)bytesToRead, cancelToken);

                // Copy to destination
                await destination.WriteAsync(buffer, 0, bytesRead, cancelToken);

                bytesRemaining -= bytesToRead;
            }
        }
    }
}
