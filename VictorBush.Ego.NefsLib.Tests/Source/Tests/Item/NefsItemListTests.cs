// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Tests.Item
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Tests.TestArchives;
    using Xunit;

    public class NefsItemListTests
    {
        [Fact]
        public void Add_ItemHasParent_ItemAdded()
        {
            var list = new NefsItemList(@"C:\data.nefs");
            var dir1 = TestHelpers.CreateItem(0, 0, "dir1", 0, 0, new List<UInt32>(), NefsItemType.Directory);
            var file1 = TestHelpers.CreateItem(1, 0, "file1.txt", 100, 200, new List<UInt32> { 200 }, NefsItemType.File);
            list.Add(dir1);
            list.Add(file1);

            Assert.Equal(2, list.Count);
            Assert.Equal("file1.txt", list.GetItem(file1.Id).FileName);
            Assert.Equal("dir1", list.GetItem(dir1.Id).FileName);
            Assert.Equal("dir1\\file1.txt", list.GetItemFilePath(file1.Id));
        }

        [Fact]
        public void Add_ItemIsInRoot_ItemAdded()
        {
            var list = new NefsItemList(@"C:\data.nefs");
            var item = TestHelpers.CreateItem(0, 0, "file1.txt", 100, 200, new List<UInt32> { 200 }, NefsItemType.File);
            list.Add(item);

            Assert.Equal(1, list.Count);
            Assert.Equal("file1.txt", list.EnumerateDepthFirst().First().FileName);
        }

        [Fact]
        public void Add_ParentNotInList_ArgumentExceptionThrown()
        {
            var list = new NefsItemList(@"C:\data.nefs");
            var item = TestHelpers.CreateItem(0, 1, "file1.txt", 100, 200, new List<UInt32> { 200 }, NefsItemType.File);
            Assert.Throws<ArgumentException>(() => list.Add(item));
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void EnumerateById_ItemsOrderedById()
        {
            // Purposely skip id numbers
            var file0 = TestHelpers.CreateItem(0, 0, "file0", 100, 200, new List<UInt32> { 200 }, NefsItemType.File);
            var file1 = TestHelpers.CreateItem(3, 3, "file1", 100, 200, new List<UInt32> { 200 }, NefsItemType.File);
            var dir0 = TestHelpers.CreateItem(5, 5, "dir0", 0, 0, new List<UInt32>(), NefsItemType.Directory);
            var dir0File0 = TestHelpers.CreateItem(7, 5, "dir0File0", 100, 200, new List<UInt32> { 200 }, NefsItemType.File);

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
        public void EnumerateDepthFirst_ItemsOrderedCorrectly()
        {
            // Purposely skip id numbers
            var file0 = TestHelpers.CreateItem(0, 0, "file0", 100, 200, new List<UInt32> { 200 }, NefsItemType.File);
            var file1 = TestHelpers.CreateItem(3, 3, "file1", 100, 200, new List<UInt32> { 200 }, NefsItemType.File);
            var dir0 = TestHelpers.CreateItem(5, 5, "dir0", 0, 0, new List<UInt32>(), NefsItemType.Directory);
            var dir0File0 = TestHelpers.CreateItem(7, 5, "dir0File0", 100, 200, new List<UInt32> { 200 }, NefsItemType.File);

            var list = new NefsItemList(@"C:\data.nefs");
            list.Add(file1);
            list.Add(file0);
            list.Add(dir0);
            list.Add(dir0File0);

            var result = list.EnumerateDepthFirst();
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
    }
}
