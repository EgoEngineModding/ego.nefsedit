// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// An entry in header part 1 for an item in an archive.
    /// </summary>
    public class NefsHeaderPart1Entry : INefsHeaderPartEntry
    {


        public int Size => NefsHeaderPart1.EntrySize;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart1Entry"/> class.
        /// </summary>
        /// <param name="guid">The Guid of the item this metadata belongs to.</param>
        internal NefsHeaderPart1Entry(Guid guid)
        {
            this.Guid = guid;
        }

        /// <summary>
        /// The unique identifier of the item this data is for.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// The id of the item. It is possible to have duplicate item's with the same id.
        /// </summary>
        public NefsItemId Id => new NefsItemId(this.Data0x10_Id.Value);

        /// <summary>
        /// The index used for parts 2 and 7 for this item.
        /// </summary>
        public UInt32 IndexPart2 => this.Data0x08_IndexPart2.Value;

        /// <summary>
        /// The index into header part 4 for this item. For the actual offset, see <see cref="OffsetIntoPart4"/>.
        /// </summary>
        public UInt32 IndexPart4 => this.Data0x0c_IndexPart4.Value;

        /// <summary>
        /// The offset into header part 2.
        /// </summary>
        public UInt64 OffsetIntoPart2 => this.IndexPart2 * NefsHeaderPart2.EntrySize;

        /// <summary>
        /// The offset into header part 4.
        /// </summary>
        public UInt64 OffsetIntoPart4 => this.IndexPart4 * Nefs20HeaderPart4.EntrySize;

        /// <summary>
        /// The offset into header part 6.
        /// </summary>
        public UInt64 OffsetIntoPart6 => this.IndexPart2 * Nefs20HeaderPart6.EntrySize;

        /// <summary>
        /// The offset into header part 7.
        /// </summary>
        public UInt64 OffsetIntoPart7 => this.IndexPart2 * NefsHeaderPart7.EntrySize;

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
        internal UInt32Type Data0x08_IndexPart2 { get; } = new UInt32Type(0x08);

        /// <summary>
        /// Data at offset 0x0c.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x0c_IndexPart4 { get; } = new UInt32Type(0x0c);

        /// <summary>
        /// Data at offset 0x10.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x10_Id { get; } = new UInt32Type(0x10);
    }
}
