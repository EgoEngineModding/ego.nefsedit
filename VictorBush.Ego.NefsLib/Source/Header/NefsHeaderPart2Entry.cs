// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// An entry in header part 2 for an item in an archive.
    /// </summary>
    public class NefsHeaderPart2Entry
    {
        /// <summary>
        /// The size of a part 2 entry. This is used to get the offset into part 2 from an index into part 2.
        /// </summary>
        public const uint Size = 0x14;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart2Entry"/> class.
        /// </summary>
        internal NefsHeaderPart2Entry()
        {
        }

        /// <summary>
        /// The id of the directory this item belongs to.
        /// </summary>
        [FileData]
        public UInt32Type DirectoryId { get; } = new UInt32Type(0x00);

        /// <summary>
        /// Id of the first child of this item.
        /// - If the first child id matches this item's id, then there are no children.
        /// - If this item is a file, there won't be any children (only directories can have children).
        /// </summary>
        [FileData]
        public UInt32Type FirstChildId { get; } = new UInt32Type(0x04);

        /// <summary>
        /// Offset into header part 3 (file strings table) for the name of this item.
        /// </summary>
        [FileData]
        public UInt32Type OffsetIntoPart3 { get; } = new UInt32Type(0x08);

        /// <summary>
        /// Extracted sisze of this item.
        /// </summary>
        [FileData]
        public UInt32Type ExtractedSize { get; } = new UInt32Type(0x0c);

        /// <summary>
        /// The unique identifier of this item in the archive.
        /// </summary>
        [FileData]
        public UInt32Type Id { get; } = new UInt32Type(0x10);
    }
}
