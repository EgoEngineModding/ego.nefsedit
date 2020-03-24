// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 6.
    /// </summary>
    public class NefsHeaderPart6
    {
        private readonly List<NefsHeaderPart6Entry> entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart6"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal NefsHeaderPart6(IList<NefsHeaderPart6Entry> entries)
        {
            this.entries = new List<NefsHeaderPart6Entry>(entries);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart6"/> class from a list of items.
        /// </summary>
        /// <param name="items">The list of items in the archive sorted by id.</param>
        internal NefsHeaderPart6(NefsItemList items)
        {
            this.entries = new List<NefsHeaderPart6Entry>();

            foreach (var item in items)
            {
                var entry = new NefsHeaderPart6Entry();
                entry.Byte0.Value[0] = item.Part6Unknown0x00;
                entry.Byte1.Value[0] = item.Part6Unknown0x01;
                entry.Byte2.Value[0] = item.Part6Unknown0x02;
                entry.Byte3.Value[0] = item.Part6Unknown0x03;

                this.entries.Add(entry);
            }
        }

        /// <summary>
        /// The part 6 entries for each item in the archive.
        /// </summary>
        public IReadOnlyList<NefsHeaderPart6Entry> Entries => this.entries;
    }
}
