// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// An entry in header part 6 for an item in an archive.
    /// </summary>
    public class Nefs20HeaderPart6Entry
    {
        /// <summary>
        /// The size of a part 6 entry.
        /// </summary>
        public const uint Size = 0x4;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs20HeaderPart6Entry"/> class.
        /// </summary>
        /// <param name="guid">The Guid of the item this metadata belongs to.</param>
        public Nefs20HeaderPart6Entry(Guid guid)
        {
            this.Guid = guid;
        }

        /// <summary>
        /// A bitfield that has various flags.
        /// </summary>
        public Nefs20HeaderPart6Flags Flags => (Nefs20HeaderPart6Flags)this.Data0x02_Flags.Value;

        /// <summary>
        /// The unique identifier of the item this data is for.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte Unknown0x3 => this.Data0x03_Unknown.Value;

        /// <summary>
        /// Unknown.
        /// </summary>
        public UInt16 Volume => this.Data0x00_Volume.Value;

        /// <summary>
        /// Data at offset 0x00.
        /// </summary>
        [FileData]
        internal UInt16Type Data0x00_Volume { get; } = new UInt16Type(0x00);

        /// <summary>
        /// Data at offset 0x01.
        /// </summary>
        [FileData]
        internal UInt8Type Data0x02_Flags { get; } = new UInt8Type(0x02);

        /// <summary>
        /// Data at offset 0x02.
        /// </summary>
        [FileData]
        internal UInt8Type Data0x03_Unknown { get; } = new UInt8Type(0x03);

        /// <summary>
        /// Creates a <see cref="NefsItemAttributes"/> object.
        /// </summary>
        /// <returns>The attributes.</returns>
        public NefsItemAttributes CreateAttributes()
        {
            return new NefsItemAttributes(
                v20Unknown0x01: this.Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x01),
                v20Unknown0x02: this.Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x02),
                isDirectory: this.Flags.HasFlag(Nefs20HeaderPart6Flags.IsDirectory),
                v20Unknown0x08: this.Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x08),
                v20Unknown0x10: this.Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x10),
                v20Unknown0x20: this.Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x20),
                v20Unknown0x40: this.Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x40),
                v20Unknown0x80: this.Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x80),
                part6Volume: this.Volume,
                part6Unknown0x3: this.Unknown0x3);
        }
    }
}
