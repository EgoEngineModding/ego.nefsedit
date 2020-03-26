// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// An entry in header part 6 for an item in an archive.
    /// </summary>
    public class NefsHeaderPart6Entry
    {
        /// <summary>
        /// The size of a part 6 entry.
        /// </summary>
        public const uint Size = 0x4;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart6Entry"/> class.
        /// </summary>
        /// <param name="id">The item id this entry is for.</param>
        public NefsHeaderPart6Entry(NefsItemId id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Unknown data.
        /// </summary>
        [FileData]
        public ByteArrayType Byte0 { get; } = new ByteArrayType(0x00, 0x01);

        /// <summary>
        /// Unknown data.
        /// </summary>
        [FileData]
        public ByteArrayType Byte1 { get; } = new ByteArrayType(0x01, 0x01);

        /// <summary>
        /// Unknown data.
        /// </summary>
        [FileData]
        public ByteArrayType Byte2 { get; } = new ByteArrayType(0x02, 0x01);

        /// <summary>
        /// Unknown data.
        /// </summary>
        [FileData]
        public ByteArrayType Byte3 { get; } = new ByteArrayType(0x03, 0x01);

        /// <summary>
        /// Gets the item id this is for. This value is not written in the header but is stored here
        /// for reference.
        /// </summary>
        public NefsItemId Id { get; }
    }
}
