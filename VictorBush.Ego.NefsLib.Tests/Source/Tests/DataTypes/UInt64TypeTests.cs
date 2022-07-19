// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.DataTypes;

public sealed class UInt64TypeTests
{
	[Fact]
	public async Task Read_NegativeOffset_DataRead()
	{
		var fs = TestHelpers.CreateDataTypesTestFileSystem();
		using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
		{
			var data = new UInt64Type(-8);
			await data.ReadAsync(file, 8, new NefsProgress());
			Assert.Equal((ulong)0x0102030405060708, data.Value);
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
			Assert.Equal((ulong)0x1112131415161718, data.Value);
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
			Assert.Equal((ulong)0x0102030405060708, data.Value);
			Assert.Equal("0x102030405060708", data.ToString());
		}
	}

	[Fact]
	public void Size_8bytes()
	{
		var data = new UInt64Type(0);
		Assert.Equal(8, data.Size);
	}
}
