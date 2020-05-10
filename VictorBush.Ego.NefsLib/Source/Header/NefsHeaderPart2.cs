// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 2.
    /// </summary>
    public class NefsHeaderPart2
    {
        private readonly List<NefsHeaderPart2Entry> entriesByIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart2"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal NefsHeaderPart2(IList<NefsHeaderPart2Entry> entries)
        {
            this.entriesByIndex = new List<NefsHeaderPart2Entry>(entries);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart2"/> class.
        /// </summary>
        /// <param name="items">The list of items in the archive.</param>
        /// <param name="part3">Header part 3.</param>
        internal NefsHeaderPart2(NefsItemList items, NefsHeaderPart3 part3)
        {
            this.entriesByIndex = new List<NefsHeaderPart2Entry>();

            foreach (var item in items.EnumerateDepthFirstByName())
            {
                var entry = new NefsHeaderPart2Entry();
                entry.Data0x00_DirectoryId.Value = item.DirectoryId.Value;
                entry.Data0x04_FirstChildId.Value = items.GetItemFirstChildId(item.Id).Value;
                entry.Data0x08_OffsetIntoPart3.Value = part3.OffsetsByFileName[item.FileName];
                entry.Data0x0c_ExtractedSize.Value = item.DataSource.Size.ExtractedSize;
                entry.Data0x10_Id.Value = item.Id.Value;

                this.entriesByIndex.Add(entry);
            }
        }

        /// <summary>
        /// Gets the list of entries in the order they appear in the header.
        /// </summary>
        public IList<NefsHeaderPart2Entry> EntriesByIndex => this.entriesByIndex;
    }
}
