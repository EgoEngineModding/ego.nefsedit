// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Item;

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
        /// The unique identifier of this item in the archive.
        /// </summary>
        public NefsItemId Id => new NefsItemId(this.Data0x10_Id.Value);

        /// <summary>
        /// The index into header part 4 for this item. For the actual offset, see <see cref="OffsetIntoPart4"/>.
        /// </summary>
        public UInt32 IndexIntoPart4 => this.Data0x0c_IndexIntoPart4.Value;

        /// <summary>
        /// The index used for parts 2, 6, and 7 for this item.
        /// </summary>
        public UInt32 MetadataIndex => this.Data0x08_MetadataIndex.Value;

        /// <summary>
        /// The offset into header part 2.
        /// </summary>
        public UInt64 OffsetIntoPart2 => this.MetadataIndex * NefsHeaderPart2Entry.Size;

        /// <summary>
        /// The offset into header part 4.
        /// </summary>
        public UInt64 OffsetIntoPart4 => this.IndexIntoPart4 * Nefs20HeaderPart4.DataSize;

        /// <summary>
        /// The offset into header part 6.
        /// </summary>
        public UInt64 OffsetIntoPart6 => this.MetadataIndex * Nefs20HeaderPart6Entry.Size;

        /// <summary>
        /// The offset into header part 7.
        /// </summary>
        public UInt64 OffsetIntoPart7 => this.MetadataIndex * NefsHeaderPart7Entry.Size;

        /// <summary>
        /// The absolute offset to the file's data in the archive. For directories, this is 0.
        /// </summary>
        public UInt64 OffsetToData => this.Data0x00_OffsetToData.Value;

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal UInt64Type Data0x00_OffsetToData { get; } = new UInt64Type(0x00);

        /// <summary>
        /// Data at offset 0x08.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x08_MetadataIndex { get; } = new UInt32Type(0x08);

        /// <summary>
        /// Data at offset 0x0c.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x0c_IndexIntoPart4 { get; } = new UInt32Type(0x0c);

        /// <summary>
        /// Data at offset 0x10.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x10_Id { get; } = new UInt32Type(0x10);
    }
}
