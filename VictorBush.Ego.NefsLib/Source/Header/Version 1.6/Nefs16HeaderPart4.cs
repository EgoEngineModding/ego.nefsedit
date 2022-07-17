// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 4.
    /// </summary>
    public class Nefs16HeaderPart4 : INefsHeaderPart4
    {
        private static readonly ILogger Log = NefsLog.GetLogger();
        private readonly List<Nefs16HeaderPart4Entry> entriesByIndex;
        private readonly Dictionary<Guid, uint> indexLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart4"/> class.
        /// </summary>
        /// <param name="entries">A collection of entries to initialize this object with.</param>
        /// <param name="indexLookup">
        /// A dictionary that matches an item Guid to a part 4 index. This is used to find the
        /// correct index part 4 value for an item.
        /// </param>
        internal Nefs16HeaderPart4(IEnumerable<Nefs16HeaderPart4Entry> entries, Dictionary<Guid, uint> indexLookup, uint unkownEndValue)
        {
            this.entriesByIndex = new List<Nefs16HeaderPart4Entry>(entries);
            this.indexLookup = new Dictionary<Guid, uint>(indexLookup);
            this.UnkownEndValue = unkownEndValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart4"/> class from a list of items.
        /// </summary>
        /// <param name="items">The items to initialize from.</param>
        internal Nefs16HeaderPart4(NefsItemList items, uint unkownEndValue)
        {
            this.entriesByIndex = new List<Nefs16HeaderPart4Entry>();
            this.indexLookup = new Dictionary<Guid, uint>();
            this.UnkownEndValue = unkownEndValue;

            var nextStartIdx = 0U;

            foreach (var item in items.EnumerateById())
            {
                if (item.Type == NefsItemType.Directory || item.DataSource.Size.Chunks.Count == 0)
                {
                    // Item does not have a part 4 entry
                    continue;
                }

                // Log this start index to item's Guid to allow lookup later
                this.indexLookup.Add(item.Guid, nextStartIdx);

                // Create entry for each data chunk
                foreach (var chunk in item.DataSource.Size.Chunks)
                {
                    // Create entry
                    var entry = new Nefs16HeaderPart4Entry();
                    entry.Data0x00_CumulativeBlockSize.Value = chunk.CumulativeSize;
                    entry.Data0x04_TransformType.Value = (ushort)this.GetTransformType(chunk.Transform);
                    entry.Data0x06_Checksum.Value = 0x848; // TODO
                    this.entriesByIndex.Add(entry);

                    nextStartIdx++;
                }
            }
        }

        /// <summary>
        /// List of data chunk info in order as they appear in the header.
        /// </summary>
        public IReadOnlyList<Nefs16HeaderPart4Entry> EntriesByIndex => this.entriesByIndex;

        public const int EntrySize = 0x8;

        public const int LastValueSize = 0x4;

        /// <summary>
        /// Gets the current size of header part 4.
        /// </summary>
        public int Size => (this.entriesByIndex.Count * EntrySize) + LastValueSize;

        IReadOnlyList<INefsHeaderPartEntry> INefsHeaderPart4.EntriesByIndex => this.entriesByIndex;

        /// <summary>
        /// There is a 4-byte value at the end of header part 4. Purpose unknown.
        /// </summary>
        public uint UnkownEndValue { get; }

        /// <summary>
        /// Creates a list of chunk metadata for an item.
        /// </summary>
        /// <param name="index">The part 4 index where the chunk list starts at.</param>
        /// <param name="numChunks">The number of chunks.</param>
        /// <param name="chunkSize">The raw chunk size used in the transform.</param>
        /// <param name="aes256key">The AES 256 key to use if chunk is encrypted.</param>
        /// <returns>A list of chunk data.</returns>
        public List<NefsDataChunk> CreateChunksList(uint index, uint numChunks, uint chunkSize, byte[] aes256key)
        {
            var chunks = new List<NefsDataChunk>();

            for (var i = index; i < index + numChunks; ++i)
            {
                var entry = this.entriesByIndex[(int)i];
                var cumulativeSize = entry.CumulativeBlockSize;
                var size = cumulativeSize;

                if (i > index)
                {
                    size -= this.entriesByIndex[(int)i - 1].CumulativeBlockSize;
                }

                // Determine transform -- need to clean this up
                NefsDataTransform transform;
                var transformVal = entry.Data0x04_TransformType.Value;

                switch (transformVal)
                {
                    case (int)Nefs16HeaderPart4TransformType.Zlib:
                        transform = new NefsDataTransform(chunkSize, true);
                        break;

                    case (int)Nefs16HeaderPart4TransformType.Aes:
                        transform = new NefsDataTransform(chunkSize, false, aes256key);
                        break;

                    case (int)Nefs16HeaderPart4TransformType.None:
                        transform = new NefsDataTransform(chunkSize, false);
                        break;

                    default:
                        Log.LogError("Found v1.6 data chunk with unknown transform; aborting.");
                        return new List<NefsDataChunk>();
                }

                // Create data chunk info
                var chunk = new NefsDataChunk(size, cumulativeSize, transform, entry.Checksum);
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
                return this.indexLookup[item.Guid];
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
