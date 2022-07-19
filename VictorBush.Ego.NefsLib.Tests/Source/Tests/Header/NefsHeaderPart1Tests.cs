// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Item;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.Header;

public class NefsHeaderPart1Tests
{
	[Fact]
	public void NefsHeaderPart1_MultipleItems_EntriesPopulated()
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

		var p4 = new Nefs20HeaderPart4(items);
		var p1 = new NefsHeaderPart1(items, p4);

		Assert.Equal(3, p1.EntriesByGuid.Count);
		Assert.Equal(3, p1.EntriesByIndex.Count);

		/*
		dir1
		*/

		// Offset to data and index to p4 are both 0 since this is a directory
		Assert.Equal(2, (int)p1.EntriesByGuid[dir1.Guid].Id.Value);
		Assert.Equal(0, (int)p1.EntriesByGuid[dir1.Guid].OffsetToData);
		Assert.Equal(0, (int)p1.EntriesByGuid[dir1.Guid].IndexPart2);
		Assert.Equal(0, (int)p1.EntriesByGuid[dir1.Guid].IndexPart4);

		/*
		file1
		*/

		Assert.Equal(0, (int)p1.EntriesByGuid[file1.Guid].Id.Value);
		Assert.Equal(123, (int)p1.EntriesByGuid[file1.Guid].OffsetToData);
		Assert.Equal(1, (int)p1.EntriesByGuid[file1.Guid].IndexPart2);
		Assert.Equal(0, (int)p1.EntriesByGuid[file1.Guid].IndexPart4);

		/*
		file2
		*/

		Assert.Equal(1, (int)p1.EntriesByGuid[file2.Guid].Id.Value);
		Assert.Equal(456, (int)p1.EntriesByGuid[file2.Guid].OffsetToData);
		Assert.Equal(2, (int)p1.EntriesByGuid[file2.Guid].IndexPart2);

		// There are 3 chunks for file1, so file2's chunks start right after that (hence p4 index == 3)
		Assert.Equal(3, (int)p1.EntriesByGuid[file2.Guid].IndexPart4);
	}

	[Fact]
	public void NefsHeaderPart1_NoItems_EntriesEmpty()
	{
		var items = new NefsItemList(@"C:\archive.nefs");
		var p4 = new Nefs20HeaderPart4(items);
		var p1 = new NefsHeaderPart1(items, p4);
		Assert.Empty(p1.EntriesByIndex);
		Assert.Empty(p1.EntriesByGuid);
	}
}
