// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Item;

    public class NefsInjectHeader
    {
        public const int Size = 0x20;

        public const int ExpectedMagicNumber = 4484;

        public NefsInjectHeader()
        {
        }

        public NefsInjectHeader(long primaryOffset, int primarySize, long secondaryOffset, int secondarySize)
        {
            this.Data0x00_MagicNum.Value = ExpectedMagicNumber;
            this.Data0x04_Version.Value = 1;
            this.Data0x08_PrimaryOffset.Value = (ulong)primaryOffset;
            this.Data0x10_PrimarySize.Value = (uint)primarySize;
            this.Data0x14_SecondaryOffset.Value = (ulong)secondaryOffset;
            this.Data0x1C_SecondarySize.Value = (uint)secondarySize;
        }

        public int MagicNumber => (int)this.Data0x00_MagicNum.Value;
        public int Version => (int)this.Data0x04_Version.Value;

        public long PrimaryOffset => (long) this.Data0x08_PrimaryOffset.Value;

        public int PrimarySize => (int)this.Data0x10_PrimarySize.Value;
        public long SecondaryOffset => (long)this.Data0x14_SecondaryOffset.Value;
        public int SecondarySize => (int)this.Data0x1C_SecondarySize.Value;

        [FileData]
        private UInt32Type Data0x00_MagicNum { get; } = new UInt32Type(0x00);

        [FileData]
        private UInt32Type Data0x04_Version { get; } = new UInt32Type(0x04);

        [FileData]
        private UInt64Type Data0x08_PrimaryOffset { get; } = new UInt64Type(0x08);

        [FileData]
        private UInt32Type Data0x10_PrimarySize { get; } = new UInt32Type(0x10);

        [FileData]
        private UInt64Type Data0x14_SecondaryOffset { get; } = new UInt64Type(0x14);

        [FileData]
        private UInt32Type Data0x1C_SecondarySize { get; } = new UInt32Type(0x1C);

    }
}
