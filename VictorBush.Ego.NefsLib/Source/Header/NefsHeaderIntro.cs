// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Text;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Utility;

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
            this.Data0x00_MagicNumber.Value = NefsMagicNumber;
            this.IsEncrypted = isEncrpyted;
        }

        /// <summary>
        /// 256-bit AES key stored as a hex string.
        /// </summary>
        public byte[] AesKeyHexString => this.Data0x24_AesKeyHexString.Value;

        /// <summary>
        /// Expected hash of header.
        /// </summary>
        public byte[] ExpectedHash => this.Data0x04_ExpectedHash.Value;

        /// <summary>
        /// Size of header in bytes.
        /// </summary>
        public UInt32 HeaderSize => this.Data0x64_HeaderSize.Value;

        /// <summary>
        /// Gets a value indicating whether the header is encrypted.
        /// </summary>
        public bool IsEncrypted { get; }

        /// <summary>
        /// File magic number; "NeFS" or 0x5346654E.
        /// </summary>
        public UInt32 MagicNumber => this.Data0x00_MagicNumber.Value;

        /// <summary>
        /// The NeFS format version.
        /// </summary>
        public UInt32 NefsVersion => this.Data0x68_NefsVersion.Value;

        /// <summary>
        /// The number of items in the archive.
        /// </summary>
        public UInt32 NumberOfItems => this.Data0x6c_NumberOfItems.Value;

        /// <summary>
        /// 8 bytes; Another constant; the last four bytes are "zlib" in ASCII.
        /// </summary>
        public UInt64 Unknown0x70zlib => this.Data0x70_UnknownZlib.Value;

        /// <summary>
        /// Unknown value.
        /// </summary>
        public UInt64 Unknown0x78 => this.Data0x78_Unknown.Value;

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x00_MagicNumber { get; } = new UInt32Type(0x0000);

        /// <summary>
        /// Data at offset 0x04.
        /// </summary>
        [FileData]
        internal ByteArrayType Data0x04_ExpectedHash { get; } = new ByteArrayType(0x0004, 0x20);

        /// <summary>
        /// Data at offset 0x24.
        /// </summary>
        [FileData]
        internal ByteArrayType Data0x24_AesKeyHexString { get; } = new ByteArrayType(0x0024, 0x40);

        /// <summary>
        /// Data at offset 0x64.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x64_HeaderSize { get; } = new UInt32Type(0x0064);

        /// <summary>
        /// Data at offset 0x68.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x68_NefsVersion { get; } = new UInt32Type(0x0068);

        /// <summary>
        /// Data at offset 0x6c.
        /// </summary>
        [FileData]
        internal UInt32Type Data0x6c_NumberOfItems { get; } = new UInt32Type(0x006c);

        /// <summary>
        /// Data at offset 0x70.
        /// </summary>
        [FileData]
        internal UInt64Type Data0x70_UnknownZlib { get; } = new UInt64Type(0x0070);

        /// <summary>
        /// Data at offset 0x78.
        /// </summary>
        [FileData]
        internal UInt64Type Data0x78_Unknown { get; } = new UInt64Type(0x0078);

        /// <summary>
        /// Gets the AES-256 key for this header.
        /// </summary>
        /// <returns>A byte array with the AES key.</returns>
        public byte[] GetAesKey()
        {
            var asciiKey = Encoding.ASCII.GetString(this.AesKeyHexString);
            return StringHelper.FromHexString(asciiKey);
        }
    }
}
