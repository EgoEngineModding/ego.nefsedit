// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Stores information about a item's data size.
    /// </summary>
    public class NefsItemSize
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsItemSize"/> class.
        /// </summary>
        /// <param name="extractedSize">The size of the item's data when extracted from the archive.</param>
        /// <param name="chunkSizes">
        /// List of cumulative chunk sizes. If the item is not compressed, this list should only
        /// have one entry equal to the extracted size.)
        /// </param>
        public NefsItemSize(UInt32 extractedSize, IReadOnlyList<UInt32> chunkSizes)
        {
            this.ExtractedSize = extractedSize;
            this.ChunkSizes = chunkSizes ?? new List<UInt32>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsItemSize"/> class. This constructor can
        /// be used if the item is not compressed (i.e., extracted size == compressed size).
        /// </summary>
        /// <param name="extractedSize">The size of the item's data when extracted from the archive.</param>
        public NefsItemSize(UInt32 extractedSize)
        {
            this.ExtractedSize = extractedSize;
            this.ChunkSizes = new List<UInt32> { extractedSize };
        }

        /// <summary>
        /// <para>List of cumulative chunk sizes.</para>
        /// <list type="bullet">
        /// <item>First entry is size of first chunk.</item>
        /// <item>Second entry is size of first + second chunk.</item>
        /// <item>Last entry is size of all chunks together.</item>
        /// </list>
        /// <para>
        /// To get the size of a specific chunk, simply subtract the previous chunk size entry. If
        /// the item is not compressed, the list should only have one entry equal to the extracted size.
        /// </para>
        /// </summary>
        /// <remarks>Stored in header part 4.</remarks>
        public IReadOnlyList<UInt32> ChunkSizes { get; }

        /// <summary>
        /// Gets the size of the data when uncompressed. If the data is not compressed, this will
        /// equal <see cref="Size"/>.
        /// </summary>
        public UInt32 ExtractedSize { get; }

        /// <summary>
        /// Gets a value indicating whether the item's data is compressed in the archive.
        /// </summary>
        public bool IsCompressed => this.Size != this.ExtractedSize;

        /// <summary>
        /// Gets the size of the data in bytes.
        /// </summary>
        public UInt32 Size => this.ChunkSizes.LastOrDefault();
    }
}
