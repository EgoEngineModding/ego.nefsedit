// See LICENSE.txt for license information.

using System.IO.Abstractions.TestingHelpers;
using System.Text;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Source.Utility;
using VictorBush.Ego.NefsLib.Tests.TestArchives;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

public class NefsWriterTests
{
	private const string TempDir = @"C:\temp";
	private readonly MockFileSystem fileSystem = new MockFileSystem();
	private readonly INefsTransformer transformer;

	public NefsWriterTests()
	{
		this.fileSystem.AddDirectory(TempDir);
		this.transformer = new NefsTransformer(this.fileSystem);
	}

	[Fact]
	public async Task WriteArchiveAsync_ArchiveNotModified_ArchiveWritten()
	{
		var sourcePath = @"C:\archive.nefs";
		var destPath = @"C:\dest.nefs";
		this.fileSystem.AddFile(sourcePath, new MockFileData("hi"));
		var sourceArchive = TestArchiveNotModified.Create(sourcePath);
		var writer = CreateWriter();
		var (archive, _) = await writer.WriteArchiveAsync(@"C:\dest.nefs", sourceArchive, new NefsProgress());

		Assert.Equal(sourceArchive.Items.Count, archive.Items.Count);

		// Try to read archive again
		var reader = new NefsReader(this.fileSystem);
		var readArchive = await reader.ReadArchiveAsync(destPath, new NefsProgress());
		Assert.Equal(sourceArchive.Items.Count, readArchive.Items.Count);
	}

