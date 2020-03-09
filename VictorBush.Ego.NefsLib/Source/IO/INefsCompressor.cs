// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// Provides compression and decrompression for item data.
    /// </summary>
    public interface INefsCompressor
    {
        /// <summary>
        /// Compresses data from an input stream into an output stream.
        /// </summary>
        /// <param name="input">The stream containing data to compress.</param>
        /// <param name="inputOffset">
        /// The absolute offset from the beginning of the input stream to compress.
        /// </param>
        /// <param name="inputLength">The number of bytes to compress.</param>
        /// <param name="output">The stream to write the compressed data to.</param>
        /// <param name="outputOffset">
        /// The absolute offset from the beginning of the output stream to write to.
        /// </param>
        /// <param name="chunkSize">The size of chunks to split the input into.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The compressed item size.</returns>
        Task<NefsItemSize> CompressAsync(
            Stream input,
            long inputOffset,
            UInt32 inputLength,
            Stream output,
            long outputOffset,
            UInt32 chunkSize,
            NefsProgress p);

        /// <summary>
        /// Compresses an entire file to a specified output file.
        /// </summary>
        /// <param name="inputFile">The path to the input file.</param>
        /// <param name="outputFile">The output file to write to.</param>
        /// <param name="chunkSize">The size of chunks to split the input into.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The compressed item size.</returns>
        Task<NefsItemSize> CompressFileAsync(
            string inputFile,
            string outputFile,
            UInt32 chunkSize,
            NefsProgress p);

        /// <summary>
        /// Compresses an entire file to a specified output file.
        /// </summary>
        /// <param name="input">The input data to compress.</param>
        /// <param name="outputFile">The output file to write to.</param>
        /// <param name="chunkSize">The size of chunks to split the input into.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The compressed item size.</returns>
        Task<NefsItemSize> CompressFileAsync(
            INefsDataSource input,
            string outputFile,
            UInt32 chunkSize,
            NefsProgress p);

        /// <summary>
        /// Compresses data from a file on disk to an output stream.
        /// </summary>
        /// <param name="inputFile">The path to the input file.</param>
        /// <param name="inputOffset">
        /// The absolute offset from the beginning of the input file to compress.
        /// </param>
        /// <param name="inputLength">The number of bytes to compress.</param>
        /// <param name="output">The output stream to write to.</param>
        /// <param name="outputOffset">
        /// The absolute offset from the beginning of the output stream to write to.
        /// </param>
        /// <param name="chunkSize">The size of chunks to split the input into.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The compressed item size.</returns>
        Task<NefsItemSize> CompressFileAsync(
            string inputFile,
            long inputOffset,
            UInt32 inputLength,
            Stream output,
            long outputOffset,
            UInt32 chunkSize,
            NefsProgress p);

        /// <summary>
        /// Compresses data from a file on disk to a specified output file.
        /// </summary>
        /// <param name="inputFile">The path to the input file.</param>
        /// <param name="inputOffset">
        /// The absolute offset from the beginning of the input file to compress.
        /// </param>
        /// <param name="inputLength">The number of bytes to compress.</param>
        /// <param name="outputFile">The output file to write to.</param>
        /// <param name="outputOffset">
        /// The absolute offset from the beginning of the output stream to write to.
        /// </param>
        /// <param name="chunkSize">The size of chunks to split the input into.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The compressed data source.</returns>
        Task<NefsItemSize> CompressFileAsync(
            string inputFile,
            long inputOffset,
            UInt32 inputLength,
            string outputFile,
            long outputOffset,
            UInt32 chunkSize,
            NefsProgress p);

        /// <summary>
        /// Decompresses a stream of data.
        /// </summary>
        /// <param name="input">The input stream to decompress.</param>
        /// <param name="inputOffset">
        /// The absolute offset from the beginning of the input stream to decompress.
        /// </param>
        /// <param name="chunkSizes">The list of compressed chunk sizes.</param>
        /// <param name="output">The output stream to write to.</param>
        /// <param name="outputOffset">
        /// The absolute offset from the beginning of the output stream to write to.
        /// </param>
        /// <param name="p">Progress info.</param>
        /// <param name="aes256key">The AES 256 key, if the data is encrypted. If not, use null.</param>
        /// <returns>An async task.</returns>
        Task DecompressAsync(
            Stream input,
            Int64 inputOffset,
            IReadOnlyList<UInt32> chunkSizes,
            Stream output,
            Int64 outputOffset,
            NefsProgress p,
            byte[] aes256key = null);

        /// <summary>
        /// Decompresses a chunk of data.
        /// </summary>
        /// <param name="chunk">The compressed chunk.</param>
        /// <param name="output">The output stream to write to.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async Task.</returns>
        Task DecompressChunkAsync(
            byte[] chunk,
            Stream output,
            NefsProgress p);

        /// <summary>
        /// Decrypts and decompresses a chunk of data.
        /// </summary>
        /// <param name="chunk">The compressed chunk.</param>
        /// <param name="aes256key">The AES 256 key.</param>
        /// <param name="output">The output stream to write to.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async Task.</returns>
        Task DecompressChunkAsync(
            byte[] chunk,
            byte[] aes256key,
            Stream output,
            NefsProgress p);
    }
}
