// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Tests.TestArchives;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.Item;

public class NefsItemListTests
{
	[Fact]
	public void Add_ItemHasParent_ItemAdded()
	{
		var list = new NefsItemList(@"C:\data.nefs");
		var dir1 = TestHelpers.CreateItem(0, 0, "dir1", 0, 0, new List<uint>(), NefsItemType.Directory);
		var file1 = TestHelpers.CreateItem(1, 0, "file1.txt", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		list.Add(dir1);
		list.Add(file1);

		Assert.Equal(2, list.Count);
		Assert.Equal("file1.txt", list.GetItem(file1.Guid).FileName);
		Assert.Equal("dir1", list.GetItem(dir1.Guid).FileName);
		Assert.Equal("dir1\\file1.txt", list.GetItemFilePath(file1.Id));
	}

	[Fact]
	public void Add_ItemIsInRoot_ItemAdded()
	{
		var list = new NefsItemList(@"C:\data.nefs");
		var item = TestHelpers.CreateItem(0, 0, "file1.txt", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		list.Add(item);

		Assert.Equal(1, list.Count);
		Assert.Equal("file1.txt", list.EnumerateDepthFirstByName().First().FileName);
	}

	[Fact]
	public void Add_ParentNotInList_ArgumentExceptionThrown()
	{
		var list = new NefsItemList(@"C:\data.nefs");
		var item = TestHelpers.CreateItem(0, 1, "file1.txt", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		Assert.Throws<ArgumentException>(() => list.Add(item));
		Assert.Equal(0, list.Count);
	}

	[Fact]
	public void EnumerateById_ItemsOrderedById()
	{
		// Purposely skip id numbers
		var file0 = TestHelpers.CreateItem(0, 0, "file0", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		var file1 = TestHelpers.CreateItem(3, 3, "file1", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		var dir0 = TestHelpers.CreateItem(5, 5, "dir0", 0, 0, new List<uint>(), NefsItemType.Directory);
		var dir0File0 = TestHelpers.CreateItem(7, 5, "dir0File0", 100, 200, new List<uint> { 200 }, NefsItemType.File);

		var list = new NefsItemList(@"C:\data.nefs");
		list.Add(dir0);
		list.Add(dir0File0);
		list.Add(file1);
		list.Add(file0);

		var result = list.EnumerateById();
		Assert.Equal(4, result.Count());
		Assert.Same(file0, result.ElementAt(0));
		Assert.Same(file1, result.ElementAt(1));
		Assert.Same(dir0, result.ElementAt(2));
		Assert.Same(dir0File0, result.ElementAt(3));
	}

	[Fact]
	public void EnumerateDepthFirstById_ItemsOrderedCorrectly()
	{
		// Purposely skip id numbers
		var fileA = TestHelpers.CreateItem(0, 0, "fileA", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		var fileB = TestHelpers.CreateItem(3, 3, "fileB", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		var dir0 = TestHelpers.CreateItem(5, 5, "dir0", 0, 0, new List<uint>(), NefsItemType.Directory);
		var dir0File0 = TestHelpers.CreateItem(7, 5, "dir0File0", 100, 200, new List<uint> { 200 }, NefsItemType.File);

		var list = new NefsItemList(@"C:\data.nefs");
		list.Add(fileB);
		list.Add(fileA);
		list.Add(dir0);
		list.Add(dir0File0);

		var result = list.EnumerateDepthFirstById();
		Assert.Equal(4, result.Count());
		Assert.Same(fileA, result.ElementAt(0));
		Assert.Same(fileB, result.ElementAt(1));
		Assert.Same(dir0, result.ElementAt(2));
		Assert.Same(dir0File0, result.ElementAt(3));
	}

	[Fact]
	public void EnumerateDepthFirstByName_ItemsOrderedCorrectly()
	{
		// Purposely skip id numbers
		var file0 = TestHelpers.CreateItem(0, 0, "file0", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		var file1 = TestHelpers.CreateItem(3, 3, "file1", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		var dir0 = TestHelpers.CreateItem(5, 5, "dir0", 0, 0, new List<uint>(), NefsItemType.Directory);
		var dir0File0 = TestHelpers.CreateItem(7, 5, "dir0File0", 100, 200, new List<uint> { 200 }, NefsItemType.File);

		var list = new NefsItemList(@"C:\data.nefs");
		list.Add(file1);
		list.Add(file0);
		list.Add(dir0);
		list.Add(dir0File0);

		var result = list.EnumerateDepthFirstByName();
		Assert.Equal(4, result.Count());
		Assert.Same(dir0, result.ElementAt(0));
		Assert.Same(dir0File0, result.ElementAt(1));
		Assert.Same(file0, result.ElementAt(2));
		Assert.Same(file1, result.ElementAt(3));
	}

	[Fact]
	public void GetItemFilePath_DirectoryInRoot_PathReturned()
	{
		var archive = TestArchiveNotModified.Create(@"C:\archive.nefs");
		var path = archive.Items.GetItemFilePath(new NefsItemId(TestArchiveNotModified.Dir1ItemId));
		Assert.Equal("dir1", path);
	}

	[Fact]
	public void GetItemFilePath_FileInDir_PathReturned()
	{
		var archive = TestArchiveNotModified.Create(@"C:\archive.nefs");
		var path = archive.Items.GetItemFilePath(new NefsItemId(TestArchiveNotModified.File2ItemId));
		Assert.Equal(@"dir1\file2.txt", path);
	}

	[Fact]
	public void GetItemFilePath_FileInRoot_PathReturned()
	{
		var archive = TestArchiveNotModified.Create(@"C:\archive.nefs");
		var path = archive.Items.GetItemFilePath(new NefsItemId(TestArchiveNotModified.File1ItemId));
		Assert.Equal("file1.txt", path);
	}

	[Fact]
	public void GetItemFirstChildId_GotIt()
	{
		var file0 = TestHelpers.CreateItem(0, 0, "file0", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		var dir0 = TestHelpers.CreateItem(5, 5, "dir0", 0, 0, new List<uint>(), NefsItemType.Directory);
		var dir0FileB = TestHelpers.CreateItem(7, 5, "dir0FileB", 100, 200, new List<uint> { 200 }, NefsItemType.File);
		var dir0FileA = TestHelpers.CreateItem(9, 5, "dir0FileA", 100, 200, new List<uint> { 200 }, NefsItemType.File);

		var list = new NefsItemList(@"C:\data.nefs");
		list.Add(file0);
		list.Add(dir0);
		list.Add(dir0FileB);
		list.Add(dir0FileA);

		Assert.Equal(0U, list.GetItemFirstChildId(file0.Id).Value);
		Assert.Equal(7U, list.GetItemFirstChildId(dir0.Id).Value);
		Assert.Equal(7U, list.GetItemFirstChildId(dir0FileB.Id).Value);
		Assert.Equal(9U, list.GetItemFirstChildId(dir0FileA.Id).Value);
	}
}
