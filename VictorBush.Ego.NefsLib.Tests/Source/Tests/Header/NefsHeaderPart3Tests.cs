// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using Xunit;

    public class NefsHeaderPart3Tests
    {
        [Fact]
        public void NefsHeaderPart3_MultipleItems_EntriesPopulated()
        {
            var items = new NefsItemList(@"C:\archive.nefs");

            var file1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456, new List<UInt32> { 11, 12, 13 }));
            var file1 = new NefsItem(new NefsItemId(0), "file1", new NefsItemId(0), NefsItemType.File, file1DataSource, TestHelpers.CreateUnknownData());
            items.Add(file1);

            var file2DataSource = new NefsItemListDataSource(items, 456, new NefsItemSize(789, new List<UInt32> { 14, 15, 16 }));
            var file2 = new NefsItem(new NefsItemId(1), "file2", new NefsItemId(1), NefsItemType.File, file2DataSource, TestHelpers.CreateUnknownData());
            items.Add(file2);

            var dir1DataSource = new NefsEmptyDataSource();
            var dir1 = new NefsItem(new NefsItemId(2), "dir1", new NefsItemId(2), NefsItemType.Directory, dir1DataSource, TestHelpers.CreateUnknownData());
            items.Add(dir1);

            var file3DataSource = new NefsItemListDataSource(items, 222, new NefsItemSize(333, new List<UInt32> { 22, 23, 24 }));
            var file3 = new NefsItem(new NefsItemId(3), "file3", dir1.Id, NefsItemType.File, file3DataSource, TestHelpers.CreateUnknownData());
            items.Add(file3);

            var p3 = new NefsHeaderPart3(items);

            Assert.Equal(4, p3.OffsetsByFileName.Count);
            Assert.Equal(4, p3.FileNamesByOffset.Count);

            // Four file names plus a null terminal for each.
            Assert.Equal(23, (int)p3.Size);

            // Strings table is sorted alphabetically
            Assert.Equal("dir1", p3.FileNamesByOffset[0]);
            Assert.Equal("file1", p3.FileNamesByOffset[5]);
            Assert.Equal("file2", p3.FileNamesByOffset[11]);
            Assert.Equal("file3", p3.FileNamesByOffset[17]);

            Assert.Equal(5, (int)p3.OffsetsByFileName[file1.FileName]);
            Assert.Equal(11, (int)p3.OffsetsByFileName[file2.FileName]);
            Assert.Equal(0, (int)p3.OffsetsByFileName[dir1.FileName]);
            Assert.Equal(17, (int)p3.OffsetsByFileName[file3.FileName]);
        }

        [Fact]
        public void NefsHeaderPart3_NoItems_EntriesEmpty()
        {
            var items = new NefsItemList(@"C:\archive.nefs");
            var p3 = new NefsHeaderPart3(items);
            Assert.Empty(p3.OffsetsByFileName);
            Assert.Empty(p3.FileNamesByOffset);
            Assert.Equal(0, (int)p3.Size);
        }
    }
}
