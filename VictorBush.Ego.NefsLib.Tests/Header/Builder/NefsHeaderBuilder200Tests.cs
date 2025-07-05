// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Tests.DataSource;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.Header.Builder;

public class NefsHeaderBuilder200Tests
{
	private readonly NefsItem dir1;
	private readonly NefsItem file1;
	private readonly NefsItem file2;
	private readonly NefsItem file3;
	private readonly NefsItem file4NotCompressed;
	private readonly NefsItemList testItems;

	public NefsHeaderBuilder200Tests()
	{
		var items = new NefsItemList(@"C:\archive.nefs");

		var file1Chunks = NefsDataChunk.CreateChunkList(new List<uint> { 1, 11, 21 }, TestHelpers.TestTransform);
		var file1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456, file1Chunks));
		this.file1 = TestHelpers.CreateFile(0, 0, "file1", file1DataSource);
		items.Add(this.file1);

		var file2Chunks = NefsDataChunk.CreateChunkList(new List<uint> { 2, 22, 52 }, TestHelpers.TestTransform);
		var file2DataSource = new NefsItemListDataSource(items, 456, new NefsItemSize(789, file2Chunks));
		this.file2 = TestHelpers.CreateFile(1, 1, "file2", file2DataSource);
		items.Add(this.file2);

		this.dir1 = TestHelpers.CreateDirectory(2, 2, "dir1");
		items.Add(this.dir1);

		var file3Chunks = NefsDataChunk.CreateChunkList(new List<uint> { 3, 13, 23 }, TestHelpers.TestTransform);
		var file3DataSource = new NefsItemListDataSource(items, 222, new NefsItemSize(333, file3Chunks));
		this.file3 = TestHelpers.CreateFile(3, this.dir1.Id.Value, "file3", file3DataSource);
		items.Add(this.file3);

		var file4DataSource = new NefsItemListDataSource(items, 777, new NefsItemSize(444));
		this.file4NotCompressed = TestHelpers.CreateFile(4, this.dir1.Id.Value, "file4", file4DataSource);
		items.Add(this.file4NotCompressed);

		this.testItems = items;
	}

	[Fact]
	public void Build_EntryTable_NoItems_EntriesEmpty()
	{
		var items = new NefsItemList(@"C:\archive.nefs");
		var builder = new NefsHeaderBuilder200();
		var header = builder.Build(new NefsHeader200(), items, new NefsProgress());
		Assert.Empty(header.EntryTable.Entries);
	}

	[Fact]
	public void Build_EntryTable_MultipleItems_EntriesPopulated()
	{
		var items = new NefsItemList(@"C:\archive.nefs");

		var file1Chunks = NefsDataChunk.CreateChunkList(new List<uint> { 11, 12, 13 }, TestHelpers.TestTransform);
		var file1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456, file1Chunks));
		var file1 = TestHelpers.CreateFile(0, 0, "file1", file1DataSource);
		items.Add(file1);

		var file2Chunks = NefsDataChunk.CreateChunkList(new List<uint> { 14, 15, 16 }, TestHelpers.TestTransform);
		var file2DataSource = new NefsItemListDataSource(items, 456, new NefsItemSize(789, file2Chunks));
		var file2 = TestHelpers.CreateFile(1, 1, "file2", file2DataSource);
		items.Add(file2);

		var dir1 = TestHelpers.CreateDirectory(2, 2, "dir1");
		items.Add(dir1);

		var file3 = TestHelpers.CreateFile(3, 3, "file1", file1DataSource, file1.Id.Value);
		items.Add(file3);

		var builder = new NefsHeaderBuilder200();
		var header = builder.Build(new NefsHeader200(), items, new NefsProgress());
		var p1 = header.EntryTable;

		Assert.Equal(4, p1.Entries.Count);

		/*
		dir1
		*/

		// Offset to data and index to p4 are both 0 since this is a directory
		Assert.Equal(2, (int)p1.Entries[dir1.Id.Index].NextDuplicate);
		Assert.Equal(0, (int)p1.Entries[dir1.Id.Index].Start);
		Assert.Equal(0, (int)p1.Entries[dir1.Id.Index].SharedInfo);
		Assert.Equal(0, (int)p1.Entries[dir1.Id.Index].FirstBlock);

		/*
		file1
		*/

		Assert.Equal(3, (int)p1.Entries[file1.Id.Index].NextDuplicate);
		Assert.Equal(123, (int)p1.Entries[file1.Id.Index].Start);
		Assert.Equal(1, (int)p1.Entries[file1.Id.Index].SharedInfo);
		Assert.Equal(0, (int)p1.Entries[file1.Id.Index].FirstBlock);

		/*
		file2
		*/

		Assert.Equal(1, (int)p1.Entries[file2.Id.Index].NextDuplicate);
		Assert.Equal(456, (int)p1.Entries[file2.Id.Index].Start);
		Assert.Equal(2, (int)p1.Entries[file2.Id.Index].SharedInfo);

		// There are 3 chunks for file1, so file2's chunks start right after that (hence p4 index == 3)
		Assert.Equal(3, (int)p1.Entries[file2.Id.Index].FirstBlock);

		/*
		file3
		*/

		Assert.Equal(3, (int)p1.Entries[file3.Id.Index].NextDuplicate);
		Assert.Equal(123, (int)p1.Entries[file3.Id.Index].Start);
		Assert.Equal(1, (int)p1.Entries[file3.Id.Index].SharedInfo);
		Assert.Equal(0, (int)p1.Entries[file3.Id.Index].FirstBlock);
	}

	[Fact]
	public void Build_BlockTable_NoItems_EntriesEmpty()
	{
		var items = new NefsItemList(@"C:\archive.nefs");
		var builder = new NefsHeaderBuilder200();
		var header = builder.Build(new NefsHeader200(), items, new NefsProgress());
		Assert.Empty(header.BlockTable.Entries);
		Assert.Equal(0, header.BlockTable.ByteCount());
	}

	[Fact]
	public void Build_BlockTable_MultipleItems_EntriesPopulated()
	{
		var builder = new NefsHeaderBuilder200();
		var header = builder.Build(new NefsHeader200(), this.testItems, new NefsProgress());
		var p4 = header.BlockTable;

		// Dir 1 and file 4 should not have entries. Only compressed files have entries in part 4.
		Assert.Equal(9, p4.Entries.Count);

		// Total size is (total number of chunks * bytes)
		Assert.Equal(36, p4.ByteCount());

		// File 1
		Assert.Equal(1U, p4.Entries[0].End);
		Assert.Equal(11U, p4.Entries[1].End);
		Assert.Equal(21U, p4.Entries[2].End);

		// File 2
		Assert.Equal(2U, p4.Entries[3].End);
		Assert.Equal(22U, p4.Entries[4].End);
		Assert.Equal(52U, p4.Entries[5].End);

		// File 3
		Assert.Equal(3U, p4.Entries[6].End);
		Assert.Equal(13U, p4.Entries[7].End);
		Assert.Equal(23U, p4.Entries[8].End);
	}

	[Fact]
	public void Build_WriteableEntryTable_FlagsSet()
	{
		var items = new NefsItemList(@"C:\archive.nefs");

		var item1Attributes = new NefsItemAttributes(
			v20IsZlib: true,
			v20IsAes: true,
			isDirectory: true,
			isDuplicated: true)
		{
			IsLastSibling = true
		};
		var item1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456));
		var item1 = new NefsItem(new NefsItemId(0), "file1", new NefsItemId(0), item1DataSource, TestHelpers.TestTransform, item1Attributes);
		items.Add(item1);

		var builder = new NefsHeaderBuilder200();
		var header = builder.Build(new NefsHeader200(), items, new NefsProgress());

		Assert.Equal(0x1F, header.WriteableEntryTable.Entries[0].Flags);
	}
}
