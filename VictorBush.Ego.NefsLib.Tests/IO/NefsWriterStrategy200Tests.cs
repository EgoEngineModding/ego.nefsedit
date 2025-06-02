// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

public class NefsWriterStrategy200Tests
{
	private readonly NefsProgress p = new(CancellationToken.None);

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

		var builder = new NefsHeaderBuilder200();
		var header = builder.Build(new NefsHeader200(), items, this.p);
		var writer = new NefsWriterStrategy200();

		/*
		Write
		*/

		byte[] buffer;
		var offset = 5;

		using (var ms = new MemoryStream())
		{
			using var _ = this.p.BeginTask(1);
			using var bw = new EndianBinaryWriter(ms);
			await writer.WriteTocTableAsync(bw, offset, header.BlockTable, this.p);
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
	public async Task WriterHeaderIntroTocAsync_ValidData_Written()
	{
		// This chunk of data is unknown, but it must be 0x5C bytes long
		var data0x24_Unknown = new byte[0x5C];
		data0x24_Unknown[0] = 20;

		var toc = new NefsTocHeaderB200
		{
			HashBlockSize = 505 << 15,
			NumVolumes = 404,
			EntryTableStart = 111,
			SharedEntryInfoTableStart = 222,
			NameTableStart = 333,
			BlockTableStart = 444,
			VolumeInfoTableStart = 555,
			WritableEntryTableStart = 666,
			WritableSharedEntryInfoTableStart = 777,
			HashDigestTableStart = 888,
			RandomPadding = data0x24_Unknown,
		};
		var writer = new NefsWriterStrategy200();

		/*
		Write
		*/

		byte[] buffer;
		var offset = 10;

		using (var ms = new MemoryStream())
		{
			using var bw = new EndianBinaryWriter(ms);
			await writer.WriteTocEntryAsync(bw, offset, toc, this.p);
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
}
