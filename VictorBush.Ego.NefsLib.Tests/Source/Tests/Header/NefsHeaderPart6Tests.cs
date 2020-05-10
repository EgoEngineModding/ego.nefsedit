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

            var file1Chunks = NefsDataChunk.CreateChunkList(new List<UInt32> { 11, 12, 13 }, TestHelpers.TestTransform);
            var file1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456, file1Chunks));
            var file1UnknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = 1,
                Part6Unknown0x01 = 2,
                Part6Unknown0x02 = 3,
                Part6Unknown0x03 = 4,
            };
            var file1 = new NefsItem(Guid.NewGuid(), new NefsItemId(0), "file1", new NefsItemId(0), NefsItemType.File, file1DataSource, TestHelpers.TestTransform, file1UnknownData);
            items.Add(file1);

            var file2Chunks = NefsDataChunk.CreateChunkList(new List<UInt32> { 14, 15, 16 }, TestHelpers.TestTransform);
            var file2DataSource = new NefsItemListDataSource(items, 456, new NefsItemSize(789, file2Chunks));
            var file2UnknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = 7,
                Part6Unknown0x01 = 8,
                Part6Unknown0x02 = 9,
                Part6Unknown0x03 = 10,
            };
            var file2 = new NefsItem(Guid.NewGuid(), new NefsItemId(1), "file2", new NefsItemId(1), NefsItemType.File, file2DataSource, TestHelpers.TestTransform, file2UnknownData);
            items.Add(file2);

            var dir1DataSource = new NefsEmptyDataSource();
            var dir1UnknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = 13,
                Part6Unknown0x01 = 14,
                Part6Unknown0x02 = 15,
                Part6Unknown0x03 = 16,
            };
            var dir1 = new NefsItem(Guid.NewGuid(), new NefsItemId(2), "dir1", new NefsItemId(2), NefsItemType.Directory, dir1DataSource, null, dir1UnknownData);
            items.Add(dir1);

            var p6 = new Nefs20HeaderPart6(items);

            Assert.Equal(3, p6.EntriesByGuid.Count);
            Assert.Equal(3, p6.EntriesByIndex.Count);

            /*
            file1
            */

            Assert.Equal(1, p6.EntriesByGuid[file1.Guid].Byte0);
            Assert.Equal(2, p6.EntriesByGuid[file1.Guid].Byte1);
            Assert.Equal(3, p6.EntriesByGuid[file1.Guid].Byte2);
            Assert.Equal(4, p6.EntriesByGuid[file1.Guid].Byte3);

            /*
            file2
            */

            Assert.Equal(7, p6.EntriesByGuid[file2.Guid].Byte0);
            Assert.Equal(8, p6.EntriesByGuid[file2.Guid].Byte1);
            Assert.Equal(9, p6.EntriesByGuid[file2.Guid].Byte2);
            Assert.Equal(10, p6.EntriesByGuid[file2.Guid].Byte3);

            /*
            dir1
            */

            Assert.Equal(13, p6.EntriesByGuid[dir1.Guid].Byte0);
            Assert.Equal(14, p6.EntriesByGuid[dir1.Guid].Byte1);
            Assert.Equal(15, p6.EntriesByGuid[dir1.Guid].Byte2);
            Assert.Equal(16, p6.EntriesByGuid[dir1.Guid].Byte3);
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
}
