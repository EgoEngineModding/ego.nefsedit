// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Compression;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;
    using VictorBush.Ego.NefsLib.Utility;

    /// <summary>
    /// Handles compression and encryption for NeFS item data.
    /// </summary>
    public class NefsTransformer : INefsTransformer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsTransformer"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system to use.</param>
        public NefsTransformer(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        /// Gets the file system to use.
        /// </summary>
        private IFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public async Task DetransformAsync(
            Stream input,
            Int64 inputOffset,
            Stream output,
            Int64 outputOffset,
            uint extractedSize,
            IReadOnlyList<NefsDataChunk> chunks,
            NefsProgress p)
        {
            var numChunks = chunks.Count;
            var bytesRemaining = extractedSize;

            input.Seek(inputOffset, SeekOrigin.Begin);
            output.Seek(outputOffset, SeekOrigin.Begin);

            using (var t = p.BeginTask(1.0f, $"Detransforming stream"))
            {
                for (int i = 0; i < numChunks; i++)
                {
                    using (var st = p.BeginSubTask(1.0f / numChunks, $"Detransforming chunk {i + 1}/{numChunks}..."))
                    {
                        // Determine the maximum output size for this chunk based on expected output size
                        var maxChunkSize = Math.Min(bytesRemaining, chunks[i].Transform.ChunkSize);

                        // Revert the transform
                        var chunkSize = await this.DetransformChunkAsync(input, output, chunks[i], maxChunkSize, p);
                        bytesRemaining -= chunkSize;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task<uint> DetransformChunkAsync(
             Stream input,
             Stream output,
             NefsDataChunk chunk,
             uint maxOutputSize,
             NefsProgress p)
        {
            using var detransformedStream = new MemoryStream();
            CryptoStream cryptoStream = null;

            // Copy chunk to temp stream
            await input.CopyPartialAsync(detransformedStream, chunk.Size, p.CancellationToken);
            detransformedStream.Seek(0, SeekOrigin.Begin);

            try
            {
                // Decrypt
                if (chunk.Transform.IsAesEncrypted)
                {
                    using var aesManager = this.CreateAesManager(chunk.Transform.Aes256Key);
                    using var tempStream = new MemoryStream();

                    cryptoStream = new CryptoStream(detransformedStream, aesManager.CreateDecryptor(), CryptoStreamMode.Read);
                    await cryptoStream.CopyToAsync(tempStream, p.CancellationToken);
                    tempStream.Seek(0, SeekOrigin.Begin);

                    detransformedStream.Seek(0, SeekOrigin.Begin);
                    await tempStream.CopyToAsync(detransformedStream, p.CancellationToken);
                    detransformedStream.Seek(0, SeekOrigin.Begin);
                    detransformedStream.SetLength(tempStream.Length);
                }

                // Decompress
                if (chunk.Transform.IsZlibCompressed)
                {
                    using var inflater = new DeflateStream(detransformedStream, CompressionMode.Decompress, leaveOpen: true);
                    using var tempStream = new MemoryStream();

                    await inflater.CopyToAsync(tempStream, p.CancellationToken);
                    tempStream.Seek(0, SeekOrigin.Begin);

                    detransformedStream.Seek(0, SeekOrigin.Begin);
                    await tempStream.CopyToAsync(detransformedStream, p.CancellationToken);
                    detransformedStream.Seek(0, SeekOrigin.Begin);
                    detransformedStream.SetLength(tempStream.Length);
                }

                // Copy detransformed chunk to output stream
                var chunkSize = Math.Min(detransformedStream.Length, maxOutputSize);
                await detransformedStream.CopyPartialAsync(output, chunkSize, p.CancellationToken);
                return (uint)chunkSize;
            }
            finally
            {
                // Manually cleanup crypto stream. The .NET standard version does not have the ability to leave the underlying stream open.
                if (cryptoStream is not null)
                    cryptoStream.Dispose();
            }
        }

        /// <inheritdoc/>
        public async Task DetransformFileAsync(
            string inputFile,
            Int64 inputOffset,
            string outputFile,
            Int64 outputOffset,
            uint extractedSize,
            IReadOnlyList<NefsDataChunk> chunks,
            NefsProgress p)
        {
            using (var t = p.BeginTask(1.0f))
            using (var inputStream = this.FileSystem.File.OpenRead(inputFile))
            using (var outputStream = this.FileSystem.File.OpenWrite(outputFile))
            {
                await this.DetransformAsync(inputStream, inputOffset, outputStream, outputOffset, extractedSize, chunks, p);
            }
        }

        /// <inheritdoc/>
        public async Task<NefsItemSize> TransformAsync(
            Stream input,
            Int64 inputOffset,
            UInt32 inputLength,
            Stream output,
            Int64 outputOffset,
            NefsDataTransform transform,
            NefsProgress p)
        {
            var chunks = new List<NefsDataChunk>();
            var rawChunkSize = transform.ChunkSize;

            input.Seek(inputOffset, SeekOrigin.Begin);
            output.Seek(outputOffset, SeekOrigin.Begin);

            // Split file into chunks and transform them
            using (var t = p.BeginTask(1.0f, $"Transforming stream"))
            {
                var cumulativeChunkSize = 0U;
                var bytesRemaining = (int)inputLength;

                // Determine how many chunks to split file into
                var numChunks = (int)Math.Ceiling(inputLength / (double)rawChunkSize);

                for (var i = 0; i < numChunks; ++i)
                {
                    using (var st = p.BeginSubTask(1.0f / numChunks, $"Transforming chunk {i + 1}/{numChunks}"))
                    {
                        // The last chunk may not be exactly equal to the raw chunk size
                        var nextChunkSize = (int)Math.Min(rawChunkSize, bytesRemaining);
                        bytesRemaining -= nextChunkSize;

                        // Transform chunk and write to output stream
                        var chunkSize = await this.TransformChunkAsync(input, (uint)nextChunkSize, output, transform, p);
                        cumulativeChunkSize += chunkSize;

                        // Record chunk info



                        var chunk = new NefsDataChunk(chunkSize, cumulativeChunkSize, transform, 0); // TODO : Checksum
                        chunks.Add(chunk);
                    }
                }
            }

            // Return item size
            return new NefsItemSize(inputLength, chunks);
        }

        /// <inheritdoc/>
        public async Task<UInt32> TransformChunkAsync(
            Stream input,
            UInt32 inputChunkSize,
            Stream output,
            NefsDataTransform transform,
            NefsProgress p)
        {
            using (var transformedStream = new MemoryStream())
            {
                // Copy raw chunk to temp stream
                await input.CopyPartialAsync(transformedStream, inputChunkSize, p.CancellationToken);
                transformedStream.Seek(0, SeekOrigin.Begin);

                // Compress
                if (transform.IsZlibCompressed)
                {
                    using (var tempStream = new MemoryStream())
                    {
                        await DeflateHelper.DeflateAsync(transformedStream, (int)inputChunkSize, tempStream);
                        tempStream.Seek(0, SeekOrigin.Begin);

                        transformedStream.Seek(0, SeekOrigin.Begin);
                        await tempStream.CopyPartialAsync(transformedStream, tempStream.Length, p.CancellationToken);
                        transformedStream.Seek(0, SeekOrigin.Begin);
                        transformedStream.SetLength(tempStream.Length);
                    }
                }

                // Encrypt
                if (transform.IsAesEncrypted)
                {
                    using (var aesManager = this.CreateAesManager(transform.Aes256Key))
                    using (var cryptoStream = new CryptoStream(transformedStream, aesManager.CreateEncryptor(), CryptoStreamMode.Read))
                    using (var tempStream = new MemoryStream())
                    {
                        await cryptoStream.CopyToAsync(tempStream, p.CancellationToken);
                        tempStream.Seek(0, SeekOrigin.Begin);

                        transformedStream.Seek(0, SeekOrigin.Begin);
                        await tempStream.CopyPartialAsync(transformedStream, tempStream.Length, p.CancellationToken);
                        transformedStream.Seek(0, SeekOrigin.Begin);
                        transformedStream.SetLength(tempStream.Length);
                    }
                }

                // Copy transformed chunk to output stream
                await transformedStream.CopyToAsync(output, p.CancellationToken);

                // Return size of transformed chunk
                return (uint)transformedStream.Length;
            }
        }

        /// <inheritdoc/>
        public async Task<NefsItemSize> TransformFileAsync(
            string inputFile,
            string outputFile,
            NefsDataTransform transform,
            NefsProgress p)
        {
            using (var inputStream = this.FileSystem.File.OpenRead(inputFile))
            using (var outputStream = this.FileSystem.File.OpenWrite(outputFile))
            {
                return await this.TransformAsync(inputStream, 0, (uint)inputStream.Length, outputStream, 0, transform, p);
            }
        }

        /// <inheritdoc/>
        public async Task<NefsItemSize> TransformFileAsync(
            INefsDataSource input,
            string outputFile,
            NefsDataTransform transform,
            NefsProgress p)
        {
            return await this.TransformFileAsync(input.FilePath, outputFile, transform, p);
        }

        /// <inheritdoc/>
        public async Task<NefsItemSize> TransformFileAsync(
            string inputFile,
            Int64 inputOffset,
            UInt32 inputLength,
            Stream output,
            Int64 outputOffset,
            NefsDataTransform transform,
            NefsProgress p)
        {
            using (var inputStream = this.FileSystem.File.OpenRead(inputFile))
            {
                return await this.TransformAsync(inputStream, inputOffset, inputLength, output, outputOffset, transform, p);
            }
        }

        /// <inheritdoc/>
        public async Task<NefsItemSize> TransformFileAsync(
            string inputFile,
            Int64 inputOffset,
            UInt32 inputLength,
            string outputFile,
            Int64 outputOffset,
            NefsDataTransform transform,
            NefsProgress p)
        {
            using (var inputStream = this.FileSystem.File.OpenRead(inputFile))
            using (var outputStream = this.FileSystem.File.OpenWrite(outputFile))
            {
                return await this.TransformAsync(inputStream, inputOffset, inputLength, outputStream, outputOffset, transform, p);
            }
        }

        private RijndaelManaged CreateAesManager(byte[] aes256Key)
        {
            var rijAlg = new RijndaelManaged
            {
                KeySize = 256,
                Key = aes256Key,
                Mode = CipherMode.ECB,
                BlockSize = 128,
                Padding = PaddingMode.Zeros,
            };

            return rijAlg;
        }
    }
}
