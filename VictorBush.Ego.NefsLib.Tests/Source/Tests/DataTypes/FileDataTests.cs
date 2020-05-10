// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.DataTypes
{
    using System.Linq;
    using VictorBush.Ego.NefsLib.DataTypes;
    using Xunit;

    public class FileDataTests
    {
        [Fact]
        public void GetDataListVersion160_DataReturned()
        {
            var test = new TestClass();
            var data = FileData.GetDataList(test, NefsVersion.Version160).ToList();
            Assert.Equal(4, data.Count);
            Assert.Equal(1, test.OtherVariable);

            // Properties come first, then fields
            Assert.Same(test.Data_0x4, data[0]);
            Assert.Same(test.Data_0x8, data[1]);
            Assert.Same(test.Data_0xC, data[2]);
            Assert.Same(test.Data_0x0, data[3]);
        }

        [Fact]
        public void GetDataListVersion200_DataReturned()
        {
            var test = new TestClass();
            var data = FileData.GetDataList(test, NefsVersion.Version200).ToList();
            Assert.Equal(4, data.Count);
            Assert.Equal(1, test.OtherVariable);

            // Properties come first, then fields
            Assert.Same(test.Data_0x4, data[0]);
            Assert.Same(test.Data_0xA, data[1]);
            Assert.Same(test.Data_0xC, data[2]);
            Assert.Same(test.Data_0x0, data[3]);
        }

        private class TestClass
        {
            [FileData]
            private UInt32Type data0x0 = new UInt32Type(0x0);

            private int otherVariable = 1;

            public DataType Data_0x0 => this.data0x0;

            public DataType Data_0x4 => this.Data0x4;

            public DataType Data_0x8 => this.Data0x8;

            public DataType Data_0xA => this.Data0xA;

            public DataType Data_0xC => this.Data0xC;

            [FileData]
            public UInt32Type Data0x4 { get; } = new UInt32Type(0x4);

            public int OtherVariable => this.otherVariable;

            [FileData(NefsVersion.Version160)]
            private UInt16Type Data0x8 { get; } = new UInt16Type(0x8);

            [FileData(NefsVersion.Version200)]
            private UInt16Type Data0xA { get; } = new UInt16Type(0xA);

            [FileData(NefsVersion.Version160, NefsVersion.Version200)]
            private UInt16Type Data0xC { get; } = new UInt16Type(0xC);
        }
    }
}
