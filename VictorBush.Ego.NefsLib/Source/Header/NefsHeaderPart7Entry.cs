// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// An entry in header part 7 for an item in an archive.
    /// </summary>
    public class NefsHeaderPart7Entry
    {
        /// <summary>
        /// The size of a part 7 entry.
        /// </summary>
        public const uint Size = 0x8;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart7Entry"/> class.
        /// </summary>
        internal NefsHeaderPart7Entry()
        {
        }

        /// <summary>
        /// The sibling item id.
        /// </summary>
        [FileData]
        public UInt32Type Data0x00_SiblingId { get; } = new UInt32Type(0x00);

        /// <summary>
        /// The item id this entry is for.
        /// </summary>
        [FileData]
        public UInt32Type Data0x04_Id { get; } = new UInt32Type(0x04);

        /// <summary>
        /// Gets the id of the item this entry is for.
        /// </summary>
        public NefsItemId Id => new NefsItemId(this.Data0x04_Id.Value);

        /// <summary>
        /// Gets the id of the next item in the same directory as this item. If this item is the
        /// last item in the directory, the sibling id will equal the item id.
        /// </summary>
        public NefsItemId SiblingId => new NefsItemId(this.Data0x00_SiblingId.Value);
    }
}
