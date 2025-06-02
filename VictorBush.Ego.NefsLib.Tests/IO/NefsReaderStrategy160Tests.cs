// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

public class NefsReaderStrategy160Tests
{
	private readonly NefsProgress p = new(CancellationToken.None);

	[Fact]
	public async Task ReadHeaderPart1Async_ExtraBytesAtEnd_ExtraBytesIgnored()
	{
		byte[] bytes =
		{
			// 5 bytes offset
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

			// Entry 1
			0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
			0x11, 0x12, 0x13, 0x14, 0x21, 0x22, 0x23, 0x24,
			0x31, 0x32, 0x33, 0x34,

			// Extra bytes
			0xFF, 0xFF,
		};

		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);
		var size = NefsTocEntry160.ByteCount + 2;
		var offset = 5;

		// Test
		var part1 = await NefsReaderStrategy160.ReadHeaderPart1Async(br, offset, size, this.p);

		// Verify
		Assert.Single(part1.Entries);

		var e1 = part1.Entries[0];
		Assert.Equal((ulong)0x0807060504030201, e1.Start);
		Assert.Equal((uint)0x14131211, e1.SharedInfo);
		Assert.Equal((uint)0x24232221, e1.FirstBlock);
		Assert.Equal((uint)0x34333231, e1.NextDuplicate);
	}

	[Fact]
	public async Task ReadHeaderPart1Async_OffsetOutOfBounds_NoEntries()
	{
		var bytes = new byte[10];
		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);

		// Test
		var part1 = await NefsReaderStrategy160.ReadHeaderPart1Async(br, 10, 5, this.p);

		// Verify
		Assert.Empty(part1.Entries);
	}

	[Fact]
	public async Task ReadHeaderPart1Async_SizeOutOfBounds_NoEntries()
	{
		var bytes = new byte[10];
		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);

		// Test
		var part1 = await NefsReaderStrategy160.ReadHeaderPart1Async(br, 0, 20, this.p);

		// Verify
		Assert.Empty(part1.Entries);
	}

	[Fact]
	public async Task ReadHeaderPart1Async_ValidData_DataRead()
	{
		byte[] bytes =
		{
			// 5 bytes offset
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

			// Entry 1
			0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
			0x11, 0x12, 0x13, 0x14, 0x21, 0x22, 0x23, 0x24,
			0x31, 0x32, 0x33, 0x34,

			// Entry 2
			0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
			0x51, 0x52, 0x53, 0x54, 0x61, 0x62, 0x63, 0x64,
			0x71, 0x72, 0x73, 0x74,
		};

		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);
		var size = 2 * NefsTocEntry160.ByteCount;
		var offset = 5;

		// Test
		var part1 = await NefsReaderStrategy160.ReadHeaderPart1Async(br, offset, size, this.p);

		// Verify
		Assert.Equal(2, part1.Entries.Count);

		var e1 = part1.Entries[0];
		Assert.Equal((ulong)0x0807060504030201, e1.Start);
		Assert.Equal((uint)0x14131211, e1.SharedInfo);
		Assert.Equal((uint)0x24232221, e1.FirstBlock);
		Assert.Equal((uint)0x34333231, e1.NextDuplicate);

		var e2 = part1.Entries[1];
		Assert.Equal((ulong)0x4847464544434241, e2.Start);
		Assert.Equal((uint)0x54535251, e2.SharedInfo);
		Assert.Equal((uint)0x64636261, e2.FirstBlock);
		Assert.Equal((uint)0x74737271, e2.NextDuplicate);
	}

	[Fact]
	public async Task ReadHeaderPart1Async_ZeroSize_NoEntries()
	{
		var bytes = new byte[2];
		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);

		// Test
		var part1 = await NefsReaderStrategy160.ReadHeaderPart1Async(br, 0, 0, this.p);

		// Verify
		Assert.Empty(part1.Entries);
	}

	[Fact]
	public async Task ReadHeaderPart2Async_ExtraBytesAtEnd_ExtraBytesIgnored()
	{
		byte[] bytes =
		{
			// 5 bytes offset
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

			// Entry 1
			0x01, 0x02, 0x03, 0x04, 0x11, 0x12, 0x13, 0x14,
			0x21, 0x22, 0x23, 0x24, 0x31, 0x32, 0x33, 0x34,
			0x41, 0x42, 0x43, 0x44,

			// Extra bytes
			0xFF, 0xFF,
		};

		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);
		var size = NefsTocSharedEntryInfo160.ByteCount + 2;
		var offset = 5;

		// Test
		var part2 = await NefsReaderStrategy160.ReadHeaderPart2Async(br, offset, size, this.p);

		// Verify
		Assert.Single(part2.Entries);

		var e1 = part2.Entries[0];
		Assert.Equal((uint)0x04030201, e1.Parent);
		Assert.Equal((uint)0x14131211, e1.FirstChild);
		Assert.Equal((uint)0x24232221, e1.NameOffset);
		Assert.Equal((uint)0x34333231, e1.Size);
		Assert.Equal((uint)0x44434241, e1.FirstDuplicate);
	}

	[Fact]
	public async Task ReadHeaderPart2Async_ValidData_DataRead()
	{
		byte[] bytes =
		{
			// 5 bytes offset
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

			// Entry 1
			0x01, 0x02, 0x03, 0x04, 0x11, 0x12, 0x13, 0x14,
			0x21, 0x22, 0x23, 0x24, 0x31, 0x32, 0x33, 0x34,
			0x41, 0x42, 0x43, 0x44,

			// Entry 2
			0x51, 0x52, 0x53, 0x54, 0x61, 0x62, 0x63, 0x64,
			0x71, 0x72, 0x73, 0x74, 0x81, 0x82, 0x83, 0x84,
			0x91, 0x92, 0x93, 0x94,
		};

		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);
		var size = 2 * NefsTocSharedEntryInfo160.ByteCount;
		var offset = 5;

		// Test
		var part2 = await NefsReaderStrategy160.ReadHeaderPart2Async(br, offset, size, this.p);

		// Verify
		Assert.Equal(2, part2.Entries.Count);

		var e1 = part2.Entries[0];
		Assert.Equal((uint)0x04030201, e1.Parent);
		Assert.Equal((uint)0x14131211, e1.FirstChild);
		Assert.Equal((uint)0x24232221, e1.NameOffset);
		Assert.Equal((uint)0x34333231, e1.Size);
		Assert.Equal((uint)0x44434241, e1.FirstDuplicate);

		var e2 = part2.Entries[1];
		Assert.Equal((uint)0x54535251, e2.Parent);
		Assert.Equal((uint)0x64636261, e2.FirstChild);
		Assert.Equal((uint)0x74737271, e2.NameOffset);
		Assert.Equal((uint)0x84838281, e2.Size);
		Assert.Equal((uint)0x94939291, e2.FirstDuplicate);
	}

	[Fact]
	public async Task ReadHeaderPart6Async_ValidData_DataRead()
	{
		NefsHeaderEntryTable160 entryTable;
		NefsHeaderWriteableEntryTable160 part6;

		var part1Offset = 5;
		var part1Size = 2 * NefsTocEntry160.ByteCount;

		var part6Offset = 5;

		// NOTE: Part 6 is ordered in the same way part 1 is
		byte[] part1Bytes =
		{
			// 5 bytes offset
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

			// Entry 1
			0x11, 0x12, 0x13, 0x14, // Offset to data
			0x15, 0x14, 0x17, 0x18, // Offset to data (cont)
			0x19, 0x1A, 0x1B, 0x1C, // Index part 2
			0x1D, 0x1E, 0x1F, 0x20, // Index part 4
			0x21, 0x22, 0x23, 0x24, // Item id

			// Entry 2
			0x01, 0x02, 0x03, 0x04, // Offset to data
			0x05, 0x04, 0x07, 0x08, // Offset to data (cont)
			0x09, 0x0A, 0x0B, 0x0C, // Index part 2
			0x0D, 0x0E, 0x0F, 0x10, // Index part 4
			0x11, 0x12, 0x13, 0x14, // Item id
		};

		using (var part1Stream = new MemoryStream(part1Bytes))
		{
			using var br = new EndianBinaryReader(part1Stream, true);
			entryTable = await NefsReaderStrategy160.ReadHeaderPart1Async(br, part1Offset, part1Size, this.p);
		}

		// Part 6 data
		byte[] bytes =
		{
			// 5 bytes offset
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

			// Entry 1
			0x11, 0x12, 0x13, 0x14,

			// Entry 2
			0x21, 0x22, 0x23, 0x24,
		};

		// Test
		using (var part6Stream = new MemoryStream(bytes))
		{
			using var br = new EndianBinaryReader(part6Stream, true);
			part6 = await NefsReaderStrategy160.Read160HeaderPart6Async(br, part6Offset, entryTable.Entries.Count, this.p);
		}

		// Verify
		Assert.Equal(2, part6.Entries.Count);

		var e1 = part6.Entries[0];
		Assert.Equal(0x1211, e1.Volume);
		Assert.Equal(0x1413, e1.Flags);

		var e2 = part6.Entries[1];
		Assert.Equal(0x2221, e2.Volume);
		Assert.Equal(0x2423, e2.Flags);
	}

	[Fact]
	public async Task ReadHeaderPart7Async_ValidData_DataRead()
	{
		byte[] bytes =
		{
			// 5 bytes offset
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

			// Entry 1
			0x11, 0x12, 0x13, 0x14,
			0x15, 0x16, 0x17, 0x18,

			// Entry 2
			0x21, 0x22, 0x23, 0x24,
			0x25, 0x26, 0x27, 0x28,
		};

		var stream = new MemoryStream(bytes);
		using var br = new EndianBinaryReader(stream, true);
		var offset = 5;

		// Test
		var part7 = await NefsReaderStrategy160.Read160HeaderPart7Async(br, offset, 2, this.p);

		// Verify
		Assert.Equal(2, part7.Entries.Count);

		var e1 = part7.Entries[0];
		Assert.Equal((uint)0x14131211, e1.NextSibling);
		Assert.Equal((uint)0x18171615, e1.PatchedEntry);

		var e2 = part7.Entries[1];
		Assert.Equal((uint)0x24232221, e2.NextSibling);
		Assert.Equal((uint)0x28272625, e2.PatchedEntry);
	}
}
