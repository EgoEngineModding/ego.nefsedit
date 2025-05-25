// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Item;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.Header;

public class NefsHeaderPart4Tests
{
	private readonly NefsItem dir1;

	private readonly NefsItem file1;

	private readonly NefsItem file2;

	private readonly NefsItem file3;

	private readonly NefsItem file4NotCompressed;

	private readonly NefsItemList testItems;

	public NefsHeaderPart4Tests()
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
	public void CreateChunksList_FirstItem_ListReturned()
	{
		var p4 = new Nefs200HeaderBlockTable(this.testItems);
		var expected = this.file1.DataSource.Size.Chunks;
		var chunks = p4.CreateChunksList(0, (uint)expected.Count, TestHelpers.TestTransform);

		Assert.Equal(expected.Count, chunks.Count);
		Assert.Equal(expected[0].CumulativeSize, chunks[0].CumulativeSize);
		Assert.Equal(expected[1].CumulativeSize, chunks[1].CumulativeSize);
		Assert.Equal(expected[2].CumulativeSize, chunks[2].CumulativeSize);

		Assert.Same(TestHelpers.TestTransform, chunks[0].Transform);
		Assert.Same(TestHelpers.TestTransform, chunks[1].Transform);
		Assert.Same(TestHelpers.TestTransform, chunks[2].Transform);
	}

	[Fact]
	public void CreateChunksList_SecondItem_ListReturned()
	{
		var p4 = new Nefs200HeaderBlockTable(this.testItems);
		var expected = this.file2.DataSource.Size.Chunks;
		var chunks = p4.CreateChunksList(3, (uint)expected.Count, TestHelpers.TestTransform);

		Assert.Equal(expected.Count, chunks.Count);
		Assert.Equal(expected[0].CumulativeSize, chunks[0].CumulativeSize);
		Assert.Equal(expected[1].CumulativeSize, chunks[1].CumulativeSize);
		Assert.Equal(expected[2].CumulativeSize, chunks[2].CumulativeSize);

		Assert.Same(TestHelpers.TestTransform, chunks[0].Transform);
		Assert.Same(TestHelpers.TestTransform, chunks[1].Transform);
		Assert.Same(TestHelpers.TestTransform, chunks[2].Transform);
	}

	[Fact]
	public void GetIndexForItem_ItemIsDirectory_ZeroReturned()
	{
		var p4 = new Nefs200HeaderBlockTable(this.testItems);
		Assert.Equal(0U, p4.GetIndexForItem(this.dir1));
	}

	[Fact]
	public void GetIndexForItem_ItemIsUncompressed_NegativeOneReturned()
	{
		var p4 = new Nefs200HeaderBlockTable(this.testItems);
		Assert.Equal(0xFFFFFFFFU, p4.GetIndexForItem(this.file4NotCompressed));
	}

	[Fact]
	public void GetIndexForItem_ItemIsValue_IndexReturned()
	{
		var p4 = new Nefs200HeaderBlockTable(this.testItems);
		Assert.Equal(0U, p4.GetIndexForItem(this.file1));
		Assert.Equal(3U, p4.GetIndexForItem(this.file2));
		Assert.Equal(6U, p4.GetIndexForItem(this.file3));
	}

	[Fact]
	public void NefsHeaderPart4_MultipleItems_EntriesPopulated()
	{
		var p4 = new Nefs200HeaderBlockTable(this.testItems);

		// Dir 1 and file 4 should not have entries. Only compressed files have entries in part 4.
		Assert.Equal(9, p4.EntriesByIndex.Count);

		// Total size is (total number of chunks * bytes)
		Assert.Equal(36, p4.Size);

		// File 1
		Assert.Equal(1U, p4.EntriesByIndex[0].CumulativeChunkSize);
		Assert.Equal(11U, p4.EntriesByIndex[1].CumulativeChunkSize);
		Assert.Equal(21U, p4.EntriesByIndex[2].CumulativeChunkSize);

		// File 2
		Assert.Equal(2U, p4.EntriesByIndex[3].CumulativeChunkSize);
		Assert.Equal(22U, p4.EntriesByIndex[4].CumulativeChunkSize);
		Assert.Equal(52U, p4.EntriesByIndex[5].CumulativeChunkSize);

		// File 3
		Assert.Equal(3U, p4.EntriesByIndex[6].CumulativeChunkSize);
		Assert.Equal(13U, p4.EntriesByIndex[7].CumulativeChunkSize);
		Assert.Equal(23U, p4.EntriesByIndex[8].CumulativeChunkSize);
	}

	[Fact]
	public void NefsHeaderPart4_NoItems_EntriesEmpty()
	{
		var items = new NefsItemList(@"C:\archive.nefs");
		var p4 = new Nefs200HeaderBlockTable(items);
		Assert.Empty(p4.EntriesByIndex);
		Assert.Equal(0, p4.Size);
	}
}
