// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// Header intro table of contents. Contains offsets to other header parts.
    /// </summary>
    public class Nefs20HeaderIntroToc : INefsHeaderIntroToc
    {
        /// <summary>
        /// Size of data chunks a file is broken up into before each chunk is compressed and
        /// inserted into the archive.
        /// </summary>
        public const uint ChunkSize = 0x10000;

        /// <summary>
        /// Offset to the table of contents in the header.
        /// </summary>
        public const uint Offset = NefsHeaderIntro.Size;

        /// <summary>
        /// The size of this section.
        /// </summary>
        public const uint Size = 0x80;

        /// <summary>
        /// Hash block size.
        /// </summary>
        public UInt32 HashBlockSize => (UInt32)this.Data0x02_HashBlockSize.Value << 15;

        /// <summary>
        /// Number of volumes.
        /// </summary>
        public UInt16 NumVolumes => this.Data0x00_NumVolumes.Value;

        /// <inheritdoc/>
        public UInt32 OffsetToPart1 => this.Data0x04_OffsetToPart1.Value;

        /// <inheritdoc/>
        public UInt32 OffsetToPart2 => this.Data0x0c_OffsetToPart2.Value;

        /// <inheritdoc/>
        public UInt32 OffsetToPart3 => this.Data0x14_OffsetToPart3.Value;

        /// <inheritdoc/>
        public UInt32 OffsetToPart4 => this.Data0x18_OffsetToPart4.Value;

        /// <inheritdoc/>
        public UInt32 OffsetToPart5 => this.Data0x1c_OffsetToPart5.Value;

        /// <inheritdoc/>
        public UInt32 OffsetToPart6 => this.Data0x08_OffsetToPart6.Value;

        /// <inheritdoc/>
        public UInt32 OffsetToPart7 => this.Data0x10_OffsetToPart7.Value;

        /// <inheritdoc/>
        public UInt32 OffsetToPart8 => this.Data0x20_OffsetToPart8.Value;

        /// <inheritdoc/>
        public uint Part1Size => this.OffsetToPart2 - this.OffsetToPart1;

        /// <inheritdoc/>
        public uint Part2Size => this.OffsetToPart3 - this.OffsetToPart2;

        /// <inheritdoc/>
        public uint Part3Size => this.OffsetToPart4 - this.OffsetToPart3;

        /// <inheritdoc/>
        public uint Part4Size => this.OffsetToPart5 - this.OffsetToPart4;

        /// <summary>
        /// Unknown chunk of data.
        /// </summary>
        public byte[] Unknown0x24 => this.Data0x24_Unknown.Value;

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal UInt16Type Data0x00_NumVolumes { get; } = new UInt16Type(0x0000);

        /// <summary>
        /// Data at offset 0x02. This is the low 16-bits of a 32-bit value.
        /// </summary>
        [FileData]
        internal UInt16Type Data0x02_HashBlockSize { get; } = new UInt16Type(0x0002);

        /// <summary>
        /// Data at offset 0x04.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x04_OffsetToPart1 { get; } = new UInt32Type(0x0004);

        /// <summary>
        /// Data at offset 0x08.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x08_OffsetToPart6 { get; } = new UInt32Type(0x0008);

        /// <summary>
        /// Data at offset 0x0c.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x0c_OffsetToPart2 { get; } = new UInt32Type(0x000c);

        /// <summary>
        /// Data at offset 0x10.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x10_OffsetToPart7 { get; } = new UInt32Type(0x0010);

        /// <summary>
        /// Data at offset 0x14.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x14_OffsetToPart3 { get; } = new UInt32Type(0x0014);

        /// <summary>
        /// Data at offset 0x18.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x18_OffsetToPart4 { get; } = new UInt32Type(0x0018);

        /// <summary>
        /// Data at offset 0x1c.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x1c_OffsetToPart5 { get; } = new UInt32Type(0x001c);

        /// <summary>
        /// Data at offset 0x20.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x20_OffsetToPart8 { get; } = new UInt32Type(0x0020);

        /// <summary>
        /// Data at 0x24.
        /// </summary>
        [FileData]
        internal ByteArrayType Data0x24_Unknown { get; } = new ByteArrayType(0x0024, 0x5c);

        /// <inheritdoc/>
        public UInt32 ComputeNumChunks(uint extractedSize) =>
            (uint)Math.Ceiling(extractedSize / (double)ChunkSize);
    }
}
