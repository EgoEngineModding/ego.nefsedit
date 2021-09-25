// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.DataTypes
{
    using System;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    public class UInt32TypeTests
    {
        [Fact]
        public async Task Read_NegativeOffset_DataRead()
        {
            var fs = TestHelpers.CreateDataTypesTestFileSystem();
            using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
            {
                var data = new UInt32Type(-8);
                await data.ReadAsync(file, 8, new NefsProgress());
                Assert.Equal((UInt32)0x05060708, data.Value);
                Assert.Equal("0x5060708", data.ToString());
            }
        }

        [Fact]
        public async Task Read_PositiveOffset_DataRead()
        {
            var fs = TestHelpers.CreateDataTypesTestFileSystem();
            using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
            {
                var data = new UInt32Type(8);
                await data.ReadAsync(file, 0, new NefsProgress());
                Assert.Equal((UInt32)0x15161718, data.Value);
                Assert.Equal("0x15161718", data.ToString());
            }
        }

        [Fact]
        public async Task Read_ZeroOffset_DataRead()
        {
            var fs = TestHelpers.CreateDataTypesTestFileSystem();
            using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
            {
                var data = new UInt32Type(0x0);
                await data.ReadAsync(file, 0, new NefsProgress());
                Assert.Equal((UInt32)0x05060708, data.Value);
                Assert.Equal("0x5060708", data.ToString());
            }
        }

        [Fact]
        public void Size_4bytes()
        {
            var data = new UInt32Type(0);
            Assert.Equal(4, data.Size);
        }
    }
}
