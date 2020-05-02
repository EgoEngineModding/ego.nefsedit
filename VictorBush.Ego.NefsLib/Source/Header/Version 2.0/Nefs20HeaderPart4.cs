// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 4.
    /// </summary>
    public class Nefs20HeaderPart4 : INefsHeaderPart4
    {
        /// <summary>
        /// The size of a peice of data in a part 4 entry. This is used to get the offset into part
        /// 4 from an index into part 4.
        /// </summary>
        public const int DataSize = 0x04;

        private readonly SortedDictionary<uint, Nefs20HeaderPart4Entry> entriesByIndex;

        private readonly Dictionary<NefsItemId, uint> indexById;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs20HeaderPart4"/> class.
        /// </summary>
        /// <param name="entries">A collection of entries to initialize this object with.</param>
        internal Nefs20HeaderPart4(IDictionary<UInt32, Nefs20HeaderPart4Entry> entries)
        {
            this.entriesByIndex = new SortedDictionary<UInt32, Nefs20HeaderPart4Entry>(entries);
            this.indexById = new Dictionary<NefsItemId, UInt32>(this.entriesByIndex.ToDictionary(i => i.Value.Id, i => i.Key));

            // Compute size
            this.ComputeSize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs20HeaderPart4"/> class from a list of items.
        /// </summary>
        /// <param name="items">The items to initialize from.</param>
        internal Nefs20HeaderPart4(NefsItemList items)
        {
            this.entriesByIndex = new SortedDictionary<UInt32, Nefs20HeaderPart4Entry>();
            this.indexById = new Dictionary<NefsItemId, UInt32>();

            var nextIdx = 0;
            foreach (var item in items.EnumerateById())
            {
                if (item.DataSource.Size.ExtractedSize == item.DataSource.Size.TransformedSize)
                {
                    // Item does not have a part 4 entry since it has no compressed data
                    continue;
                }

                // Create entry
                var entry = new Nefs20HeaderPart4Entry(item.Id);
                entry.ChunkSizes.AddRange(item.DataSource.Size.Chunks.Select(c => c.CumulativeSize));

                // Add to entries list and advance index
                this.entriesByIndex.Add((uint)nextIdx, entry);
                this.indexById.Add(item.Id, (uint)nextIdx);
                nextIdx += entry.ChunkSizes.Count;
            }

            // Compute size
            this.ComputeSize();
        }

        /// <summary>
        /// Gets the list of entries in the correct order.
        /// </summary>
        public IEnumerable<Nefs20HeaderPart4Entry> Entries => this.entriesByIndex.Values;

        /// <summary>
        /// The dictionary of chunk sizes lists. The key is the index into the of chunks sizes. The
        /// value is the part 4 entry for that item.
        /// </summary>
        public IReadOnlyDictionary<UInt32, Nefs20HeaderPart4Entry> EntriesByIndex => this.entriesByIndex;

        /// <summary>
        /// Gets the current size of header part 4.
        /// </summary>
        public UInt32 Size { get; private set; }

        /// <summary>
        /// Creates a list of chunk metadata for an item.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="transform">The transform used for data chunks.</param>
        /// <returns>A list of chunk data.</returns>
        public List<NefsDataChunk> CreateChunksListForItem(NefsItemId id, NefsDataTransform transform)
        {
            if (!this.indexById.ContainsKey(id))
            {
                return new List<NefsDataChunk>();
            }

            var chunks = new List<NefsDataChunk>();
            var idx = this.indexById[id];
            var entry = this.entriesByIndex[idx];

            for (var i = 0; i < entry.ChunkSizes.Count; ++i)
            {
                var cumulativeSize = entry.ChunkSizes[i];
                var size = cumulativeSize;

                if (i > 0)
                {
                    size -= entry.ChunkSizes[i - 1];
                }

                var chunk = new NefsDataChunk(size, cumulativeSize, transform);
                chunks.Add(chunk);
            }

            return chunks;
        }

        /// <summary>
        /// Gets a copy of the chunk sizes list for an item.
        /// </summary>
        /// <param name="item">The item to get chunk sizes for.</param>
        /// <returns>The list of chunk sizes.</returns>
        public List<UInt32> GetChunkSizesForItem(NefsItem item)
        {
            if (item.Type == NefsItemType.Directory)
            {
                // Item is a directory; no chunk sizes
                return new List<UInt32>();
            }
            else if (item.ExtractedSize == item.CompressedSize)
            {
                // Item is uncompressed; no chunk sizes
                return new List<UInt32>();
            }
            else
            {
                // Item is compressed; get chunk sizes
                var idx = this.indexById[item.Id];

                // Use ToList() to create a copy of the list
                return this.entriesByIndex[idx].ChunkSizes.ToList();
            }
        }

        /// <inheritdoc/>
        public UInt32 GetIndexForItem(NefsItem item)
        {
            // Get index to part 4
            if (item.Type == NefsItemType.Directory)
            {
                // Item is a directory; the index 0
                return 0;
            }
            else if (item.ExtractedSize == item.CompressedSize)
            {
                // Item is uncompressed; the index is -1 (0xFFFFFFFF)
                return 0xFFFFFFFF;
            }
            else
            {
                // Item is compressed; get index into part 4
                return this.indexById[item.Id];
            }
        }

        /// <summary>
        /// Calculates the total size of header part 4 in bytes.
        /// </summary>
        private void ComputeSize()
        {
            this.Size = 0;
            foreach (var entry in this.entriesByIndex.Values)
            {
                this.Size += (uint)(entry.ChunkSizes.Count * DataSize);
            }
        }
    }
}
