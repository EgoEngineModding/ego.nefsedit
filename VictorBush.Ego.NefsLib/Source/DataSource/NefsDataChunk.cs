// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataSource
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a chunk of data in an archive.
    /// </summary>
    public class NefsDataChunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsDataChunk"/> class.
        /// </summary>
        /// <param name="size">The size of the data chunk.</param>
        /// <param name="cumulativeSize">The cumulative size of the data chunk.</param>
        /// <param name="transform">The transform that has been applied to this chunk.</param>
        public NefsDataChunk(UInt32 size, UInt32 cumulativeSize, NefsDataTransform transform, ushort checksum = 0)
        {
            this.Size = size;
            this.CumulativeSize = cumulativeSize;
            this.Transform = transform ?? throw new ArgumentNullException(nameof(transform));
            this.Checksum = checksum;
        }

        /// <summary>
        /// The cumulative size of the chunk. This is equal to the size of this chunk + the size of
        /// all the previous chunks.
        /// </summary>
        public UInt32 CumulativeSize { get; }

        /// <summary>
        /// The size of the chunk.
        /// </summary>
        public UInt32 Size { get; }

        /// <summary>
        /// The transform that has been applied to this chunk.
        /// </summary>
        public NefsDataTransform Transform { get; }
        public UInt16 Checksum { get; }

        /// <summary>
        /// Creats a list of chunks given a list of cumulative chunk sizes.
        /// </summary>
        /// <param name="cumulativeSizes">List of cumulative chunk sizes.</param>
        /// <param name="transform">The transform applied to all chunks.</param>
        /// <returns>A list of chunks.</returns>
        public static List<NefsDataChunk> CreateChunkList(
            IReadOnlyList<UInt32> cumulativeSizes,
            NefsDataTransform transform)
        {
            var chunks = new List<NefsDataChunk>();

            for (var i = 0; i < cumulativeSizes.Count; ++i)
            {
                var size = cumulativeSizes[i];

                // Get individual size by subtracting previous cumulative size
                if (i > 0)
                {
                    size -= cumulativeSizes[i - 1];
                }

                // TODO - checksum???
                var chunk = new NefsDataChunk(size, cumulativeSizes[i], transform, 0);
                chunks.Add(chunk);
            }

            return chunks;
        }
    }
}
