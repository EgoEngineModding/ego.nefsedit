// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// An entry in header part 2 for an item in an archive.
    /// </summary>
    public class Nefs16HeaderPart2Entry
    {
        /// <summary>
        /// The size of a part 2 entry. This is used to get the offset into part 2 from an index
        /// into part 2.
        /// </summary>
        public const uint Size = 0x14;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart2Entry"/> class.
        /// </summary>
        internal Nefs16HeaderPart2Entry()
        {
        }

        /// <summary>
        /// The id of the directory this item belongs to.
        /// </summary>
        public NefsItemId DirectoryId => new NefsItemId(this.Data0x00_DirectoryId.Value);

        /// <summary>
        /// Extracted sisze of this item.
        /// </summary>
        public UInt32 ExtractedSize => this.Data0x0c_ExtractedSize.Value;

        /// <summary>
        /// Id of the first child of this item.
        /// - If the first child id matches this item's id, then there are no children.
        /// - If this item is a file, there won't be any children (only directories can have children).
        /// </summary>
        public NefsItemId FirstChildId => new NefsItemId(this.Data0x04_FirstChildId.Value);

        /// <summary>
        /// The unique identifier of this item in the archive.
        /// </summary>
        public NefsItemId Id => new NefsItemId(this.Data0x10_Id.Value);

        /// <summary>
        /// Offset into header part 3 (file strings table) for the name of this item.
        /// </summary>
        public UInt32 OffsetIntoPart3 => this.Data0x08_OffsetIntoPart3.Value;

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x00_DirectoryId { get; } = new UInt32Type(0x00);

        /// <summary>
        /// Data at offset 0x04.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x04_FirstChildId { get; } = new UInt32Type(0x04);

        /// <summary>
        /// Data at offset 0x08.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x08_OffsetIntoPart3 { get; } = new UInt32Type(0x08);

        /// <summary>
        /// Data at offset 0x0c.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x0c_ExtractedSize { get; } = new UInt32Type(0x0c);

        /// <summary>
        /// Data at offset 0x10.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x10_Id { get; } = new UInt32Type(0x10);
    }
}
