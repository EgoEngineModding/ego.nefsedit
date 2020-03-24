// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 1.
    /// </summary>
    public class NefsHeaderPart1
    {
        private readonly SortedDictionary<NefsItemId, NefsHeaderPart1Entry> entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart1"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal NefsHeaderPart1(IDictionary<NefsItemId, NefsHeaderPart1Entry> entries)
        {
            this.entries = new SortedDictionary<NefsItemId, NefsHeaderPart1Entry>(entries);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart1"/> class.
        /// </summary>
        /// <param name="items">The list of items in the archive sorted by id.</param>
        /// <param name="part4">Header part 4.</param>
        internal NefsHeaderPart1(NefsItemList items, NefsHeaderPart4 part4)
        {
            this.entries = new SortedDictionary<NefsItemId, NefsHeaderPart1Entry>();

            foreach (var item in items)
            {
                var entry = new NefsHeaderPart1Entry();

                entry.Id.Value = item.Id.Value;
                entry.OffsetToData.Value = item.DataSource.Offset;
                entry.IndexIntoPart4.Value = part4.GetIndexForItem(item);

                // Get index into part 2. When NefsLib writes the header, it will always write part
                // 1 and part 2 ordered by item id.
                entry.IndexIntoPart2.Value = item.Id.Value;

                this.entries.Add(item.Id, entry);
            }
        }

        /// <summary>
        /// Gets the list of entries sorted by id.
        /// </summary>
        public IEnumerable<NefsHeaderPart1Entry> Entries => this.entries.Values;

        /// <summary>
        /// The part 1 entries for each item in the archive. The key is the item id; the value is
        /// the part 1 entry for that item.
        /// </summary>
        /// <remarks>
        /// Part 1 entries are not guaranteed to be written in order. Part 1 entries are not
        /// guaranteed to be written in the same order as part 2 entries, so the entries are stored
        /// in dictionaries after they are read from disk for easy access based on item id.
        /// </remarks>
        public IReadOnlyDictionary<NefsItemId, NefsHeaderPart1Entry> EntriesById => this.entries;
    }
}
