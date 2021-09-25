// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// An entry in header part 4 for an item in an archive.
    /// </summary>
    public class Nefs16HeaderPart4Entry : INefsHeaderPartEntry
    {
        /// <summary>
        /// The size of a part 4 entry. This is used to get the offset into part 4 from an index
        /// into part 4.
        /// </summary>
        public int Size => Nefs16HeaderPart4.EntrySize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart4Entry"/> class.
        /// </summary>
        internal Nefs16HeaderPart4Entry()
        {
        }

        /// <summary>
        /// Checksum of the chunk.
        /// </summary>
        public UInt16 Checksum => this.Data0x06_Checksum.Value;

        /// <summary>
        /// Cumulative block size of this chunk.
        /// </summary>
        public UInt32 CumulativeBlockSize => this.Data0x00_CumulativeBlockSize.Value;

        /// <summary>
        /// Transformation applied to this chunk.
        /// </summary>
        public Nefs16HeaderPart4TransformType TransformType => (Nefs16HeaderPart4TransformType)this.Data0x04_TransformType.Value;

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x00_CumulativeBlockSize { get; } = new UInt32Type(0x00);

        /// <summary>
        /// Data at offset 0x04.
        /// </summary>
        [FileData]
        internal UInt16Type Data0x04_TransformType { get; } = new UInt16Type(0x04);

        /// <summary>
        /// Data at offset 0x06.
        /// </summary>
        [FileData]
        internal UInt16Type Data0x06_Checksum { get; } = new UInt16Type(0x06);
    }
}
