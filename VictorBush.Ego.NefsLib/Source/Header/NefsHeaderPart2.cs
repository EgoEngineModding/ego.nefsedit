// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 2.
    /// </summary>
    public class NefsHeaderPart2
    {
        private readonly Dictionary<NefsItemId, NefsHeaderPart2Entry> entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart2"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal NefsHeaderPart2(IDictionary<NefsItemId, NefsHeaderPart2Entry> entries)
        {
            this.entries = new Dictionary<NefsItemId, NefsHeaderPart2Entry>(entries);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart2"/> class.
        /// </summary>
        /// <param name="items">The list of items in the archive sorted by id.</param>
        /// <param name="part3">Header part 3.</param>
        internal NefsHeaderPart2(NefsItemList items, NefsHeaderPart3 part3)
        {
            this.entries = new Dictionary<NefsItemId, NefsHeaderPart2Entry>();

            foreach (var item in items)
            {
                var entry = new NefsHeaderPart2Entry();
                entry.DirectoryId.Value = item.DirectoryId.Value;
                entry.FirstChildId.Value = item.Id.Value;
                entry.ExtractedSize.Value = item.DataSource.Size.ExtractedSize;
                entry.OffsetIntoPart3.Value = part3.OffsetsByFileName[item.FileName];
                entry.Id.Value = item.Id.Value;

                // Find first child item
                var firstChild = items.Where(i => i.DirectoryId == item.Id && i != item).OrderBy(i => i.Id.Value).FirstOrDefault();
                if (firstChild != null)
                {
                    entry.FirstChildId.Value = firstChild.Id.Value;
                }

                this.entries.Add(item.Id, entry);
            }
        }

        /// <summary>
        /// The part 2 entries for each item in the archive. The key is the item id; the value is
        /// the part 2 entry for that item.
        /// </summary>
        /// <remarks>
        /// Part 2 entries are not guaranteed to be written in the same order as part 1 entries, so
        /// the entries are stored in dictionaries after they are read from disk for easy access
        /// based on item id.
        /// </remarks>
        public IReadOnlyDictionary<NefsItemId, NefsHeaderPart2Entry> EntriesById => this.entries;
    }
}
