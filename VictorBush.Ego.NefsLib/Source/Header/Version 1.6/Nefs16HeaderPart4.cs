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
    public class Nefs16HeaderPart4
    {
        private readonly SortedDictionary<uint, Nefs16HeaderPart4Entry> entriesByIndex;

        private readonly Dictionary<NefsItemId, uint> indexById;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart4"/> class.
        /// </summary>
        /// <param name="entries">A collection of entries to initialize this object with.</param>
        internal Nefs16HeaderPart4(IDictionary<UInt32, Nefs16HeaderPart4Entry> entries)
        {
            this.entriesByIndex = new SortedDictionary<UInt32, Nefs16HeaderPart4Entry>(entries);
            this.indexById = new Dictionary<NefsItemId, UInt32>(this.entriesByIndex.ToDictionary(i => i.Value.Id, i => i.Key));

            // Compute size
            this.ComputeSize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart4"/> class from a list of items.
        /// </summary>
        /// <param name="items">The items to initialize from.</param>
        internal Nefs16HeaderPart4(NefsItemList items)
        {
            this.entriesByIndex = new SortedDictionary<UInt32, Nefs16HeaderPart4Entry>();
            this.indexById = new Dictionary<NefsItemId, UInt32>();

            var nextIdx = 0;
            foreach (var item in items.EnumerateById())
            {
                //if (!item.DataSource.Size.IsCompressed)
                //{
                //    // Item does not have a part 4 entry since it has no compressed data
                //    continue;
                //}

                //// Create entry
                //var entry = new Nefs16HeaderPart4Entry(item.Id);
                //entry.Chunks.AddRange(item.DataSource.Size.ChunkSizes);

                //// Add to entries list and advance index
                //this.entriesByIndex.Add((uint)nextIdx, entry);
                //this.indexById.Add(item.Id, (uint)nextIdx);
                //nextIdx += entry.ChunkSizes.Count;
            }

            // Compute size
            this.ComputeSize();
        }

        /// <summary>
        /// Gets the list of entries in the correct order.
        /// </summary>
        public IEnumerable<Nefs16HeaderPart4Entry> Entries => this.entriesByIndex.Values;

        /// <summary>
        /// The dictionary of chunk sizes lists. The key is the index into the of chunks sizes. The
        /// value is the part 4 entry for that item.
        /// </summary>
        public IReadOnlyDictionary<UInt32, Nefs16HeaderPart4Entry> EntriesByIndex => this.entriesByIndex;

        /// <summary>
        /// Gets the current size of header part 4.
        /// </summary>
        public UInt32 Size { get; private set; }

        ///// <summary>
        ///// Gets a copy of the chunk sizes list for an item.
        ///// </summary>
        ///// <param name="item">The item to get chunk sizes for.</param>
        ///// <returns>The list of chunk sizes.</returns>
        //public List<NefsDataChunk> GetChunksForItem(NefsItem item)
        //{
        //    if (item.Type == NefsItemType.Directory)
        //    {
        //        // Item is a directory; no chunk sizes
        //        return new List<NefsDataChunk>();
        //    }
        //    else if (item.ExtractedSize == item.CompressedSize)
        //    {
        //        //TODO: this is not correct

        //        // Item is uncompressed; no chunk sizes
        //        return new List<NefsDataChunk>();
        //    }
        //    else
        //    {
        //        // Item is compressed; get chunk sizes
        //        var idx = this.indexById[item.Id];

        //        // Use ToList() to create a copy of the list
        //        return this.entriesByIndex[idx].Chunks.ToList();
        //    }
        //}

        /// <summary>
        /// Gets the index into part 4 for the specified item. The index into part 4 is potentially
        /// different from the item's id.
        /// </summary>
        /// <param name="item">The item to get the index for.</param>
        /// <returns>The index into part 4.</returns>
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
                //TODO: This is not correct for v1.6

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
                this.Size += (uint)(entry.Chunks.Count * Nefs16HeaderPart4Chunk.Size);
            }
        }
    }
}
