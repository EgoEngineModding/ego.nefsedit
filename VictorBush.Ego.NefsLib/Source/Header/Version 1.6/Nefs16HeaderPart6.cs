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
    public class Nefs16HeaderPart6
    {
        private readonly Dictionary<Guid, Nefs16HeaderPart6Entry> entriesByGuid;
        private readonly List<Nefs16HeaderPart6Entry> entriesByIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart6"/> class.
        /// </summary>
        /// <param name="entries">A list of entries to instantiate this part with.</param>
        internal Nefs16HeaderPart6(IList<Nefs16HeaderPart6Entry> entries)
        {
            this.entriesByIndex = new List<Nefs16HeaderPart6Entry>(entries);
            this.entriesByGuid = new Dictionary<Guid, Nefs16HeaderPart6Entry>(entries.ToDictionary(e => e.Guid));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart6"/> class from a list of items.
        /// </summary>
        /// <param name="items">The list of items in the archive.</param>
        internal Nefs16HeaderPart6(NefsItemList items)
        {
            this.entriesByIndex = new List<Nefs16HeaderPart6Entry>();
            this.entriesByGuid = new Dictionary<Guid, Nefs16HeaderPart6Entry>();

            // Sort part 6 by item id. Part 1 and part 6 order must match.
            foreach (var item in items.EnumerateById())
            {
                var entry = new Nefs16HeaderPart6Entry(item.Guid);
                entry.Data0x00_Byte0.Value[0] = item.Part6Unknown0x00;
                entry.Data0x01_Byte1.Value[0] = item.Part6Unknown0x01;
                entry.Data0x02_Byte2.Value[0] = item.Part6Unknown0x02;
                entry.Data0x03_Byte3.Value[0] = item.Part6Unknown0x03;

                this.entriesByGuid.Add(item.Guid, entry);
                this.entriesByIndex.Add(entry);
            }
        }

        /// <summary>
        /// Gets entries for each item in the archive, accessible by Guid.
        /// </summary>
        public IReadOnlyDictionary<Guid, Nefs16HeaderPart6Entry> EntriesByGuid => this.entriesByGuid;

        /// <summary>
        /// Gets the list of entries in the order they appear in the header.
        /// </summary>
        public IList<Nefs16HeaderPart6Entry> EntriesByIndex => this.entriesByIndex;
    }
}
