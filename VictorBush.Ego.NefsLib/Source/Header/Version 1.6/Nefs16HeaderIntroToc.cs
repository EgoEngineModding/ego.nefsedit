// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// Header intro table of contents. Contains offsets to other header parts.
    /// </summary>
    public class Nefs16HeaderIntroToc
    {
        /// <summary>
        /// Offset to the table of contents in the header.
        /// </summary>
        public const uint Offset = Nefs16HeaderIntro.Size;

        /// <summary>
        /// The size of this section.
        /// </summary>
        public const uint Size = 0x80;

        /// <summary>
        /// Offset the header part 1.
        /// </summary>
        public UInt32 OffsetToPart1 => this.Data0x08_OffsetToPart1.Value;

        /// <summary>
        /// Offset to header part 2.
        /// </summary>
        public UInt32 OffsetToPart2 => this.Data0x10_OffsetToPart2.Value;

        /// <summary>
        /// Offset to header part 3 (the filename/directory strings list).
        /// </summary>
        public UInt32 OffsetToPart3 => this.Data0x18_OffsetToPart3.Value;

        /// <summary>
        /// Offset to header part 4.
        /// </summary>
        public UInt32 OffsetToPart4 => this.Data0x1c_OffsetToPart4.Value;

        /// <summary>
        /// Offset to header part 5.
        /// </summary>
        public UInt32 OffsetToPart5 => this.Data0x20_OffsetToPart5.Value;

        /// <summary>
        /// Offset to header part 6.
        /// </summary>
        public UInt32 OffsetToPart6 => this.Data0x0c_OffsetToPart6.Value;

        /// <summary>
        /// Offset to header part 7.
        /// </summary>
        public UInt32 OffsetToPart7 => this.Data0x14_OffsetToPart7.Value;

        /// <summary>
        /// Offset to header part 8.
        /// </summary>
        public UInt32 OffsetToPart8 => this.Data0x24_OffsetToPart8.Value;

        /// <summary>
        /// The size of header part 1.
        /// </summary>
        public uint Part1Size => this.OffsetToPart2 - this.OffsetToPart1;

        /// <summary>
        /// The size of header part 2.
        /// </summary>
        public uint Part2Size => this.OffsetToPart3 - this.OffsetToPart2;

        /// <summary>
        /// The size of header part 3.
        /// </summary>
        public uint Part3Size => this.OffsetToPart4 - this.OffsetToPart3;

        /// <summary>
        /// The size of header part 4.
        /// </summary>
        public uint Part4Size => this.OffsetToPart5 - this.OffsetToPart4;

        /// <summary>
        /// The size of header part 5.
        /// </summary>
        public uint Part5Size => this.OffsetToPart6 > 0
            ? this.OffsetToPart6 - this.OffsetToPart5
            : this.OffsetToPart8 - this.OffsetToPart5;

        /// <summary>
        /// The size of header part 6.
        /// </summary>
        public uint Part6Size => this.OffsetToPart6 > 0 ? this.OffsetToPart7 - this.OffsetToPart6 : 0;

        /// <summary>
        /// The size of header part 7.
        /// </summary>
        public uint Part7Size => this.OffsetToPart6 > 0 ? this.OffsetToPart8 - this.OffsetToPart7 : 0;

        /// <summary>
        /// Unknown.
        /// </summary>
        public UInt64 Unknown0x00 => this.Data0x00_Unknown.Value;

        /// <summary>
        /// Unknown chunk of data.
        /// </summary>
        public byte[] Unknown0x28 => this.Data0x28_Unknown.Value;

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal UInt64Type Data0x00_Unknown { get; } = new UInt64Type(0x0000);

        /// <summary>
        /// Data at offset 0x08.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x08_OffsetToPart1 { get; } = new UInt32Type(0x0008);

        /// <summary>
        /// Data at offset 0x0c.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x0c_OffsetToPart6 { get; } = new UInt32Type(0x000c);

        /// <summary>
        /// Data at offset 0x10.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x10_OffsetToPart2 { get; } = new UInt32Type(0x0010);

        /// <summary>
        /// Data at offset 0x14.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x14_OffsetToPart7 { get; } = new UInt32Type(0x0014);

        /// <summary>
        /// Data at offset 0x18.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x18_OffsetToPart3 { get; } = new UInt32Type(0x0018);

        /// <summary>
        /// Data at offset 0x1c.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x1c_OffsetToPart4 { get; } = new UInt32Type(0x001c);

        /// <summary>
        /// Data at offset 0x20.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x20_OffsetToPart5 { get; } = new UInt32Type(0x0020);

        /// <summary>
        /// Data at offset 0x24.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x24_OffsetToPart8 { get; } = new UInt32Type(0x0024);

        /// <summary>
        /// Data at 0x28.
        /// </summary>
        [FileData]
        internal ByteArrayType Data0x28_Unknown { get; } = new ByteArrayType(0x0028, 0x58);
    }
}
