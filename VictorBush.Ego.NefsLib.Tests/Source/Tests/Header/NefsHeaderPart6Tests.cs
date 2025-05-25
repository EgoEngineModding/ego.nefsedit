// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Item;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.Header;

public class NefsHeaderPart6Tests
{
	[Fact]
	public void Nefs16HeaderFlags_FlagsSet()
	{
		var items = new NefsItemList(@"C:\archive.nefs");

		var item1Attributes = new NefsItemAttributes(
			v16IsTransformed: true,
			isDirectory: true,
			isDuplicated: true,
			isCacheable: true,
			v16Unknown0x10: true,
			isPatched: true,
			v16Unknown0x40: true,
			v16Unknown0x80: true);
		var item1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456));
		var item1 = new NefsItem(new NefsItemId(0), "file1", new NefsItemId(0), item1DataSource, TestHelpers.TestTransform, item1Attributes);
		items.Add(item1);

		var p6 = new Nefs160HeaderWriteableEntryTable(items);

		Assert.Equal(0xFF, (byte)p6.EntriesByIndex[0].Flags);
	}

	[Fact]
	public void Nefs20HeaderFlags_FlagsSet()
	{
		var items = new NefsItemList(@"C:\archive.nefs");

		var item1Attributes = new NefsItemAttributes(
			v20IsZlib: true,
			v20IsAes: true,
			isDirectory: true,
			isDuplicated: true,
			v20Unknown0x10: true,
			v20Unknown0x20: true,
			v20Unknown0x40: true,
			v20Unknown0x80: true);
		var item1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456));
		var item1 = new NefsItem(new NefsItemId(0), "file1", new NefsItemId(0), item1DataSource, TestHelpers.TestTransform, item1Attributes);
		items.Add(item1);

		var p6 = new Nefs20HeaderPart6(items);

		Assert.Equal(0xFF, (byte)p6.EntriesByIndex[0].Flags);
	}

	[Fact]
	public void NefsHeaderPart6_MultipleItems_EntriesPopulated()
	{
		var items = new NefsItemList(@"C:\archive.nefs");

		var file1Attributes = new NefsItemAttributes(
			part6Volume: 12);
		var file1Chunks = NefsDataChunk.CreateChunkList(new List<uint> { 11, 12, 13 }, TestHelpers.TestTransform);
		var file1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456, file1Chunks));
		var file1 = new NefsItem(new NefsItemId(0), "file1", new NefsItemId(0), file1DataSource, TestHelpers.TestTransform, file1Attributes);
		items.Add(file1);

		var file2Attributes = new NefsItemAttributes(
			part6Volume: 6);
		var file2Chunks = NefsDataChunk.CreateChunkList(new List<uint> { 14, 15, 16 }, TestHelpers.TestTransform);
		var file2DataSource = new NefsItemListDataSource(items, 456, new NefsItemSize(789, file2Chunks));
		var file2 = new NefsItem(new NefsItemId(1), "file2", new NefsItemId(1), file2DataSource, TestHelpers.TestTransform, file2Attributes);
		items.Add(file2);

		var dir1Attributes = new NefsItemAttributes(
			isDirectory: true,
			part6Volume: 1);
		var dir1DataSource = new NefsEmptyDataSource();
		var dir1 = new NefsItem(new NefsItemId(2), "dir1", new NefsItemId(2), dir1DataSource, null, dir1Attributes);
		items.Add(dir1);

		var p6 = new Nefs20HeaderPart6(items);

		Assert.Equal(3, p6.EntriesByGuid.Count);
		Assert.Equal(3, p6.EntriesByIndex.Count);

		/*
		file1
		*/

		Assert.Equal(file1Attributes.Part6Volume, p6.EntriesByGuid[file1.Guid].Volume);

		/*
		file2
		*/

		Assert.Equal(file2Attributes.Part6Volume, p6.EntriesByGuid[file2.Guid].Volume);

		/*
		dir1
		*/

		var attributes = p6.EntriesByGuid[dir1.Guid].CreateAttributes();
		Assert.Equal(dir1Attributes.Part6Volume, p6.EntriesByGuid[dir1.Guid].Volume);
		Assert.True(attributes.IsDirectory);
	}

	[Fact]
	public void NefsHeaderPart6_NoItems_EntriesEmpty()
	{
		var items = new NefsItemList(@"C:\archive.nefs");
		var p6 = new Nefs20HeaderPart6(items);
		Assert.Empty(p6.EntriesByIndex);
		Assert.Empty(p6.EntriesByGuid);
	}
}
