// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// An entry in header part 4 for an item in an archive.
    /// </summary>
    public class NefsHeaderPart4Entry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart4Entry"/> class.
        /// </summary>
        /// <param name="id">The id of the item this entry is for.</param>
        internal NefsHeaderPart4Entry(NefsItemId id)
        {
            this.Id = id;
        }

        /// <summary>
        /// <para>List of cumulative chunk sizes.</para>
        /// <list type="bullet">
        /// <item>First entry is size of first chunk.</item>
        /// <item>Second entry is size of first + second chunk.</item>
        /// <item>Last entry is size of all chunks together.</item>
        /// </list>
        /// <para>
        /// To get the size of a specific chunk, simply subtract the previous chunk size entry. If
        /// the item is not compressed, the list should only have one entry equal to the extracted size.
        /// </para>
        /// </summary>
        public List<UInt32> ChunkSizes { get; } = new List<UInt32>();

        /// <summary>
        /// The id of the item this entry belongs to.
        /// </summary>
        public NefsItemId Id { get; }
    }
}
