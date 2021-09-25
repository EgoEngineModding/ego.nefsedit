// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Item;

    public class NefsInjectHeader
    {
        public const int Size = 0x20;

        public NefsInjectHeader()
        {
        }

        public NefsInjectHeader(long primaryOffset, int primarySize, long secondaryOffset, int secondarySize)
        {
            this.Data0x00_PrimaryOffset.Value = (ulong)primaryOffset;
            this.Data0x08_PrimarySize.Value = (uint)primarySize;
            this.Data0x0C_SecondaryOffset.Value = (ulong)secondaryOffset;
            this.Data0x14_SecondarySize.Value = (uint)secondarySize;
        }

        public long PrimaryOffset => (long) this.Data0x00_PrimaryOffset.Value;

        public int PrimarySize => (int)this.Data0x08_PrimarySize.Value;
        public long SecondaryOffset => (long)this.Data0x0C_SecondaryOffset.Value;
        public int SecondarySize => (int)this.Data0x14_SecondarySize.Value;

        [FileData]
        private UInt64Type Data0x00_PrimaryOffset { get; } = new UInt64Type(0x00);

        [FileData]
        private UInt32Type Data0x08_PrimarySize { get; } = new UInt32Type(0x08);

        [FileData]
        private UInt64Type Data0x0C_SecondaryOffset { get; } = new UInt64Type(0x0C);

        [FileData]
        private UInt32Type Data0x14_SecondarySize { get; } = new UInt32Type(0x14);

    }
}
