// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using Xunit;

    public class NefsHeaderPart2Tests
    {
        [Fact]
        public void NefsHeaderPart2_MultipleItems_EntriesPopulated()
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
            var p2 = new NefsHeaderPart2(items, p3);

            Assert.Equal(4, p2.EntriesById.Count);

            // NOTE: Part 3 is the strings table. So offset into p3 must take into account null
            // terminated file/dir names. Also note strings table is alphabetized. Also note the
            // data file name is added to the strings table.

            /*
            file1
            */

            Assert.Equal(0, (int)p2.EntriesById[file1.Id].Id.Value);
            Assert.Equal(0, (int)p2.EntriesById[file1.Id].DirectoryId.Value);
            Assert.Equal(0, (int)p2.EntriesById[file1.Id].FirstChildId.Value);
            Assert.Equal(456, (int)p2.EntriesById[file1.Id].ExtractedSize);
            Assert.Equal(18, (int)p2.EntriesById[file1.Id].OffsetIntoPart3);

            /*
            file2
            */

            Assert.Equal(1, (int)p2.EntriesById[file2.Id].Id.Value);
            Assert.Equal(1, (int)p2.EntriesById[file2.Id].DirectoryId.Value);
            Assert.Equal(1, (int)p2.EntriesById[file2.Id].FirstChildId.Value);
            Assert.Equal(789, (int)p2.EntriesById[file2.Id].ExtractedSize);
            Assert.Equal(24, (int)p2.EntriesById[file2.Id].OffsetIntoPart3);

            /*
            dir1
            */

            Assert.Equal(2, (int)p2.EntriesById[dir1.Id].Id.Value);
            Assert.Equal(2, (int)p2.EntriesById[dir1.Id].Data0x00_DirectoryId.Value);
            Assert.Equal(3, (int)p2.EntriesById[dir1.Id].Data0x04_FirstChildId.Value);
            Assert.Equal(0, (int)p2.EntriesById[dir1.Id].Data0x0c_ExtractedSize.Value);
            Assert.Equal(13, (int)p2.EntriesById[dir1.Id].Data0x08_OffsetIntoPart3.Value);

            /*
            file3
            */

            Assert.Equal(3, (int)p2.EntriesById[file3.Id].Id.Value);
            Assert.Equal(2, (int)p2.EntriesById[file3.Id].Data0x00_DirectoryId.Value);
            Assert.Equal(3, (int)p2.EntriesById[file3.Id].Data0x04_FirstChildId.Value);
            Assert.Equal(333, (int)p2.EntriesById[file3.Id].Data0x0c_ExtractedSize.Value);
            Assert.Equal(30, (int)p2.EntriesById[file3.Id].Data0x08_OffsetIntoPart3.Value);
        }

        [Fact]
        public void NefsHeaderPart2_NoItems_EntriesEmpty()
        {
            var items = new NefsItemList(@"C:\archive.nefs");
            var p3 = new NefsHeaderPart3(items);
            var p2 = new NefsHeaderPart2(items, p3);
            Assert.Empty(p2.EntriesById);
        }
    }
}
