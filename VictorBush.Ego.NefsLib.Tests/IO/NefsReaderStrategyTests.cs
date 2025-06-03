// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

public class NefsReaderStrategyTests
{
	private readonly NefsProgress p = new(CancellationToken.None);

	[Fact]
	public async Task ReadHeaderPart3Async_DoubleTerminators_StringsRead()
	{
		byte[] bytes =
		{
			// Offset
			0xFF, 0xFF,

			// Entries
			0x41, 0x42, 0x00,
			0x43, 0x00, 0x00, // Double terminators
			0x45, 0x46, 0x00,
		};

		var stream = new MemoryStream(bytes);
		var size = 9;
		var offset = 2;

		// Test
		var part3 = await NefsReaderStrategy.ReadHeaderPart3Async(stream, offset, size, this.p);

		// Verify
		Assert.Equal(4, part3.FileNamesByOffset.Count);
		Assert.Equal(4, part3.OffsetsByFileName.Count);
		Assert.Equal("AB", part3.FileNamesByOffset[0]);
		Assert.Equal("C", part3.FileNamesByOffset[3]);
		Assert.Equal("", part3.FileNamesByOffset[5]);
		Assert.Equal("EF", part3.FileNamesByOffset[6]);
	}

	[Fact]
	public async Task ReadHeaderPart3Async_NoEndingTerminator_StringsRead()
	{
		byte[] bytes =
		{
			// Offset
			0xFF, 0xFF,

			// Entries
			0x41, 0x42, 0x00,
			0x43, 0x44, 0x00,
			0x45, 0x46, 0x47, // No ending terminator
		};

		var stream = new MemoryStream(bytes);
		var size = 9;
		var offset = 2;

		// Test
		var part3 = await NefsReaderStrategy.ReadHeaderPart3Async(stream, offset, size, this.p);

		// Verify
		Assert.Equal(2, part3.FileNamesByOffset.Count);
		Assert.Equal(2, part3.OffsetsByFileName.Count);
		Assert.Equal("AB", part3.FileNamesByOffset[0]);
		Assert.Equal("CD", part3.FileNamesByOffset[3]);
	}

	[Fact]
	public async Task ReadHeaderPart3Async_ValidData_StringsRead()
	{
		byte[] bytes =
		{
			// Offset
			0xFF, 0xFF,

			// Entries
			0x41, 0x42, 0x00,
			0x43, 0x44, 0x00,
			0x45, 0x46, 0x00,
		};

		var stream = new MemoryStream(bytes);
		var size = 9;
		var offset = 2;

		// Test
		var part3 = await NefsReaderStrategy.ReadHeaderPart3Async(stream, offset, size, this.p);

		// Verify
		Assert.Equal(3, part3.FileNamesByOffset.Count);
		Assert.Equal(3, part3.OffsetsByFileName.Count);
		Assert.Equal("AB", part3.FileNamesByOffset[0]);
		Assert.Equal("CD", part3.FileNamesByOffset[3]);
		Assert.Equal("EF", part3.FileNamesByOffset[6]);
	}
}
