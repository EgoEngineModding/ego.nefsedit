// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.DataTypes;

public sealed class ByteArrayTypeTests
{
	[Fact]
	public void ByteArrayType_ValidSize_SizeSet()
	{
		var data = new ByteArrayType(0, 6);
		Assert.Equal(6, data.Size);
	}

	[Fact]
	public void ByteArrayType_ZeroSize_ArgumentOutOfRangeExceptionThrown()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new ByteArrayType(0, 0));
	}

	[Fact]
	public void GetBytes_DataNotRead_EmptyByteArrayReturned()
	{
		var data = new ByteArrayType(0, 2);
		var expected = new byte[] { 0x00, 0x00 };
		Assert.True(expected.SequenceEqual(data.GetBytes()));
	}

	[Fact]
	public async Task GetBytes_DataRead_DataReturned()
	{
		var fs = TestHelpers.CreateDataTypesTestFileSystem();
		using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
		{
			var data = new ByteArrayType(0, 6);
			await data.ReadAsync(file, 0, new NefsProgress());

			var expected = new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03 };
			Assert.True(expected.SequenceEqual(data.GetBytes()));
		}
	}

	[Fact]
	public async Task GetUInt32_NoOffset_ValueReturned()
	{
		var fs = TestHelpers.CreateDataTypesTestFileSystem();
		using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
		{
			var data = new ByteArrayType(0, 6);
			await data.ReadAsync(file, 0, new NefsProgress());
			Assert.Equal((uint)0x05060708, data.GetUInt32(0));
		}
	}

	[Fact]
	public async Task GetUInt32_OffsetOutOfBounds_ArgumentOutOfRangeExceptionThrown()
	{
		var fs = TestHelpers.CreateDataTypesTestFileSystem();
		using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
		{
			var data = new ByteArrayType(0, 6);
			await data.ReadAsync(file, 0, new NefsProgress());

			// Offset is outside the bounds of the array
			Assert.Throws<ArgumentOutOfRangeException>(() => data.GetUInt32(8));
		}
	}

	[Fact]
	public async Task GetUInt32_OffsetTwoBytesFromEnd_ArgumentOutOfRangeExceptionThrown()
	{
		var fs = TestHelpers.CreateDataTypesTestFileSystem();
		using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
		{
			var data = new ByteArrayType(0, 6);
			await data.ReadAsync(file, 0, new NefsProgress());

			// Offset is 2 bytes away from end of array
			Assert.Throws<ArgumentOutOfRangeException>(() => data.GetUInt32(4));
		}
	}

	[Fact]
	public async Task GetUInt32_ValidOffset_ValueReturned()
	{
		var fs = TestHelpers.CreateDataTypesTestFileSystem();
		using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
		{
			var data = new ByteArrayType(0, 6);
			await data.ReadAsync(file, 0, new NefsProgress());
			Assert.Equal((uint)0x03040506, data.GetUInt32(2));
		}
	}

	[Fact]
	public async Task Read_VariousTests()
	{
		var fs = TestHelpers.CreateDataTypesTestFileSystem();

		/*
         *  Data size: 0x3
         *  Data offset: 0x2
         *  Base offset: 0x10
         */
		using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
		{
			var data = new ByteArrayType(0x2, 0x3);
			await data.ReadAsync(file, 0x10, new NefsProgress());
			var expected = new byte[] { 0x26, 0x25, 0x24 };
			Assert.True(expected.SequenceEqual(data.Value));
		}

		/*
         * Data size: 5
         * Data offset: -4
         * Base offset: 0x10
         */
		using (var file = fs.File.OpenRead(TestHelpers.DataTypesTestFilePath))
		{
			var data = new ByteArrayType(-4, 5);
			await data.ReadAsync(file, 0x10, new NefsProgress());
			var expected = new byte[] { 0x14, 0x13, 0x12, 0x11, 0x28 };
			Assert.True(expected.SequenceEqual(data.Value));
		}
	}

	[Fact]
	public void Value_NewValueLengthGreaterThanOriginalSize_ExceptionThrown()
	{
		var data = new ByteArrayType(0, 2);
		Assert.Throws<ArgumentException>(() => data.Value = new byte[5]);
	}

	[Fact]
	public void Value_NewValueLengthSameAsOriginalSize_ExceptionThrown()
	{
		var data = new ByteArrayType(0, 2)
		{
			Value = new byte[] { 0x1, 0x2 }
		};

		Assert.Equal(2, data.Size);
		Assert.Equal(0x1, data.Value[0]);
		Assert.Equal(0x2, data.Value[1]);
	}
}
