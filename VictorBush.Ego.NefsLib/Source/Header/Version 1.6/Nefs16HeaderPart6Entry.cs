// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;

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
        /// <param name="guid">The Guid of the item this metadata belongs to.</param>
        public Nefs16HeaderPart6Entry(Guid guid)
        {
            this.Guid = guid;
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
        /// The unique identifier of the item this data is for.
        /// </summary>
        public Guid Guid { get; }

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
