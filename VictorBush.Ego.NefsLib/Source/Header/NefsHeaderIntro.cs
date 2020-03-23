// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// Header introduction. Contains size, encryption, and verification info.
    /// </summary>
    public class NefsHeaderIntro
    {
        /// <summary>
        /// Expected first four bytes of a NeFS archive.
        /// </summary>
        public const UInt32 NefsMagicNumber = 0x5346654E;

        /// <summary>
        /// The size of this section.
        /// </summary>
        public const uint Size = 0x80;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderIntro"/> class.
        /// </summary>
        /// <param name="isEncrpyted">A value indicating whether the header is encrypted.</param>
        public NefsHeaderIntro(bool isEncrpyted = false)
        {
            this.MagicNumber.Value = NefsMagicNumber;
            this.IsEncrypted = isEncrpyted;
        }

        /// <summary>
        /// Gets a value indicating whether the header is encrypted.
        /// </summary>
        public bool IsEncrypted { get; }

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
    }
}
