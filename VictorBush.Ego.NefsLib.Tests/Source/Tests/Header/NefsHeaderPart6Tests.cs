// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using Xunit;

    public class NefsHeaderPart6Tests
    {
        [Fact]
        public void NefsHeaderPart6_MultipleItems_EntriesPopulated()
        {
            var items = new NefsItemList(@"C:\archive.nefs");

            var file1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456, new List<UInt32> { 11, 12, 13 }));
            var file1UnknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = 1,
                Part6Unknown0x01 = 2,
                Part6Unknown0x02 = 3,
                Part6Unknown0x03 = 4,
                Part7Unknown0x00 = 5,
                Part7Unknown0x04 = 6,
            };
            var file1 = new NefsItem(new NefsItemId(0), "file1", "file1", new NefsItemId(0), NefsItemType.File, file1DataSource, file1UnknownData);
            items.Add(file1);

            var file2DataSource = new NefsItemListDataSource(items, 456, new NefsItemSize(789, new List<UInt32> { 14, 15, 16 }));
            var file2UnknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = 7,
                Part6Unknown0x01 = 8,
                Part6Unknown0x02 = 9,
                Part6Unknown0x03 = 10,
                Part7Unknown0x00 = 11,
                Part7Unknown0x04 = 12,
            };
            var file2 = new NefsItem(new NefsItemId(1), "file2", "file2", new NefsItemId(1), NefsItemType.File, file2DataSource, file2UnknownData);
            items.Add(file2);

            var dir1DataSource = new NefsEmptyDataSource();
            var dir1UnknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = 13,
                Part6Unknown0x01 = 14,
                Part6Unknown0x02 = 15,
                Part6Unknown0x03 = 16,
                Part7Unknown0x00 = 17,
                Part7Unknown0x04 = 18,
            };
            var dir1 = new NefsItem(new NefsItemId(2), "dir1", "dir1", new NefsItemId(2), NefsItemType.Directory, dir1DataSource, dir1UnknownData);
            items.Add(dir1);

            var p6 = new NefsHeaderPart6(items);

            Assert.Equal(3, p6.Entries.Count);

            /*
            file1
            */

            Assert.Equal(1, p6.Entries[0].Byte0.Value[0]);
            Assert.Equal(2, p6.Entries[0].Byte1.Value[0]);
            Assert.Equal(3, p6.Entries[0].Byte2.Value[0]);
            Assert.Equal(4, p6.Entries[0].Byte3.Value[0]);

            /*
            file2
            */

            Assert.Equal(7, p6.Entries[1].Byte0.Value[0]);
            Assert.Equal(8, p6.Entries[1].Byte1.Value[0]);
            Assert.Equal(9, p6.Entries[1].Byte2.Value[0]);
            Assert.Equal(10, p6.Entries[1].Byte3.Value[0]);

            /*
            dir1
            */

            Assert.Equal(13, p6.Entries[2].Byte0.Value[0]);
            Assert.Equal(14, p6.Entries[2].Byte1.Value[0]);
            Assert.Equal(15, p6.Entries[2].Byte2.Value[0]);
            Assert.Equal(16, p6.Entries[2].Byte3.Value[0]);
        }

        [Fact]
        public void NefsHeaderPart6_NoItems_EntriesEmpty()
        {
            var items = new NefsItemList(@"C:\archive.nefs");
            var p6 = new NefsHeaderPart6(items);
            Assert.Empty(p6.Entries);
        }
    }
}
