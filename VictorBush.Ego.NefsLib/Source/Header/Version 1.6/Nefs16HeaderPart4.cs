// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 4.
    /// </summary>
    public class Nefs16HeaderPart4 : INefsHeaderPart4
    {
        private static readonly ILogger Log = NefsLog.GetLogger();
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
                if (item.Type == NefsItemType.Directory)
                {
                    // Item does not have a part 4 entry
                    continue;
                }

                // Create entry
                var entry = new Nefs16HeaderPart4Entry(item.Id);

                // Process chunks
                foreach (var chunk in item.DataSource.Size.Chunks)
                {
                    var c = new Nefs16HeaderPart4Chunk();
                    c.Data0x00_CumulativeBlockSize.Value = chunk.CumulativeSize;
                    c.Data0x04_TransformType.Value = (ushort)this.GetTransformType(chunk.Transform);
                    c.Data0x06_Checksum.Value = 0; // TODO
                    entry.Chunks.Add(c);
                }

                // Add to entries list and advance index
                this.entriesByIndex.Add((uint)nextIdx, entry);
                this.indexById.Add(item.Id, (uint)nextIdx);
                nextIdx += entry.Chunks.Count;
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

        /// <summary>
        /// Creates a list of chunk metadata for an item.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="aes256key">The AES 256 key to use if chunk is encrypted.</param>
        /// <returns>A list of chunk data.</returns>
        public List<NefsDataChunk> CreateChunksListForItem(NefsItemId id, byte[] aes256key)
        {
            if (!this.indexById.ContainsKey(id))
            {
                return new List<NefsDataChunk>();
            }

            var chunks = new List<NefsDataChunk>();
            var idx = this.indexById[id];
            var entry = this.entriesByIndex[idx];

            for (var i = 0; i < entry.Chunks.Count; ++i)
            {
                var cumulativeSize = entry.Chunks[i].CumulativeBlockSize;
                var size = cumulativeSize;

                if (i > 0)
                {
                    size -= entry.Chunks[i - 1].CumulativeBlockSize;
                }

                // Determine transform -- need to clean this up
                NefsDataTransform transform;
                var transformVal = entry.Chunks[i].Data0x04_TransformType.Value;

                switch (transformVal)
                {
                    case (int)Nefs16HeaderPart4TransformType.Zlib:
                        transform = new NefsDataTransform(Nefs16HeaderIntroToc.ChunkSize, true);
                        break;

                    case (int)Nefs16HeaderPart4TransformType.Aes:
                        transform = new NefsDataTransform(Nefs16HeaderIntroToc.ChunkSize, false, aes256key);
                        break;

                    case (int)Nefs16HeaderPart4TransformType.None:
                        transform = new NefsDataTransform(Nefs16HeaderIntroToc.ChunkSize, false);
                        break;

                    default:
                        Log.LogError("Found v1.6 data chunk with unknown transform; aborting.");
                        return new List<NefsDataChunk>();
                }

                // Create data chunk info
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
            else
            {
                // Get index into part 4
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

        private Nefs16HeaderPart4TransformType GetTransformType(NefsDataTransform transform)
        {
            // Can v1.6 have both aes and zlib simulatneously?
            if (transform.IsAesEncrypted && transform.IsZlibCompressed)
            {
                Log.LogWarning("Found multiple data transforms for header part 4 entry.");
            }

            if (transform.IsAesEncrypted)
            {
                return Nefs16HeaderPart4TransformType.Aes;
            }
            else if (transform.IsZlibCompressed)
            {
                return Nefs16HeaderPart4TransformType.Zlib;
            }

            return Nefs16HeaderPart4TransformType.None;
        }
    }
}
