// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 7.
    /// </summary>
    public class NefsHeaderPart7
    {
        private readonly List<NefsHeaderPart7Entry> entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart7"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal NefsHeaderPart7(IList<NefsHeaderPart7Entry> entries)
        {
            this.entries = new List<NefsHeaderPart7Entry>(entries);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart7"/> class from a list of items.
        /// </summary>
        /// <param name="items">The list of items in the archive sorted by id.</param>
        internal NefsHeaderPart7(NefsItemList items)
        {
            this.entries = new List<NefsHeaderPart7Entry>();

            foreach (var item in items)
            {
                var entry = new NefsHeaderPart7Entry();
                entry.Unknown0x00.Value = item.Part7Unknown0x00;
                entry.Unknown0x04.Value = item.Part7Unknown0x04;

                this.entries.Add(entry);
            }
        }

        /// <summary>
        /// The part 7 entries for each item in the archive.
        /// </summary>
        public IReadOnlyList<NefsHeaderPart7Entry> Entries => this.entries;

        /// <summary>
        /// The current size of header part 7.
        /// </summary>
        public UInt32 Size => (UInt32)this.Entries.Count * NefsHeaderPart7Entry.Size;
    }
}
