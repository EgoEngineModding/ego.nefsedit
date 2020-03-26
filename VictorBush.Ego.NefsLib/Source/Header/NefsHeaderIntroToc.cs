// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// Header intro table of contents. Contains offsets to other header parts.
    /// </summary>
    public class NefsHeaderIntroToc
    {
        /// <summary>
        /// Offset to the table of contents in the header.
        /// </summary>
        public const uint Offset = NefsHeaderIntro.Size;

        /// <summary>
        /// The size of this section.
        /// </summary>
        public const uint Size = 0x80;

        /// <summary>Unknown, maybe constant (01 00 00 01).</summary>
        [FileData]
        public UInt32Type Unknown0x00 { get; } = new UInt32Type(0x0000);

        /// <summary>Offset the header part 1.</summary>
        [FileData]
        public UInt32Type OffsetToPart1 { get; } = new UInt32Type(0x0004);

        /// <summary>Offset to header part 6.</summary>
        [FileData]
        public UInt32Type OffsetToPart6 { get; } = new UInt32Type(0x0008);

        /// <summary>Offset to header part 2.</summary>
        [FileData]
        public UInt32Type OffsetToPart2 { get; } = new UInt32Type(0x000c);

        /// <summary>Offset to header part 7.</summary>
        [FileData]
        public UInt32Type OffsetToPart7 { get; } = new UInt32Type(0x0010);

        /// <summary>Offset to header part 3 (the filename/directory strings list).</summary>
        [FileData]
        public UInt32Type OffsetToPart3 { get; } = new UInt32Type(0x0014);

        /// <summary>Offset to header part 4.</summary>
        [FileData]
        public UInt32Type OffsetToPart4 { get; } = new UInt32Type(0x0018);

        /// <summary>Offset to header part 5.</summary>
        [FileData]
        public UInt32Type OffsetToPart5 { get; } = new UInt32Type(0x001c);

        /// <summary>Offset to header part 8.</summary>
        [FileData]
        public UInt32Type OffsetToPart8 { get; } = new UInt32Type(0x0020);

        /// <summary>Unknown chunk of data.</summary>
        [FileData]
        public ByteArrayType Unknown0x24 { get; } = new ByteArrayType(0x0024, 0x5c);

        /// <summary>
        /// The size of header part 1.
        /// </summary>
        public uint Part1Size => this.OffsetToPart2.Value - this.OffsetToPart1.Value;

        /// <summary>
        /// The size of header part 2.
        /// </summary>
        public uint Part2Size => this.OffsetToPart3.Value - this.OffsetToPart2.Value;

        /// <summary>
        /// The size of header part 3.
        /// </summary>
        public uint Part3Size => this.OffsetToPart4.Value - this.OffsetToPart3.Value;

        /// <summary>
        /// The size of header part 4.
        /// </summary>
        public uint Part4Size => this.OffsetToPart5.Value - this.OffsetToPart4.Value;

        /// <summary>
        /// The size of header part 5.
        /// </summary>
        public uint Part5Size => this.OffsetToPart6.Value - this.OffsetToPart5.Value;

        /// <summary>
        /// The size of header part 6.
        /// </summary>
        public uint Part6Size => this.OffsetToPart7.Value - this.OffsetToPart6.Value;

        /// <summary>
        /// The size of header part 7.
        /// </summary>
        public uint Part7Size => this.OffsetToPart8.Value - this.OffsetToPart7.Value;
    }
}
