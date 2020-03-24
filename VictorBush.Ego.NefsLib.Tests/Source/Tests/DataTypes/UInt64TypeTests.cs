// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.DataTypes
{
    using System;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    public class UInt64TypeTests
    {
        [Fact]
        public async Task Read_NegativeOffset_DataRead()
        {
            var fs = TestHelpers.CreateDataTypesTestFileSystem();
            using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
            {
                var data = new UInt64Type(-8);
                await data.ReadAsync(file, 8, new NefsProgress());
                Assert.Equal((UInt64)0x0102030405060708, data.Value);
                Assert.Equal("0x102030405060708", data.ToString());
            }
        }

        [Fact]
        public async Task Read_PositveOffset_DataRead()
        {
            var fs = TestHelpers.CreateDataTypesTestFileSystem();
            using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
            {
                var data = new UInt64Type(8);
                await data.ReadAsync(file, 0, new NefsProgress());
                Assert.Equal((UInt64)0x1112131415161718, data.Value);
                Assert.Equal("0x1112131415161718", data.ToString());
            }
        }

        [Fact]
        public async Task Read_ZeroOffset_DataRead()
        {
            var fs = TestHelpers.CreateDataTypesTestFileSystem();
            using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
            {
                var data = new UInt64Type(0x0);
                await data.ReadAsync(file, 0, new NefsProgress());
                Assert.Equal((UInt64)0x0102030405060708, data.Value);
                Assert.Equal("0x102030405060708", data.ToString());
            }
        }

        [Fact]
        public void Size_8bytes()
        {
            var data = new UInt64Type(0);
            Assert.Equal((uint)8, data.Size);
        }
    }
}
