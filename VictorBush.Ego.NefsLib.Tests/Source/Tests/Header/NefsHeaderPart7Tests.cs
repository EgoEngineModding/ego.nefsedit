// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using Xunit;

    public class NefsHeaderPart7Tests
    {
        [Fact]
        public void NefsHeaderPart7_MultipleItems_EntriesPopulated()
        {
            var items = new NefsItemList(@"C:\archive.nefs");

            var file1Id = new NefsItemId(31);
            var file1Chunks = NefsDataChunk.CreateChunkList(new List<UInt32> { 11, 12, 13 }, TestHelpers.TestTransform);
            var file1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456, file1Chunks));
            var file1UnknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = 1,
                Part6Unknown0x01 = 2,
                Part6Unknown0x02 = 3,
                Part6Unknown0x03 = 4,
            };

            var file1 = new NefsItem(file1Id, "file1", file1Id, NefsItemType.File, file1DataSource, TestHelpers.TestTransform, file1UnknownData);
            items.Add(file1);

            var file2Id = new NefsItemId(41);
            var file2Chunks = NefsDataChunk.CreateChunkList(new List<UInt32> { 14, 15, 16 }, TestHelpers.TestTransform);
            var file2DataSource = new NefsItemListDataSource(items, 456, new NefsItemSize(789, file2Chunks));
            var file2UnknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = 7,
                Part6Unknown0x01 = 8,
                Part6Unknown0x02 = 9,
                Part6Unknown0x03 = 10,
            };
            var file2 = new NefsItem(file2Id, "file2", file2Id, NefsItemType.File, file2DataSource, TestHelpers.TestTransform, file2UnknownData);
            items.Add(file2);

            var dir2Id = new NefsItemId(51);
            var dir1DataSource = new NefsEmptyDataSource();
            var dir1UnknownData = new NefsItemUnknownData
            {
                Part6Unknown0x00 = 13,
                Part6Unknown0x01 = 14,
                Part6Unknown0x02 = 15,
                Part6Unknown0x03 = 16,
            };
            var dir1 = new NefsItem(dir2Id, "dir1", dir2Id, NefsItemType.Directory, dir1DataSource, null, dir1UnknownData);
            items.Add(dir1);

            var p7 = new NefsHeaderPart7(items);

            Assert.Equal(3, p7.EntriesById.Count);

            /*
            file1
            */

            Assert.Equal(31U, p7.EntriesById[file1.Id].Id.Value);
            Assert.Equal(41U, p7.EntriesById[file1.Id].SiblingId.Value);

            /*
            file2
            */

            Assert.Equal(41U, p7.EntriesById[file2.Id].Id.Value);
            Assert.Equal(51U, p7.EntriesById[file2.Id].SiblingId.Value);

            /*
            dir1
            */

            Assert.Equal(51U, p7.EntriesById[dir1.Id].Id.Value);
            Assert.Equal(51U, p7.EntriesById[dir1.Id].SiblingId.Value);
        }

        [Fact]
        public void NefsHeaderPart7_NoItems_EntriesEmpty()
        {
            var items = new NefsItemList(@"C:\archive.nefs");
            var p7 = new NefsHeaderPart7(items);
            Assert.Empty(p7.EntriesById);
            Assert.Empty(p7.EntriesByIndex);
        }
    }
}
