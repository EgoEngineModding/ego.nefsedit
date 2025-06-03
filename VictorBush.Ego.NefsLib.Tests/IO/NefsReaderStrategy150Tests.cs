// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

public class NefsReaderStrategy150Tests
{
	private readonly NefsProgress p = new(CancellationToken.None);

	[Fact]
	public async Task Read150HeaderPart1Async_ExtraBytesAtEnd_ExtraBytesIgnored()
	{
		byte[] bytes =
		{
			// 5 bytes offset
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

			// Entry 1
			0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
			0x41, 0x42,
			0x43, 0x44,
			0x11, 0x12, 0x13, 0x14,
			0x21, 0x22, 0x23, 0x24,
			0x31, 0x32, 0x33, 0x34,

			// Extra bytes
			0xFF, 0xFF,
		};

		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);
		var size = NefsTocEntry150.ByteCount + 2;
		var offset = 5;

		// Test
		var part1 = await NefsReaderStrategy150.Read150HeaderPart1Async(br, offset, size, this.p);

		// Verify
		Assert.Single(part1.Entries);

		var e1 = part1.Entries[0];
		Assert.Equal((ulong)0x0807060504030201, e1.Start);
		Assert.Equal(0x4241u, e1.Volume);
		Assert.Equal(0x4443u, (ushort)e1.Flags);
		Assert.Equal((uint)0x14131211, e1.SharedInfo);
		Assert.Equal((uint)0x24232221, e1.FirstBlock);
		Assert.Equal((uint)0x34333231, e1.NextDuplicate);
	}

	[Fact]
	public async Task ReadHeaderPart5Async_ValidData_DataRead()
	{
		byte[] bytes =
		{
			// 5 bytes offset
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

			// Archive size
			0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,

			// Offset into part 3 for archive name
			0x21, 0x22, 0x23, 0x24,

			// First data offset
			0x25, 0x26, 0x27, 0x28,
		};

		var stream = new MemoryStream(bytes);
		var size = 16;
		var offset = 5;
		using var endianReader = new EndianBinaryReader(stream, true);

		// Test
		var part5 = await NefsReaderStrategy150.ReadHeaderPart5Async(endianReader, offset, size, this.p);

		// Verify
		Assert.Single(part5.Entries);
		Assert.Equal((ulong)0x1817161514131211, part5.Entries[0].Size);
		Assert.Equal((uint)0x24232221, part5.Entries[0].NameOffset);
		Assert.Equal((uint)0x28272625, part5.Entries[0].DataOffset);
	}
}
