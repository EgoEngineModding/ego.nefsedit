// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Utility
{
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Compression utilities.
    /// </summary>
    internal class DeflateHelper
    {
        /// <summary>
        /// Takes data from an input file stream, compresses it, and writes it to the specified
        /// output file. Streams should already seek to the proper location before calling this function.
        /// </summary>
        /// <param name="inStream">The input file stream to read from.</param>
        /// <param name="numBytes">Number of bytes to read in.</param>
        /// <param name="outStream">The output file stream to write compressed chunk to.</param>
        /// <returns>
        /// Number of bytes actually read from input file and the compressed size of the chunk that
        /// was written.
        /// </returns>
        public static async Task<(int BytesRead, int ChunkSize)> DeflateAsync(
            Stream inStream,
            int numBytes,
            Stream outStream)
        {
            using (var cts = new CancellationTokenSource())
            {
                return await DeflateAsync(inStream, numBytes, outStream, cts.Token);
            }
        }

        /// <summary>
        /// Takes data from an input file stream, compresses it, and writes it to the specified
        /// output file. Streams should already seek to the proper location before calling this function.
        /// </summary>
        /// <param name="inStream">The input file stream to read from.</param>
        /// <param name="numBytes">Number of bytes to read in.</param>
        /// <param name="outStream">The output file stream to write compressed chunk to.</param>
        /// <param name="cancelToken">Cancellation token.</param>
        /// <returns>
        /// Number of bytes actually read from input file and the compressed size of the chunk that
        /// was written.
        /// </returns>
        public static async Task<(int BytesRead, int ChunkSize)> DeflateAsync(
            Stream inStream,
            int numBytes,
            Stream outStream,
            CancellationToken cancelToken)
        {
            // Read in the input data to compress
            var inData = new byte[numBytes];
            var bytesRead = await inStream.ReadAsync(inData, 0, numBytes, cancelToken);
            var chunkSize = 0;

            // Deflate stream doesn't write properly directly to a FileStream when doing this chunk
            // business. So have to do this multi-stream setup.
            using (var inMemStream = new MemoryStream())
            using (var outMemStream = new MemoryStream())
            using (var deflateStream = new DeflateStream(outMemStream, CompressionMode.Compress))
            {
                // Read input chunk into memory stream
                await inMemStream.WriteAsync(inData, 0, bytesRead, cancelToken);
                inMemStream.Seek(0, SeekOrigin.Begin);

                // Compress the chunk with deflate stream
                await inMemStream.CopyToAsync(deflateStream, bytesRead, cancelToken);

                // Close deflate stream to finalize compression - breaking the "using()" convention,
                // but this is needed
                deflateStream.Close();

                // Write the compressed chunk to the output file
                var compressedData = outMemStream.ToArray();
                chunkSize = compressedData.Length;
                await outStream.WriteAsync(compressedData, 0, compressedData.Length, cancelToken);
            }

            return (bytesRead, chunkSize);
        }
    }
}
