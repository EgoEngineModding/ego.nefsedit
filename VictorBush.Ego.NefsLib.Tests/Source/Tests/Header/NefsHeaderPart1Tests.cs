// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using Xunit;

    public class NefsHeaderPart1Tests
    {
        [Fact]
        public void NefsHeaderPart1_MultipleItems_EntriesPopulated()
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

            var p4 = new NefsHeaderPart4(items);
            var p1 = new NefsHeaderPart1(items, p4);

            Assert.Equal(3, p1.EntriesById.Count);

            /*
            file1
            */

            Assert.Equal(0, (int)p1.EntriesById[file1.Id].Id.Value);
            Assert.Equal(123, (int)p1.EntriesById[file1.Id].OffsetToData);
            Assert.Equal(0, (int)p1.EntriesById[file1.Id].MetadataIndex);
            Assert.Equal(0, (int)p1.EntriesById[file1.Id].IndexIntoPart4);

            /*
            file2
            */

            Assert.Equal(1, (int)p1.EntriesById[file2.Id].Id.Value);
            Assert.Equal(456, (int)p1.EntriesById[file2.Id].OffsetToData);
            Assert.Equal(1, (int)p1.EntriesById[file2.Id].MetadataIndex);

            // There are 3 chunks for file1, so file2's chunks start right after that (hence p4
            // index == 3)
            Assert.Equal(3, (int)p1.EntriesById[file2.Id].IndexIntoPart4);

            /*
            dir1
            */

            // Offset to data and index to p4 are both 0 since this is a directory
            Assert.Equal(2, (int)p1.EntriesById[dir1.Id].Id.Value);
            Assert.Equal(0, (int)p1.EntriesById[dir1.Id].OffsetToData);
            Assert.Equal(2, (int)p1.EntriesById[dir1.Id].MetadataIndex);
            Assert.Equal(0, (int)p1.EntriesById[dir1.Id].IndexIntoPart4);
        }

        [Fact]
        public void NefsHeaderPart1_NoItems_EntriesEmpty()
        {
            var items = new NefsItemList(@"C:\archive.nefs");
            var p4 = new NefsHeaderPart4(items);
            var p1 = new NefsHeaderPart1(items, p4);
            Assert.Empty(p1.EntriesById);
        }
    }
}
