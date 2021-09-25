// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 7.
    /// </summary>
    public class NefsHeaderPart7
    {
        private readonly List<NefsHeaderPart7Entry> entriesByIndex;

        /// <summary>
        /// The size of a part 7 entry.
        /// </summary>
        public const int EntrySize = 0x8;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart7"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal NefsHeaderPart7(IList<NefsHeaderPart7Entry> entries)
        {
            this.entriesByIndex = new List<NefsHeaderPart7Entry>(entries);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart7"/> class from a list of items.
        /// </summary>
        /// <param name="items">The list of items in the archive.</param>
        internal NefsHeaderPart7(NefsItemList items)
        {
            this.entriesByIndex = new List<NefsHeaderPart7Entry>();

            foreach (var item in items.EnumerateDepthFirstByName())
            {
                var entry = new NefsHeaderPart7Entry();
                entry.Data0x00_SiblingId.Value = items.GetItemSiblingId(item.Id).Value;
                entry.Data0x04_Id.Value = item.Id.Value;

                this.entriesByIndex.Add(entry);
            }
        }

        public int Size => this.entriesByIndex.Count * EntrySize;

        /// <summary>
        /// Gets the list of entries in the order they appear in the header.
        /// </summary>
        public IList<NefsHeaderPart7Entry> EntriesByIndex => this.entriesByIndex;
    }
}
