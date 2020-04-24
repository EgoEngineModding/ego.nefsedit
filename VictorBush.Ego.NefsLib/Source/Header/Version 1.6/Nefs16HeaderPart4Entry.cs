// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// An entry in header part 4 for an item in an archive.
    /// </summary>
    public class Nefs16HeaderPart4Entry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart4Entry"/> class.
        /// </summary>
        /// <param name="id">The id of the item this entry is for.</param>
        internal Nefs16HeaderPart4Entry(NefsItemId id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Chunk metadata.
        /// </summary>
        public List<Nefs16HeaderPart4Chunk> Chunks { get; } = new List<Nefs16HeaderPart4Chunk>();

        /// <summary>
        /// The id of the item this entry belongs to.
        /// </summary>
        public NefsItemId Id { get; }
    }
}
