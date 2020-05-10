// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 4.
    /// </summary>
    public class Nefs20HeaderPart4 : INefsHeaderPart4
    {
        private readonly List<Nefs20HeaderPart4Entry> entriesByIndex;
        private readonly Dictionary<Guid, uint> indexLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs20HeaderPart4"/> class.
        /// </summary>
        /// <param name="entries">A collection of entries to initialize this object with.</param>
        /// <param name="indexLookup">
        /// A dictionary that matches an item Guid to a part 4 index. This is used to find the
        /// correct index part 4 value for an item.
        /// </param>
        internal Nefs20HeaderPart4(IEnumerable<Nefs20HeaderPart4Entry> entries, Dictionary<Guid, uint> indexLookup)
        {
            this.entriesByIndex = new List<Nefs20HeaderPart4Entry>(entries);
            this.indexLookup = new Dictionary<Guid, uint>(indexLookup);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs20HeaderPart4"/> class from a list of items.
        /// </summary>
        /// <param name="items">The items to initialize from.</param>
        internal Nefs20HeaderPart4(NefsItemList items)
        {
            this.entriesByIndex = new List<Nefs20HeaderPart4Entry>();
            this.indexLookup = new Dictionary<Guid, uint>();

            var nextStartIdx = 0U;

            foreach (var item in items.EnumerateById())
            {
                if (item.DataSource.Size.ExtractedSize == item.DataSource.Size.TransformedSize)
                {
                    // Item does not have a part 4 entry since it has no compressed data
                    continue;
                }

                // Log this start index to item's Guid to allow lookup later
                this.indexLookup.Add(item.Guid, nextStartIdx);

                // Create entry for each data chunk
                foreach (var chunk in item.DataSource.Size.Chunks)
                {
                    // Create entry
                    var entry = new Nefs20HeaderPart4Entry();
                    entry.Data0x00_CumulativeChunkSize.Value = chunk.CumulativeSize;
                    this.entriesByIndex.Add(entry);

                    nextStartIdx++;
                }
            }
        }

        /// <summary>
        /// List of data chunk info in order as they appear in the header.
        /// </summary>
        public IReadOnlyList<Nefs20HeaderPart4Entry> EntriesByIndex => this.entriesByIndex;

        /// <summary>
        /// Gets the current size of header part 4.
        /// </summary>
        public UInt32 Size => (uint)(this.entriesByIndex.Count * Nefs20HeaderPart4Entry.Size);

        /// <summary>
        /// Creates a list of chunk metadata for an item.
        /// </summary>
        /// <param name="index">The part 4 index where the chunk list starts at.</param>
        /// <param name="numChunks">The number of chunks.</param>
        /// <param name="transform">The transform used for data chunks.</param>
        /// <returns>A list of chunk data.</returns>
        public List<NefsDataChunk> CreateChunksList(uint index, uint numChunks, NefsDataTransform transform)
        {
            var chunks = new List<NefsDataChunk>();

            for (var i = index; i < index + numChunks; ++i)
            {
                var cumulativeSize = this.entriesByIndex[(int)i].CumulativeChunkSize;
                var size = cumulativeSize;

                if (i > index)
                {
                    size -= this.entriesByIndex[(int)i - 1].CumulativeChunkSize;
                }

                var chunk = new NefsDataChunk(size, cumulativeSize, transform);
                chunks.Add(chunk);
            }

            return chunks;
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
                return this.indexLookup[item.Guid];
            }
        }
    }
}
