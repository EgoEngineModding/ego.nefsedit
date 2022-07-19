// See LICENSE.txt for license information.

using System.IO.Abstractions.TestingHelpers;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.DataTypes;

public sealed class DataTypeTests
{
	[Theory]
	[InlineData(0)]
	[InlineData(123456)]
	[InlineData(-123456)]
	public void Offset_OffsetInitialized(int offset)
	{
		var data = new TestDataType(offset);
		Assert.Equal(offset, data.Offset);
	}

	[Fact]
	public async Task Read_NegativeAbsoluteOffset_InvalidOperationExceptionThrown()
	{
		var fs = new MockFileSystem();
		fs.AddFile("test.dat", new MockFileData(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 }));

		// -4 + 0 == negative absolute offset
		var data = new TestDataType(-4);

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			using (var file = fs.File.OpenRead("test.dat"))
			{
				await data.ReadAsync(file, 0, new NefsProgress());
			}
		});
	}

	[Fact]
	public async Task Read_NegativeDataOffset_ReadSuccess()
	{
		var fs = new MockFileSystem();
		fs.AddFile("test.dat", new MockFileData(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 }));

		// -4 + 5 == positive absolute offset
		var data = new TestDataType(-4);

		using (var file = fs.File.OpenRead("test.dat"))
		{
			await data.ReadAsync(file, 5, new NefsProgress());
		}

		var expected = new byte[] { 0x2, 0x3, 0x4, 0x5 };
		Assert.Equal(expected, data.GetBytes());
	}

	[Fact]
	public async Task Read_NullStream_ArgumentNullExceptionThrown()
	{
		var data = new TestDataType(0);
		await Assert.ThrowsAsync<ArgumentNullException>(async () => await data.ReadAsync(null, 0, new NefsProgress()));
	}

	[Fact]
	public async Task Read_PastEndOfFile_InvalidOperationExceptionThrown()
	{
		var fs = new MockFileSystem();
		fs.AddFile("test.dat", new MockFileData(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 }));

		// 0 + 10 == past end of file
		var data = new TestDataType(0);

		await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			using (var file = fs.File.OpenRead("test.dat"))
			{
				await data.ReadAsync(file, 10, new NefsProgress());
			}
		});
	}

	[Fact]
	public async Task Read_PositiveDataOffset_ReadSuccess()
	{
		var fs = new MockFileSystem();
		fs.AddFile("test.dat", new MockFileData(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 }));

		// 1 + 0 == positive absolute offset
		var data = new TestDataType(1);

		using (var file = fs.File.OpenRead("test.dat"))
		{
			await data.ReadAsync(file, 0, new NefsProgress());
		}

		var expected = new byte[] { 0x2, 0x3, 0x4, 0x5 };
		Assert.Equal(expected, data.GetBytes());
	}

	[Fact]
	public async Task Write_NegativeAbsoluteOffset_InvalidOperationExceptionThrown()
	{
		var fs = new MockFileSystem();
		var file = fs.File.OpenWrite("test.dat");

		// -4 + 0 == negative absolute offset
		var data = new TestDataType(-4);
		await Assert.ThrowsAsync<InvalidOperationException>(() => data.WriteAsync(file, 0, new NefsProgress()));
	}

	[Fact]
	public async Task Write_NegativeDataOffset_WriteSuccess()
	{
		var expected = new byte[] { 0x5, 0x1, 0x2, 0x3, 0x4 };

		var fs = new MockFileSystem();
		fs.AddFile("test.dat", new MockFileData(new byte[] { 0x5, 0x5, 0x5, 0x5, 0x5 }));

		// -4 + 5 == positive absolute offset
		var data = new TestDataType(-4);
		data.Value = new byte[] { 0x1, 0x2, 0x3, 0x4 };

		// Test
		using (var file = fs.File.OpenWrite("test.dat"))
		{
			await data.WriteAsync(file, 5, new NefsProgress());
		}

		// Verify
		var actual = fs.File.ReadAllBytes("test.dat");
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task Write_NullStream_ArgumentNullExceptionThrown()
	{
		var data = new TestDataType(0);
		await Assert.ThrowsAsync<ArgumentNullException>(() => data.WriteAsync(null, 0, new NefsProgress()));
	}

	[Fact]
	public async Task Write_PositiveDataOffset_WriteSuccess()
	{
		var expected = new byte[] { 0x5, 0x1, 0x2, 0x3, 0x4 };

		var fs = new MockFileSystem();
		fs.AddFile("test.dat", new MockFileData(new byte[] { 0x5, 0x5, 0x5, 0x5, 0x5 }));

		// 1 + 0 == positive absolute offset
		var data = new TestDataType(1);
		data.Value = new byte[] { 0x1, 0x2, 0x3, 0x4 };

		// Test
		using (var file = fs.File.OpenWrite("test.dat"))
		{
			await data.WriteAsync(file, 0, new NefsProgress());
		}

		// Verify
		var actual = fs.File.ReadAllBytes("test.dat");
		Assert.Equal(expected, actual);
	}

	/// <summary>
	/// Mock implementation of DataType to use in tests.
	/// </summary>
	private class TestDataType : DataType
	{
		public TestDataType(int offset)
			: base(offset)
		{
		}

		public override int Size => 0x04;

		public byte[] Value { get; set; }

		public override byte[] GetBytes() => Value;

		public override async Task ReadAsync(Stream file, long baseOffset, NefsProgress p)
		{
			Value = await DoReadAsync(file, baseOffset, p);
		}
	}
}
