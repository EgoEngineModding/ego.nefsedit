// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// Header part 5.
    /// </summary>
    public class NefsHeaderPart5
    {
        /// <summary>
        /// The size of header part 5.
        /// </summary>
        public const uint Size = 0x10;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart5"/> class.
        /// </summary>
        internal NefsHeaderPart5()
        {
            this.Data0x0C_FirstDataOffset.Value = Nefs20Header.DataOffsetDefault;
        }

        /// <summary>
        /// Offset into header part 3 for the name of this archive.
        /// </summary>
        public UInt32 ArchiveNameStringOffset => this.Data0x08_ArchiveNameStringOffset.Value;

        /// <summary>
        /// The size of the archive file.
        /// </summary>
        public UInt64 ArchiveSize => this.Data0x00_ArchiveSize.Value;

        /// <summary>
        /// Offset to first item data.
        /// </summary>
        public UInt32 FirstDataOffset => this.Data0x0C_FirstDataOffset.Value;

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal UInt64Type Data0x00_ArchiveSize { get; } = new UInt64Type(0x00);

        /// <summary>
        /// Data at offset 0x08.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x08_ArchiveNameStringOffset { get; } = new UInt32Type(0x08);

        /// <summary>
        /// Data at offset 0x0C.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x0C_FirstDataOffset { get; } = new UInt32Type(0x0C);
    }
}
