// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// Header introduction.
    /// </summary>
    public class NefsHeaderIntro
    {
        /// <summary>
        /// Expected first four bytes of a NeFS archive.
        /// </summary>
        public const UInt32 NefsMagicNumber = 0x5346654E;

        /// <summary>
        /// The size of the header intro.
        /// </summary>
        public const uint Size = 0x100;

        /// <summary>File magic number; "NeFS" or 0x5346654E.</summary>
        [FileData]
        public UInt32Type MagicNumber { get; } = new UInt32Type(0x0000);

        /// <summary>Expected hash of header.</summary>
        [FileData]
        public ByteArrayType ExpectedHash { get; } = new ByteArrayType(0x0004, 0x20);

        /// <summary>256-bit AES key stored as a hex string.</summary>
        [FileData]
        public ByteArrayType AesKey { get; } = new ByteArrayType(0x0024, 0x40);

        /// <summary>Size of header in bytes.</summary>
        [FileData]
        public UInt32Type HeaderSize { get; } = new UInt32Type(0x0064);

        /// <summary>Appears to be a constant number expected to be 0x20000.</summary>
        [FileData]
        public UInt32Type Unknown0x68 { get; } = new UInt32Type(0x0068);

        /// <summary>The number of items in the archive.</summary>
        [FileData]
        public UInt32Type NumberOfItems { get; } = new UInt32Type(0x006c);

        /// <summary>8 bytes; Another constant; the last four bytes are "zlib" in ASCII.</summary>
        [FileData]
        public UInt64Type Unknown0x70zlib { get; } = new UInt64Type(0x0070);

        /// <summary>Unknown value.</summary>
        [FileData]
        public UInt64Type Unknown0x78 { get; } = new UInt64Type(0x0078);

        /// <summary>Unknown, maybe constant (01 00 00 01).</summary>
        [FileData]
        public UInt32Type Unknown0x80 { get; } = new UInt32Type(0x0080);

        /// <summary>Offset the header part 1.</summary>
        [FileData]
        public UInt32Type OffsetToPart1 { get; } = new UInt32Type(0x0084);

        /// <summary>Offset to header part 6.</summary>
        [FileData]
        public UInt32Type OffsetToPart6 { get; } = new UInt32Type(0x0088);

        /// <summary>Offset to header part 2.</summary>
        [FileData]
        public UInt32Type OffsetToPart2 { get; } = new UInt32Type(0x008c);

        /// <summary>Offset to header part 7.</summary>
        [FileData]
        public UInt32Type OffsetToPart7 { get; } = new UInt32Type(0x0090);

        /// <summary>Offset to header part 3 (the filename/directory strings list).</summary>
        [FileData]
        public UInt32Type OffsetToPart3 { get; } = new UInt32Type(0x0094);

        /// <summary>Offset to header part 4.</summary>
        [FileData]
        public UInt32Type OffsetToPart4 { get; } = new UInt32Type(0x0098);

        /// <summary>Offset to header part 5.</summary>
        [FileData]
        public UInt32Type OffsetToPart5 { get; } = new UInt32Type(0x009c);

        /// <summary>Offset to header part 8.</summary>
        [FileData]
        public UInt32Type OffsetToPart8 { get; } = new UInt32Type(0x00a0);

        /// <summary>Unknown chunk of data.</summary>
        [FileData]
        public ByteArrayType Unknown0xa4 { get; } = new ByteArrayType(0x00a4, 0x5c);

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