	[Fact]
	public async Task WriteHeaderPart1Async_ValidData_Written()
	{
		var items = new NefsItemList(@"C:\hi.txt");
		var file1 = TestHelpers.CreateItem(0, 0, "file1", 10, 11, new List<uint> { 12, 13 }, NefsItemType.File);
		var file2 = TestHelpers.CreateItem(1, 1, "file2", 20, 21, new List<uint> { 22, 23 }, NefsItemType.File);
		var dir1 = TestHelpers.CreateItem(2, 2, "dir1", 0, 0, new List<uint> { 0 }, NefsItemType.Directory);
		items.Add(file1);
		items.Add(file2);
		items.Add(dir1);

		var part4 = new Nefs20HeaderPart4(items);
		var part1 = new NefsHeaderPart1(items, part4);

		/*
		Write
		*/

		var writer = CreateWriter();
		byte[] buffer;
		var offset = 5;

		using (var ms = new MemoryStream())
		{
			await writer.WriteHeaderPartWithEntriesAsync(ms, offset, part1.EntriesByIndex, new NefsProgress());
			buffer = ms.ToArray();
		}

		/*
		Verify
		*/

		/*
		file1
		*/

		// Data offset (8 bytes)
		Assert.Equal(10, BitConverter.ToInt64(buffer, offset + 0));

		// Index part 2
		Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 8));

		// Index part 4
		Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x0c));

		// Item id
		Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x10));

		/*
		file2
		*/

		offset += NefsHeaderPart1.EntrySize;

		// Data offset (8 bytes)
		Assert.Equal(20, BitConverter.ToInt64(buffer, offset + 0));

		// Index part 2
		Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 8));

		// Index part 4
		Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0x0c));

		// Item id
		Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 0x10));

		/*
		dir1
		*/

		offset += NefsHeaderPart1.EntrySize;

		// Data offset (8 bytes)
		Assert.Equal(0, BitConverter.ToInt64(buffer, offset + 0));

		// Index part 2
		Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 8));

		// Index part 4
		Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x0c));

		// Item id
		Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0x10));
	}

	[Fact]
	public async Task WriteHeaderPart2Async_ValidData_Written()
	{
		var items = new NefsItemList(@"C:\hi.txt");
		var file1 = TestHelpers.CreateItem(0, 0, "file1", 10, 11, new List<uint> { 12, 13 }, NefsItemType.File);
		var file2 = TestHelpers.CreateItem(1, 1, "file2", 20, 21, new List<uint> { 22, 23 }, NefsItemType.File);
		var dir1 = TestHelpers.CreateItem(2, 2, "dir1", 0, 0, new List<uint> { 0 }, NefsItemType.Directory);
		var file3 = TestHelpers.CreateItem(3, 2, "file3", 30, 31, new List<uint> { 32, 33 }, NefsItemType.File);
		items.Add(file1);
		items.Add(file2);
		items.Add(dir1);
		items.Add(file3);

		var part3 = new NefsHeaderPart3(items);
		var part2 = new NefsHeaderPart2(items, part3);

		/*
		Write
		*/

		var writer = CreateWriter();
		byte[] buffer;
		var offset = 5;

		using (var ms = new MemoryStream())
		{
			await writer.WriteHeaderPartWithEntriesAsync(ms, offset, part2.EntriesByIndex, new NefsProgress());
			buffer = ms.ToArray();
		}

		/*
		Verify
		*/

		/*
		dir1
		*/

		// Dir id
		Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0));

		// First child id
		Assert.Equal(3, BitConverter.ToInt32(buffer, offset + 0x04));

		// Part 3 offset
		Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x08));

		// Extracted size
		Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x0c));

		// Item id
		Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0x10));

		/*
		file3
		*/

		offset += NefsHeaderPart2.EntrySize;

		// Dir id
		Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0));

		// First child id
		Assert.Equal(3, BitConverter.ToInt32(buffer, offset + 0x04));

		// Part 3 offset
		Assert.Equal(17, BitConverter.ToInt32(buffer, offset + 0x08));

		// Extracted size
		Assert.Equal(31, BitConverter.ToInt32(buffer, offset + 0x0c));

		// Item id
		Assert.Equal(3, BitConverter.ToInt32(buffer, offset + 0x10));

		/*
		file1
		*/

		offset += NefsHeaderPart2.EntrySize;

		// Dir id
		Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0));

		// First child id
		Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x04));

		// Part 3 offset
		Assert.Equal(5, BitConverter.ToInt32(buffer, offset + 0x08));

		// Extracted size
		Assert.Equal(11, BitConverter.ToInt32(buffer, offset + 0x0c));

		// Item id
		Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x10));

		/*
		file2
		*/

		offset += NefsHeaderPart2.EntrySize;

		// Dir id
		Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 0));

		// First child id
		Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 0x04));

		// Part 3 offset
		Assert.Equal(11, BitConverter.ToInt32(buffer, offset + 0x08));

		// Extracted size
		Assert.Equal(21, BitConverter.ToInt32(buffer, offset + 0x0c));

		// Item id
		Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 0x10));
	}

	[Fact]
	public async Task WriteHeaderPart3Async_ValidData_Written()
	{
		var items = new NefsItemList(@"C:\hi.txt");
		var file1 = TestHelpers.CreateItem(0, 0, "file1", 10, 11, new List<uint> { 12, 13 }, NefsItemType.File);
		var file2 = TestHelpers.CreateItem(1, 1, "file2", 20, 21, new List<uint> { 22, 23 }, NefsItemType.File);
		var dir1 = TestHelpers.CreateItem(2, 2, "dir1", 0, 0, new List<uint> { 0 }, NefsItemType.Directory);
		var file3 = TestHelpers.CreateItem(3, 2, "file3", 30, 31, new List<uint> { 32, 33 }, NefsItemType.File);
		items.Add(file1);
		items.Add(file2);
		items.Add(dir1);
		items.Add(file3);

		var part3 = new NefsHeaderPart3(items);

		/*
		Write
		*/

		var writer = CreateWriter();
		byte[] buffer;
		var offset = 5;

		using (var ms = new MemoryStream())
		{
			await writer.WriteHeaderPart3Async(ms, (uint)offset, part3, new NefsProgress());
			buffer = ms.ToArray();
		}

		/*
		Verify
		*/

		// Null terminated strings
		Assert.Equal("dir1", Encoding.ASCII.GetString(buffer, offset + 0, 4));
		Assert.Equal(0, buffer[offset + 4]);
		Assert.Equal("file1", Encoding.ASCII.GetString(buffer, offset + 5, 5));
		Assert.Equal(0, buffer[offset + 10]);
		Assert.Equal("file2", Encoding.ASCII.GetString(buffer, offset + 11, 5));
		Assert.Equal(0, buffer[offset + 16]);
		Assert.Equal("file3", Encoding.ASCII.GetString(buffer, offset + 17, 5));
		Assert.Equal(0, buffer[offset + 22]);
	}

	[Fact]
	public async Task WriteHeaderPart4Async_ValidData_Written()
	{
		var items = new NefsItemList(@"C:\hi.txt");
		var file1 = TestHelpers.CreateItem(0, 0, "file1", 10, 11, new List<uint> { 12, 13 }, NefsItemType.File);
		var file2 = TestHelpers.CreateItem(1, 1, "file2", 20, 21, new List<uint> { 22, 23 }, NefsItemType.File);
		var dir1 = TestHelpers.CreateItem(2, 2, "dir1", 0, 0, new List<uint> { 0 }, NefsItemType.Directory);
		var file3 = TestHelpers.CreateItem(3, 2, "file3", 30, 31, new List<uint> { 32, 33 }, NefsItemType.File);
		items.Add(file1);
		items.Add(file2);
		items.Add(dir1);
		items.Add(file3);

		var part4 = new Nefs20HeaderPart4(items);

		/*
		Write
		*/

		var writer = CreateWriter();
		byte[] buffer;
		var offset = 5;

		using (var ms = new MemoryStream())
		{
			await writer.WriteHeaderPartWithEntriesAsync(ms, offset, part4.EntriesByIndex, new NefsProgress());
			buffer = ms.ToArray();
		}

		/*
		Verify
		*/

		Assert.Equal(24 + offset, buffer.Length);
		Assert.Equal(12, BitConverter.ToInt32(buffer, offset + 0));
		Assert.Equal(13, BitConverter.ToInt32(buffer, offset + 4));
		Assert.Equal(22, BitConverter.ToInt32(buffer, offset + 8));
		Assert.Equal(23, BitConverter.ToInt32(buffer, offset + 12));
		Assert.Equal(32, BitConverter.ToInt32(buffer, offset + 16));
		Assert.Equal(33, BitConverter.ToInt32(buffer, offset + 20));
	}

	[Fact]
	public async Task WriteHeaderPart5Async_ValidData_Written()
	{
		var part5 = new NefsHeaderPart5
		{
			DataSize = 1234,
			DataFileNameStringOffset = 98,
			FirstDataOffset = 56,
		};

		/*
		Write
		*/

		var writer = CreateWriter();
		byte[] buffer;
		var offset = 5;

		using (var ms = new MemoryStream())
		{
			await writer.WriteHeaderPartAsync(ms, offset, part5, new NefsProgress());
			buffer = ms.ToArray();
		}

		/*
		Verify
		*/

		Assert.Equal(1234, BitConverter.ToInt64(buffer, offset + 0));
		Assert.Equal(98, BitConverter.ToInt32(buffer, offset + 8));
		Assert.Equal(56, BitConverter.ToInt32(buffer, offset + 12));
	}

	[Fact]
	public async Task WriterHeaderIntroAsync_ValidData_Written()
	{
		var aes = new byte[] { 0xE5, 0x69, 0x65, 0x23, 0xAB, 0xF5, 0x43, 0xFF, 0xC9, 0xDF, 0xB2, 0x2C, 0x64, 0xD1, 0x11, 0x46, 0xE5, 0x9B, 0xAC, 0xC8, 0xAC, 0x8B, 0xA4, 0x15, 0x9E, 0xE0, 0xE2, 0xBB, 0x54, 0x09, 0x0A, 0x6C, 0x99, 0x30, 0xC6, 0xC1, 0x84, 0x3C, 0x90, 0x29, 0x75, 0xB2, 0xB5, 0x5E, 0x3B, 0x7A, 0x06, 0x3D, 0xE1, 0xD2, 0x1F, 0x6F, 0xB7, 0xDC, 0x57, 0x5A, 0xC4, 0x4F, 0x84, 0xCB, 0x13, 0x87, 0xAB, 0xBF };
		var hash = new byte[] { 0xCB, 0x13, 0x87, 0xAB, 0xBF, 0xD5, 0x45, 0x93, 0x34, 0x0A, 0x50, 0xC1, 0xA8, 0x0A, 0x82, 0x53, 0xF9, 0xD5, 0x46, 0xDA, 0x24, 0xDA, 0xA4, 0xDA, 0x82, 0xEA, 0x9A, 0xB5, 0xBC, 0xD8, 0x6B, 0xFC };

		var intro = new NefsHeaderIntro
		{
			AesKeyHexString = aes,
			ExpectedHash = new Sha256Hash(hash),
			HeaderSize = 12345,
			NumberOfItems = 9876,
			NefsVersion = 101,
			Unknown0x70zlib = 202,
			Unknown0x78 = 303,
		};

		/*
		Write
		*/

		var writer = CreateWriter();
		byte[] buffer;

		using (var ms = new MemoryStream())
		{
			await writer.WriteHeaderPartAsync(ms, 0, intro, new NefsProgress());
			buffer = ms.ToArray();
		}

		/*
		Verify
		*/

		// Magic #
		Assert.Equal(0x5346654EU, BitConverter.ToUInt32(buffer, 0));

		// Expected hash
		VerifyArrraySlice(hash, 0, buffer, 0x04, 0x20);

		// AES key
		VerifyArrraySlice(aes, 0, buffer, 0x24, 0x40);

		// Header size
		Assert.Equal(12345, BitConverter.ToInt32(buffer, 0x64));

		// 0x68 unknwon
		Assert.Equal(101, BitConverter.ToInt32(buffer, 0x68));

		// Number of items
		Assert.Equal(9876, BitConverter.ToInt32(buffer, 0x6C));

		// 0x70 unknown (8 bytes)
		Assert.Equal(202, BitConverter.ToInt64(buffer, 0x70));

		// 0x78 unknwon (8 bytes)
		Assert.Equal(303, BitConverter.ToInt64(buffer, 0x78));
	}

	[Fact]
	public async Task WriterHeaderIntroTocAsync_ValidData_Written()
	{
		// This chunk of data is unknown, but it must be 0x5C bytes long
		var data0x24_Unknown = new byte[0x5C];
		data0x24_Unknown[0] = 20;

		var toc = new Nefs20HeaderIntroToc
		{
			HashBlockSize = 505 << 15,
			NumVolumes = 404,
			OffsetToPart1 = 111,
			OffsetToPart2 = 222,
			OffsetToPart3 = 333,
			OffsetToPart4 = 444,
			OffsetToPart5 = 555,
			OffsetToPart6 = 666,
			OffsetToPart7 = 777,
			OffsetToPart8 = 888,
			Unknown0x24 = data0x24_Unknown,
		};

		/*
		Write
		*/

		var writer = CreateWriter();
		byte[] buffer;
		var offset = 10;

		using (var ms = new MemoryStream())
		{
			await writer.WriteHeaderPartAsync(ms, offset, toc, new NefsProgress());
			buffer = ms.ToArray();
		}

		/*
		Verify
		*/

		// Num volumes
		Assert.Equal(404, BitConverter.ToInt16(buffer, offset + 0x00));

		// Hash block size
		Assert.Equal(505, BitConverter.ToInt16(buffer, offset + 0x02));

		// Offset to part 1
		Assert.Equal(111, BitConverter.ToInt32(buffer, offset + 0x04));

		// Offset to part 6
		Assert.Equal(666, BitConverter.ToInt32(buffer, offset + 0x08));

		// Offset to part 2
		Assert.Equal(222, BitConverter.ToInt32(buffer, offset + 0x0C));

		// Offset to part 7
		Assert.Equal(777, BitConverter.ToInt32(buffer, offset + 0x10));

		// Offset to part 3
		Assert.Equal(333, BitConverter.ToInt32(buffer, offset + 0x14));

		// Offset to part 4
		Assert.Equal(444, BitConverter.ToInt32(buffer, offset + 0x18));

		// Offset to part 5
		Assert.Equal(555, BitConverter.ToInt32(buffer, offset + 0x1C));

		// Offset to part 8
		Assert.Equal(888, BitConverter.ToInt32(buffer, offset + 0x20));

		// 0x24 Unknown
		Assert.Equal(20, buffer[offset + 0x24]);
	}

	private NefsWriter CreateWriter()
	{
		return new NefsWriter(TempDir, this.fileSystem, this.transformer);
	}

	private void VerifyArrraySlice(
		byte[] expectedBuffer,
		int expectedOffset,
		byte[] actualBuffer,
		int actualOffset,
		int length)
	{
		var expected = expectedOffset;
		var actual = actualOffset;

		for (; actual < length; ++actual, ++expected)
		{
			Assert.Equal(expectedBuffer[expected], actualBuffer[actual]);
		}
	}
}
