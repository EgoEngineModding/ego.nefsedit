// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataSource
{
    using System;

    /// <summary>
    /// Describes a transformation that is applied to a file before being put in the archive..
    /// </summary>
    public class NefsDataTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsDataTransform"/> class.
        /// </summary>
        /// <param name="chunkSize">The chunk size to use.</param>
        /// <param name="isZlib">Whether to compress with zlib.</param>
        /// <param name="aesKey">The AES-256 key for encryption. Use null if not encrypted.</param>
        public NefsDataTransform(UInt32 chunkSize, bool isZlib, byte[] aesKey = null)
        {
            this.ChunkSize = chunkSize;
            this.IsZlibCompressed = isZlib;
            this.IsAesEncrypted = aesKey != null;
            this.Aes256Key = aesKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsDataTransform"/> class. This transform
        /// does nothing (file is simply placed in the archive as-is).
        /// </summary>
        /// <param name="fileSize">The file size.</param>
        public NefsDataTransform(UInt32 fileSize)
        {
            this.ChunkSize = fileSize;
            this.IsZlibCompressed = false;
            this.IsAesEncrypted = false;
            this.Aes256Key = null;
        }

        /// <summary>
        /// The AES 256 key. If the chunk is not encrypted, this will be null.
        /// </summary>
        public byte[] Aes256Key { get; }

        /// <summary>
        /// The size of chunks to split the input file up into before transforming each chunk.
        /// </summary>
        public UInt32 ChunkSize { get; }

        /// <summary>
        /// Whether data chunks are AES encrypted.
        /// </summary>
        public bool IsAesEncrypted { get; }

        /// <summary>
        /// Whether data chunks are zlib compressed.
        /// </summary>
        public bool IsZlibCompressed { get; }
    }
}
