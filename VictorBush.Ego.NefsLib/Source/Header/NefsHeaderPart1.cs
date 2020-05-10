// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 1. The "master catalogue" of items in the archive.
    /// </summary>
    public class NefsHeaderPart1
    {
        private readonly Dictionary<Guid, NefsHeaderPart1Entry> entriesByGuid;
        private readonly List<NefsHeaderPart1Entry> entriesByIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart1"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal NefsHeaderPart1(IList<NefsHeaderPart1Entry> entries)
        {
            this.entriesByIndex = new List<NefsHeaderPart1Entry>(entries);
            this.entriesByGuid = new Dictionary<Guid, NefsHeaderPart1Entry>(entries.ToDictionary(e => e.Guid));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart1"/> class.
        /// </summary>
        /// <param name="items">The list of items in the archive.</param>
        /// <param name="part4">Header part 4.</param>
        internal NefsHeaderPart1(NefsItemList items, INefsHeaderPart4 part4)
        {
            this.entriesByGuid = new Dictionary<Guid, NefsHeaderPart1Entry>();
            var indexPart2 = 0U;

            // Enumerate this list depth first. This determines the part 2 order. The part 1 entries
            // will be sorted by item id.
            foreach (var item in items.EnumerateDepthFirstByName())
            {
                var entry = new NefsHeaderPart1Entry(item.Guid);
                entry.Data0x00_OffsetToData.Value = item.DataSource.Offset;
                entry.Data0x08_IndexPart2.Value = indexPart2++;
                entry.Data0x10_Id.Value = item.Id.Value;
                entry.Data0x0c_IndexPart4.Value = part4.GetIndexForItem(item);

                this.entriesByGuid.Add(item.Guid, entry);
            }

            // Sort part 1 by item id
            this.entriesByIndex = new List<NefsHeaderPart1Entry>(this.entriesByGuid.Values.OrderBy(e => e.Id));
        }

        /// <summary>
        /// Gets entries for each item in the archive, accessible by Guid.
        /// </summary>
        public IReadOnlyDictionary<Guid, NefsHeaderPart1Entry> EntriesByGuid => this.entriesByGuid;

        /// <summary>
        /// Gets the list of entries in the order they appear in the header. Usually items are
        /// sorted by id, but this is not guaranteed (for example, DiRT Rally has a header with
        /// items out of order).
        /// </summary>
        public IList<NefsHeaderPart1Entry> EntriesByIndex => this.entriesByIndex;
    }
}
