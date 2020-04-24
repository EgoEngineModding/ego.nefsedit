// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// An entry in header part 6 for an item in an archive.
    /// </summary>
    public class Nefs16HeaderPart6Entry
    {
        /// <summary>
        /// The size of a part 6 entry.
        /// </summary>
        public const uint Size = 0x4;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16HeaderPart6Entry"/> class.
        /// </summary>
        /// <param name="id">The item id this entry is for.</param>
        public Nefs16HeaderPart6Entry(NefsItemId id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Unknown data.
        /// </summary>
        public byte Byte0 => this.Data0x00_Byte0.Value[0];

        /// <summary>
        /// Unknown data.
        /// </summary>
        public byte Byte1 => this.Data0x01_Byte1.Value[0];

        /// <summary>
        /// Unknown data.
        /// </summary>
        public byte Byte2 => this.Data0x02_Byte2.Value[0];

        /// <summary>
        /// Unknown data.
        /// </summary>
        public byte Byte3 => this.Data0x03_Byte3.Value[0];

        /// <summary>
        /// Gets the item id this is for. This value is not written in the header but is stored here
        /// for reference.
        /// </summary>
        public NefsItemId Id { get; }

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal ByteArrayType Data0x00_Byte0 { get; } = new ByteArrayType(0x00, 0x01);

        /// <summary>
        /// Data at offset 0x01.
        /// </summary>
        [FileData]
        internal ByteArrayType Data0x01_Byte1 { get; } = new ByteArrayType(0x01, 0x01);

        /// <summary>
        /// Data at offset 0x02.
        /// </summary>
        [FileData]
        internal ByteArrayType Data0x02_Byte2 { get; } = new ByteArrayType(0x02, 0x01);

        /// <summary>
        /// Data at offset 0x03.
        /// </summary>
        [FileData]
        internal ByteArrayType Data0x03_Byte3 { get; } = new ByteArrayType(0x03, 0x01);
    }
}
