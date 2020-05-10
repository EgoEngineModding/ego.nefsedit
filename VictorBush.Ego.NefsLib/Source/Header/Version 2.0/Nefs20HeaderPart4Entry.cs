// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// An entry in header part 4 for an item in an archive.
    /// </summary>
    public class Nefs20HeaderPart4Entry
    {
        /// <summary>
        /// The size of a part 4 entry. This is used to get the offset into part 4 from an index
        /// into part 4.
        /// </summary>
        public const uint Size = 0x4;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs20HeaderPart4Entry"/> class.
        /// </summary>
        internal Nefs20HeaderPart4Entry()
        {
        }

        /// <summary>
        /// Cumulative size of this data chunk. To get the size of this chunk, subtract the previous
        /// chunk's cumulative size.
        /// </summary>
        public UInt32 CumulativeChunkSize => this.Data0x00_CumulativeChunkSize.Value;

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x00_CumulativeChunkSize { get; } = new UInt32Type(0x00);
    }
}
