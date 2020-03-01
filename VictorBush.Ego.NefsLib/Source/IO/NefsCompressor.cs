// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;
    using VictorBush.Ego.NefsLib.Utility;

    /// <summary>
    /// Compresses and decompresses NeFS item data.
    /// </summary>
    public class NefsCompressor : INefsCompressor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsCompressor"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system to use.</param>
        public NefsCompressor(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        /// Gets the file system to use.
        /// </summary>
        private IFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public async Task<NefsItemSize> CompressAsync(
            Stream input,
            Int64 inputOffset,
            UInt32 inputLength,
            Stream output,
            Int64 outputOffset,
            UInt32 chunkSize,
            NefsProgress p)
        {
            var chunkSizes = new List<UInt32>();

            input.Seek(inputOffset, SeekOrigin.Begin);
            output.Seek(outputOffset, SeekOrigin.Begin);

            // Split file into chunks and compress them
            using (var t = p.BeginTask(1.0f, $"Compressing stream"))
            {
                int lastBytesRead = 0;
                int lastChunkSize = 0;
                int totalChunkSize = 0;
                var currentChunk = 0;

                // Determine how many chunks to split file into
                var numChunks = (int)Math.Ceiling(inputLength / (double)chunkSize);

                do
                {
                    using (var st = p.BeginSubTask(1.0f / numChunks, $"Compressing chunk {currentChunk + 1}/{numChunks}"))
                    {
                        // Compress this chunk and write it to the output file
                        (lastBytesRead, lastChunkSize) = await DeflateHelper.DeflateAsync(input, (int)chunkSize, output, p.CancellationToken);

                        totalChunkSize += lastChunkSize;
                        currentChunk++;

                        // Record the total compressed size after this chunk
                        chunkSizes.Add((UInt32)totalChunkSize);
                    }
                }
                while (lastBytesRead == chunkSize);
            }

            // Return item size
            return new NefsItemSize(inputLength, chunkSizes);
        }

        /// <inheritdoc/>
        public Task<NefsItemSize> CompressFileAsync(
            string inputFile,
            string outputFile,
            UInt32 chunkSize,
            NefsProgress p)
        {
            using (var inputStream = this.FileSystem.File.OpenRead(inputFile))
            using (var outputStream = this.FileSystem.File.OpenRead(outputFile))
            {
                return this.CompressAsync(inputStream, 0, (uint)inputStream.Length, outputStream, 0, chunkSize, p);
            }
        }

        /// <inheritdoc/>
        public Task<NefsItemSize> CompressFileAsync(
            INefsDataSource input,
            string outputFile,
            UInt32 chunkSize,
            NefsProgress p)
        {
            return this.CompressFileAsync(input.FilePath, outputFile, chunkSize, p);
        }

        /// <inheritdoc/>
        public Task<NefsItemSize> CompressFileAsync(
            string inputFile,
            Int64 inputOffset,
            UInt32 inputLength,
            Stream output,
            Int64 outputOffset,
            UInt32 chunkSize,
            NefsProgress p)
        {
            using (var inputStream = this.FileSystem.File.OpenRead(inputFile))
            {
                return this.CompressAsync(inputStream, inputOffset, inputLength, output, outputOffset, chunkSize, p);
            }
        }

        /// <inheritdoc/>
        public Task<NefsItemSize> CompressFileAsync(
            string inputFile,
            Int64 inputOffset,
            UInt32 inputLength,
            string outputFile,
            Int64 outputOffset,
            UInt32 chunkSize,
            NefsProgress p)
        {
            using (var inputStream = this.FileSystem.File.OpenRead(inputFile))
            using (var outputStream = this.FileSystem.File.OpenRead(outputFile))
            {
                return this.CompressAsync(inputStream, inputOffset, inputLength, outputStream, outputOffset, chunkSize, p);
            }
        }
    }
}
