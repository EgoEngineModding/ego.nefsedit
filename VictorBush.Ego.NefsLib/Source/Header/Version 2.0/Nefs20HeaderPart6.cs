// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 6.
    /// </summary>
    public class Nefs20HeaderPart6
    {
        private readonly Dictionary<Guid, Nefs20HeaderPart6Entry> entriesByGuid;
        private readonly List<Nefs20HeaderPart6Entry> entriesByIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs20HeaderPart6"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal Nefs20HeaderPart6(IList<Nefs20HeaderPart6Entry> entries)
        {
            this.entriesByIndex = new List<Nefs20HeaderPart6Entry>(entries);
            this.entriesByGuid = new Dictionary<Guid, Nefs20HeaderPart6Entry>(entries.ToDictionary(e => e.Guid));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs20HeaderPart6"/> class from a list of items.
        /// </summary>
        /// <param name="items">The list of items in the archive.</param>
        internal Nefs20HeaderPart6(NefsItemList items)
        {
            this.entriesByIndex = new List<Nefs20HeaderPart6Entry>();
            this.entriesByGuid = new Dictionary<Guid, Nefs20HeaderPart6Entry>();

            // Sort part 6 by item id. Part 1 and part 6 order must match.
            foreach (var item in items.EnumerateDepthFirstByName())
            {
                var flags = Nefs20HeaderPart6Flags.None;
                flags |= item.Attributes.IsTransformed ? Nefs20HeaderPart6Flags.IsTransformed : 0;
                flags |= item.Attributes.V20Unknown0x02 ? Nefs20HeaderPart6Flags.Unknown0x02 : 0;
                flags |= item.Attributes.IsDirectory ? Nefs20HeaderPart6Flags.IsDirectory : 0;
                flags |= item.Attributes.IsDuplicated ? Nefs20HeaderPart6Flags.IsDuplicated : 0;
                flags |= item.Attributes.V20Unknown0x10 ? Nefs20HeaderPart6Flags.Unknown0x10 : 0;
                flags |= item.Attributes.V20Unknown0x20 ? Nefs20HeaderPart6Flags.Unknown0x20 : 0;
                flags |= item.Attributes.V20Unknown0x40 ? Nefs20HeaderPart6Flags.Unknown0x40 : 0;
                flags |= item.Attributes.V20Unknown0x80 ? Nefs20HeaderPart6Flags.Unknown0x80 : 0;

                var entry = new Nefs20HeaderPart6Entry(item.Guid);
                entry.Data0x00_Volume.Value = item.Attributes.Part6Volume;
                entry.Data0x02_Flags.Value = (byte)flags;
                entry.Data0x03_Unknown.Value = item.Attributes.Part6Unknown0x3;

                this.entriesByGuid.Add(item.Guid, entry);
                this.entriesByIndex.Add(entry);
            }
        }

        /// <summary>
        /// Gets entries for each item in the archive, accessible by Guid.
        /// </summary>
        public IReadOnlyDictionary<Guid, Nefs20HeaderPart6Entry> EntriesByGuid => this.entriesByGuid;

        /// <summary>
        /// Gets the list of entries in the order they appear in the header.
        /// </summary>
        public IList<Nefs20HeaderPart6Entry> EntriesByIndex => this.entriesByIndex;
    }
}
