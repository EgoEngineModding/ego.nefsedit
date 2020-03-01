// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// An entry in header part 1 for an item in an archive.
    /// </summary>
    public class NefsHeaderPart1Entry
    {
        /// <summary>
        /// The size of a part 1 entry.
        /// </summary>
        public const uint Size = 0x14;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart1Entry"/> class.
        /// </summary>
        internal NefsHeaderPart1Entry()
        {
        }

        /// <summary>
        /// The absolute offset to the file's data in the archive. For directories, this is 0.
        /// </summary>
        [FileData]
        public UInt64Type OffsetToData { get; } = new UInt64Type(0x00);

        /// <summary>
        /// The index into header part 2 for this item. For the actual offset, see <see cref="OffsetIntoPart2"/>.
        /// </summary>
        [FileData]
        public UInt32Type IndexIntoPart2 { get; } = new UInt32Type(0x08);

        /// <summary>
        /// The index into header part 4 for this item. For the actual offset, see <see cref="OffsetIntoPart4"/>.
        /// </summary>
        [FileData]
        public UInt32Type IndexIntoPart4 { get; } = new UInt32Type(0x0c);

        /// <summary>
        /// The unique identifier of this item in the archive.
        /// </summary>
        [FileData]
        public UInt32Type Id { get; } = new UInt32Type(0x10);

        /// <summary>
        /// The offset into header part 2.
        /// </summary>
        public UInt64 OffsetIntoPart2 => this.IndexIntoPart2.Value * NefsHeaderPart2Entry.Size;

        /// <summary>
        /// The offset into header part 4.
        /// </summary>
        public UInt64 OffsetIntoPart4 => this.IndexIntoPart4.Value * NefsHeaderPart4.DataSize;
    }
}