// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 1.
    /// </summary>
    public class Nefs16HeaderPart1
    {
        private readonly SortedDictionary<NefsItemId, Nefs16HeaderPart1Entry> entriesById;
        private readonly List<Nefs16HeaderPart1Entry> entriesByIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart1"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal Nefs16HeaderPart1(IList<Nefs16HeaderPart1Entry> entries)
        {
            this.entriesByIndex = new List<Nefs16HeaderPart1Entry>(entries);
            this.entriesById = new SortedDictionary<NefsItemId, Nefs16HeaderPart1Entry>(entries.ToDictionary(e => new NefsItemId(e.Id.Value), e => e));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart1"/> class.
        /// </summary>
        /// <param name="items">The list of items in the archive sorted by id.</param>
        /// <param name="part4">Header part 4.</param>
        internal Nefs16HeaderPart1(NefsItemList items, Nefs16HeaderPart4 part4)
        {
            this.entriesById = new SortedDictionary<NefsItemId, Nefs16HeaderPart1Entry>();
            var nextMetadataIndex = 0U;

            // Enumerate this list depth first. This determines the metadata index.
            // The part 1 entries will be sorted by item id.
            foreach (var item in items.EnumerateDepthFirstByName())
            {
                var entry = new Nefs16HeaderPart1Entry();
                entry.Data0x00_OffsetToData.Value = item.DataSource.Offset;
                entry.Data0x08_MetadataIndex.Value = nextMetadataIndex++;
                entry.Data0x10_Id.Value = item.Id.Value;
                entry.Data0x0c_IndexIntoPart4.Value = part4.GetIndexForItem(item);

                this.entriesById.Add(item.Id, entry);
            }

            // Part 1 is sorted by item id
            this.entriesByIndex = new List<Nefs16HeaderPart1Entry>(this.entriesById.Values);
        }

        /// <summary>
        /// Gets entries for each item in the archive, sorted by id. The key is the item id; the
        /// value is the metadata entry for that item.
        /// </summary>
        public IReadOnlyDictionary<NefsItemId, Nefs16HeaderPart1Entry> EntriesById => this.entriesById;

        /// <summary>
        /// Gets the list of entries in the order they appear in the header. For part 1,
        /// the items should be sorted by id.
        /// </summary>
        public IList<Nefs16HeaderPart1Entry> EntriesByIndex => this.entriesByIndex;
    }
}
