// See LICENSE.txt for license information.

using System.IO.Abstractions.TestingHelpers;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

public class NefsReaderTests
{
	private readonly MockFileSystem fileSystem = new MockFileSystem();

	private readonly NefsProgress p = new NefsProgress(CancellationToken.None);

	[Fact]
	public async void ReadHeaderPart1Async_ExtraBytesAtEnd_ExtraBytesIgnored()
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
		var reader = new NefsReader(this.fileSystem);
		var size = NefsHeaderPart1.EntrySize + 2;
		var offset = 5;

		// Test
		var part1 = await reader.ReadHeaderPart1Async(stream, offset, size, this.p);

		// Verify
		Assert.Single(part1.EntriesByIndex);

		var e1 = part1.EntriesByIndex[0];
		Assert.Equal((ulong)0x0807060504030201, e1.OffsetToData);
		Assert.Equal((uint)0x14131211, e1.IndexPart2);
		Assert.Equal((uint)0x24232221, e1.IndexPart4);
		Assert.Equal((uint)0x34333231, e1.Id.Value);
	}

	[Fact]
	public async void ReadHeaderPart1Async_OffsetOutOfBounds_NoEntries()
	{
		var bytes = new byte[10];
		var stream = new MemoryStream(bytes);
		var reader = new NefsReader(this.fileSystem);

		// Test
		var part1 = await reader.ReadHeaderPart1Async(stream, 10, 5, this.p);

		// Verify
		Assert.Empty(part1.EntriesByIndex);
	}

	[Fact]
	public async void ReadHeaderPart1Async_SizeOutOfBounds_NoEntries()
	{
		var bytes = new byte[10];
		var stream = new MemoryStream(bytes);
		var reader = new NefsReader(this.fileSystem);

		// Test
		var part1 = await reader.ReadHeaderPart1Async(stream, 0, 20, this.p);

		// Verify
		Assert.Empty(part1.EntriesByIndex);
	}

	[Fact]
	public async void ReadHeaderPart1Async_ValidData_DataRead()
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
		var reader = new NefsReader(this.fileSystem);
		var size = 2 * NefsHeaderPart1.EntrySize;
		var offset = 5;

		// Test
		var part1 = await reader.ReadHeaderPart1Async(stream, offset, size, this.p);

		// Verify
		Assert.Equal(2, part1.EntriesByIndex.Count);

		var e1 = part1.EntriesByIndex[0];
		Assert.Equal((ulong)0x0807060504030201, e1.OffsetToData);
		Assert.Equal((uint)0x14131211, e1.IndexPart2);
		Assert.Equal((uint)0x24232221, e1.IndexPart4);
		Assert.Equal((uint)0x34333231, e1.Id.Value);

		var e2 = part1.EntriesByIndex[1];
		Assert.Equal((ulong)0x4847464544434241, e2.OffsetToData);
		Assert.Equal((uint)0x54535251, e2.IndexPart2);
		Assert.Equal((uint)0x64636261, e2.IndexPart4);
		Assert.Equal((uint)0x74737271, e2.Id.Value);
	}

	[Fact]
	public async void ReadHeaderPart1Async_ZeroSize_NoEntries()
	{
		var bytes = new byte[2];
		var stream = new MemoryStream(bytes);
		var reader = new NefsReader(this.fileSystem);

		// Test
		var part1 = await reader.ReadHeaderPart1Async(stream, 0, 0, this.p);

		// Verify
		Assert.Empty(part1.EntriesByIndex);
	}

	[Fact]
	public async void ReadHeaderPart2Async_ExtraBytesAtEnd_ExtraBytesIgnored()
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
		var reader = new NefsReader(this.fileSystem);
		var size = NefsHeaderPart2.EntrySize + 2;
		var offset = 5;

		// Test
		var part2 = await reader.ReadHeaderPart2Async(stream, offset, size, this.p);

		// Verify
		Assert.Single(part2.EntriesByIndex);

		var e1 = part2.EntriesByIndex[0];
		Assert.Equal((uint)0x04030201, e1.DirectoryId.Value);
		Assert.Equal((uint)0x14131211, e1.FirstChildId.Value);
		Assert.Equal((uint)0x24232221, e1.OffsetIntoPart3);
		Assert.Equal((uint)0x34333231, e1.ExtractedSize);
		Assert.Equal((uint)0x44434241, e1.Id.Value);
	}

	[Fact]
	public async void ReadHeaderPart2Async_ValidData_DataRead()
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
		var reader = new NefsReader(this.fileSystem);
		var size = 2 * NefsHeaderPart2.EntrySize;
		var offset = 5;

		// Test
		var part2 = await reader.ReadHeaderPart2Async(stream, offset, size, this.p);

		// Verify
		Assert.Equal(2, part2.EntriesByIndex.Count);

		var e1 = part2.EntriesByIndex[0];
		Assert.Equal((uint)0x04030201, e1.DirectoryId.Value);
		Assert.Equal((uint)0x14131211, e1.FirstChildId.Value);
		Assert.Equal((uint)0x24232221, e1.OffsetIntoPart3);
		Assert.Equal((uint)0x34333231, e1.ExtractedSize);
		Assert.Equal((uint)0x44434241, e1.Id.Value);

		var e2 = part2.EntriesByIndex[1];
		Assert.Equal((uint)0x54535251, e2.DirectoryId.Value);
		Assert.Equal((uint)0x64636261, e2.FirstChildId.Value);
		Assert.Equal((uint)0x74737271, e2.OffsetIntoPart3);
		Assert.Equal((uint)0x84838281, e2.ExtractedSize);
		Assert.Equal((uint)0x94939291, e2.Id.Value);
	}

	[Fact]
	public async void ReadHeaderPart3Async_DoubleTerminators_StringsRead()
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
		var reader = new NefsReader(this.fileSystem);
		var size = 9;
		var offset = 2;

		// Test
		var part3 = await reader.ReadHeaderPart3Async(stream, offset, size, this.p);

		// Verify
		Assert.Equal(4, part3.FileNamesByOffset.Count);
		Assert.Equal(4, part3.OffsetsByFileName.Count);
		Assert.Equal("AB", part3.FileNamesByOffset[0]);
		Assert.Equal("C", part3.FileNamesByOffset[3]);
		Assert.Equal("", part3.FileNamesByOffset[5]);
		Assert.Equal("EF", part3.FileNamesByOffset[6]);
	}

	[Fact]
	public async void ReadHeaderPart3Async_NoEndingTerminator_StringsRead()
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
		var reader = new NefsReader(this.fileSystem);
		var size = 9;
		var offset = 2;

		// Test
		var part3 = await reader.ReadHeaderPart3Async(stream, offset, size, this.p);

		// Verify
		Assert.Equal(2, part3.FileNamesByOffset.Count);
		Assert.Equal(2, part3.OffsetsByFileName.Count);
		Assert.Equal("AB", part3.FileNamesByOffset[0]);
		Assert.Equal("CD", part3.FileNamesByOffset[3]);
	}

	[Fact]
	public async void ReadHeaderPart3Async_ValidData_StringsRead()
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
		var reader = new NefsReader(this.fileSystem);
		var size = 9;
		var offset = 2;

		// Test
		var part3 = await reader.ReadHeaderPart3Async(stream, offset, size, this.p);

		// Verify
		Assert.Equal(3, part3.FileNamesByOffset.Count);
		Assert.Equal(3, part3.OffsetsByFileName.Count);
		Assert.Equal("AB", part3.FileNamesByOffset[0]);
		Assert.Equal("CD", part3.FileNamesByOffset[3]);
		Assert.Equal("EF", part3.FileNamesByOffset[6]);
	}

	[Fact]
	public async void ReadHeaderPart4Async_ValidData_DataRead()
	{
		// Item 1 has 2 chunk sizes
		var e1p1 = new NefsHeaderPart1Entry(Guid.NewGuid())
		{
			Id = new NefsItemId(0),
			IndexPart4 = 0,
		};

		// Item 2 has 1 chunk size
		var e2p1 = new NefsHeaderPart1Entry(Guid.NewGuid())
		{
			Id = new NefsItemId(1),
			IndexPart4 = 2,
		};

		// Item 3 has no chunks
		var e3p1 = new NefsHeaderPart1Entry(Guid.NewGuid())
		{
			Id = new NefsItemId(2),
			IndexPart4 = 0xFFFFFFFF,
		};

		// Item 4 is a directory (extracted size == 0)
		var e4p1 = new NefsHeaderPart1Entry(Guid.NewGuid())
		{
			Id = new NefsItemId(3),
			IndexPart4 = 0,
		};

		// Item 5 has 3 chunks
		var e5p1 = new NefsHeaderPart1Entry(Guid.NewGuid())
		{
			Id = new NefsItemId(4),
			IndexPart4 = 3,
		};

		var part1Items = new List<NefsHeaderPart1Entry>
		{
			e1p1,
			e2p1,
			e3p1,
			e4p1,
			e5p1,
		};

		var part1 = new NefsHeaderPart1(part1Items);

		// Setup data
		byte[] bytes =
		{
			// Offset
			0xFF, 0xFF,

			// Item 1
			0x11, 0x12, 0x13, 0x14,
			0x15, 0x16, 0x17, 0x18,

			// Item 2
			0x21, 0x22, 0x23, 0x24,

			// Item 5
			0x31, 0x32, 0x33, 0x34,
			0x35, 0x36, 0x37, 0x38,
			0x39, 0x3A, 0x3B, 0x3C,

			// Last four bytes
			0x01, 0x02, 0x03, 0x04,
		};

		var stream = new MemoryStream(bytes);
		var reader = new NefsReader(this.fileSystem);
		var size = 28;
		var offset = 2;

		// Test
		var part4 = await reader.ReadHeaderPart4Version20Async(stream, offset, size, part1, this.p);

		// Verify
		Assert.Equal(7, part4.EntriesByIndex.Count);

		// Item 1
		Assert.Equal((uint)0x14131211, part4.EntriesByIndex[0].CumulativeChunkSize);
		Assert.Equal((uint)0x18171615, part4.EntriesByIndex[1].CumulativeChunkSize);

		// Item 2
		Assert.Equal((uint)0x24232221, part4.EntriesByIndex[2].CumulativeChunkSize);

		// Item 3
		Assert.Equal((uint)0x34333231, part4.EntriesByIndex[3].CumulativeChunkSize);
		Assert.Equal((uint)0x38373635, part4.EntriesByIndex[4].CumulativeChunkSize);
		Assert.Equal((uint)0x3C3B3A39, part4.EntriesByIndex[5].CumulativeChunkSize);
	}

	[Fact]
	public async void ReadHeaderPart5Async_ValidData_DataRead()
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
		var reader = new NefsReader(this.fileSystem);
		var size = 16;
		var offset = 5;

		// Test
		var part5 = await reader.ReadHeaderPart5Async(stream, offset, size, this.p);

		// Verify
		Assert.Equal((ulong)0x1817161514131211, part5.DataSize);
		Assert.Equal((uint)0x24232221, part5.DataFileNameStringOffset);
		Assert.Equal((uint)0x28272625, part5.FirstDataOffset);
	}

	[Fact]
	public async void ReadHeaderPart6Async_ValidData_DataRead()
	{
		var reader = new NefsReader(this.fileSystem);
		NefsHeaderPart1 part1;
		Nefs20HeaderPart6 part6;

		var part1Offset = 5;
		var part1Size = 2 * NefsHeaderPart1.EntrySize;

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
			part1 = await reader.ReadHeaderPart1Async(part1Stream, part1Offset, part1Size, this.p);
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
			part6 = await reader.ReadHeaderPart6Version20Async(part6Stream, part6Offset, part1, this.p);
		}

		// Verify
		Assert.Equal(2, part6.EntriesByIndex.Count);

		var e1 = part6.EntriesByIndex[0];
		Assert.Equal(0x1211, e1.Volume);
		Assert.Equal(0x13, (byte)e1.Flags);
		Assert.Equal(0x14, e1.Unknown0x3);
		Assert.Same(e1, part6.EntriesByGuid[part1.EntriesByIndex[0].Guid]);

		var e2 = part6.EntriesByIndex[1];
		Assert.Equal(0x2221, e2.Volume);
		Assert.Equal(0x23, (byte)e2.Flags);
		Assert.Equal(0x24, e2.Unknown0x3);
		Assert.Same(e2, part6.EntriesByGuid[part1.EntriesByIndex[1].Guid]);
	}

	[Fact]
	public async void ReadHeaderPart7Async_ValidData_DataRead()
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
		var reader = new NefsReader(this.fileSystem);
		var offset = 5;

		// Test
		var part7 = await reader.ReadHeaderPart7Async(stream, offset, 2, this.p);

		// Verify
		Assert.Equal(2, part7.EntriesByIndex.Count);

		var e1 = part7.EntriesByIndex[0];
		Assert.Equal((uint)0x14131211, e1.SiblingId.Value);
		Assert.Equal((uint)0x18171615, e1.Id.Value);

		var e2 = part7.EntriesByIndex[1];
		Assert.Equal((uint)0x24232221, e2.SiblingId.Value);
		Assert.Equal((uint)0x28272625, e2.Id.Value);
	}
}
