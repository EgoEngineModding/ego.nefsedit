// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.DataTypes
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using VictorBush.Ego.NefsLib.DataTypes;
    using Xunit;

    public class FileDataTests
    {
        [Fact]
        public void GetDataList_DataReturned()
        {
            var test = new TestClass();
            var data = FileData.GetDataList(test).ToList();
            Assert.Equal(5, data.Count);
            Assert.Equal(1, test.OtherVariable);

            // Properties come first, then fields
            Assert.Same(test.Data_0x4, data[0]);
            Assert.Same(test.Data_0x8, data[1]);
            Assert.Same(test.Data_0xA, data[2]);
            Assert.Same(test.Data_0xC, data[3]);
            Assert.Same(test.Data_0x0, data[4]);
        }

        [Fact]
        public void GetDataList_Interface_DataReturned()
        {
            ITestInterface test = new TestClass();
            var data = FileData.GetDataList(test).ToList();
            Assert.Equal(5, data.Count);
            Assert.Equal(1, test.OtherVariable);

            // Properties come first, then fields
            Assert.Same(test.Data_0x4, data[0]);
            Assert.Same(test.Data_0x8, data[1]);
            Assert.Same(test.Data_0xA, data[2]);
            Assert.Same(test.Data_0xC, data[3]);
            Assert.Same(test.Data_0x0, data[4]);
        }

        private class TestClass : ITestInterface
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

            [FileData]
            private UInt16Type Data0x8 { get; } = new UInt16Type(0x8);

            [FileData]
            private UInt16Type Data0xA { get; } = new UInt16Type(0xA);

            [FileData]
            private UInt16Type Data0xC { get; } = new UInt16Type(0xC);
        }

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Test file.")]
        private interface ITestInterface
        {
            DataType Data_0x0 { get; }

            DataType Data_0x4 { get; }

            DataType Data_0x8 { get; }

            DataType Data_0xA { get; }

            DataType Data_0xC { get; }

            int OtherVariable { get; }
        }
    }
}
