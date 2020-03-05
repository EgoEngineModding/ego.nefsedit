// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.DataTypes
{
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
            Assert.Equal(2, data.Count);
            Assert.Equal(1, test.OtherVariable);

            // Properties come first, then fields
            Assert.Same(test.Data_0x4, data[0]);
            Assert.Same(test.Data_0x0, data[1]);
        }

        private class TestClass
        {
            private int otherVariable = 1;

            [FileData]
            private UInt32Type data0x0 = new UInt32Type(0x0);

            [FileData]
            public UInt32Type Data0x4 { get; } = new UInt32Type(0x4);

            public DataType Data_0x0 => this.data0x0;

            public DataType Data_0x4 => this.Data0x4;

            public int OtherVariable => this.otherVariable;
        }
    }
}
