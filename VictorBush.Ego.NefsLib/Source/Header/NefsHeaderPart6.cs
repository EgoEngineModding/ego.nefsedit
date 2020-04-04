// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 6.
    /// </summary>
    public class NefsHeaderPart6
    {
        private readonly SortedDictionary<NefsItemId, NefsHeaderPart6Entry> entriesById;

        private readonly List<NefsHeaderPart6Entry> entriesByIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart6"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal NefsHeaderPart6(IList<NefsHeaderPart6Entry> entries)
        {
            this.entriesByIndex = new List<NefsHeaderPart6Entry>(entries);
            this.entriesById = new SortedDictionary<NefsItemId, NefsHeaderPart6Entry>(entries.ToDictionary(e => e.Id, e => e));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart6"/> class from a list of items.
        /// </summary>
        /// <param name="items">The list of items in the archive sorted by id.</param>
        internal NefsHeaderPart6(NefsItemList items)
        {
            this.entriesByIndex = new List<NefsHeaderPart6Entry>();
            this.entriesById = new SortedDictionary<NefsItemId, NefsHeaderPart6Entry>();

            foreach (var item in items.EnumerateDepthFirstByName())
            {
                var entry = new NefsHeaderPart6Entry(item.Id);
                entry.Data0x00_Byte0.Value[0] = item.Part6Unknown0x00;
                entry.Data0x01_Byte1.Value[0] = item.Part6Unknown0x01;
                entry.Data0x02_Byte2.Value[0] = item.Part6Unknown0x02;
                entry.Data0x03_Byte3.Value[0] = item.Part6Unknown0x03;

                this.entriesByIndex.Add(entry);
                this.entriesById.Add(item.Id, entry);
            }
        }

        /// <summary>
        /// Gets entries for each item in the archive, sorted by id. The key is the item id; the
        /// value is the metadata entry for that item.
        /// </summary>
        public IReadOnlyDictionary<NefsItemId, NefsHeaderPart6Entry> EntriesById => this.entriesById;

        /// <summary>
        /// Gets the list of entries in the order they appear in the header.
        /// </summary>
        public IList<NefsHeaderPart6Entry> EntriesByIndex => this.entriesByIndex;
    }
}
