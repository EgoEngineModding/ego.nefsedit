// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 1.
    /// </summary>
    public class NefsHeaderPart1
    {
        private readonly SortedDictionary<NefsItemId, NefsHeaderPart1Entry> entriesById;
        private readonly List<NefsHeaderPart1Entry> entriesByIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart1"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal NefsHeaderPart1(IList<NefsHeaderPart1Entry> entries)
        {
            this.entriesByIndex = new List<NefsHeaderPart1Entry>(entries);
            this.entriesById = new SortedDictionary<NefsItemId, NefsHeaderPart1Entry>(entries.ToDictionary(e => new NefsItemId(e.Id.Value), e => e));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart1"/> class.
        /// </summary>
        /// <param name="items">The list of items in the archive sorted by id.</param>
        /// <param name="part4">Header part 4.</param>
        internal NefsHeaderPart1(NefsItemList items, NefsHeaderPart4 part4)
        {
            this.entriesByIndex = new List<NefsHeaderPart1Entry>();
            this.entriesById = new SortedDictionary<NefsItemId, NefsHeaderPart1Entry>();
            var nextMetadataIndex = 0U;

            foreach (var item in items.EnumerateById())
            {
                var entry = new NefsHeaderPart1Entry();
                entry.Data0x00_OffsetToData.Value = item.DataSource.Offset;
                entry.Data0x08_MetadataIndex.Value = nextMetadataIndex++;
                entry.Data0x10_Id.Value = item.Id.Value;
                entry.Data0x0c_IndexIntoPart4.Value = part4.GetIndexForItem(item);

                this.entriesByIndex.Add(entry);
                this.entriesById.Add(item.Id, entry);
            }
        }

        /// <summary>
        /// Gets entries for each item in the archive, sorted by id. The key is the item id; the
        /// value is the metadata entry for that item.
        /// </summary>
        public IReadOnlyDictionary<NefsItemId, NefsHeaderPart1Entry> EntriesById => this.entriesById;

        /// <summary>
        /// Gets the list of entries in the order they appear in the header. For part 1,
        /// the items should be sorted by id.
        /// </summary>
        public IList<NefsHeaderPart1Entry> EntriesByIndex => this.entriesByIndex;
    }
}
