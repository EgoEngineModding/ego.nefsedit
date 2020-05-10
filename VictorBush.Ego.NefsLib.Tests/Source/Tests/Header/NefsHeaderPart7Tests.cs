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
            var file1 = TestHelpers.CreateFile(file1Id.Value, file1Id.Value, "file1", file1DataSource);
            items.Add(file1);

            var file2Id = new NefsItemId(41);
            var file2Chunks = NefsDataChunk.CreateChunkList(new List<UInt32> { 14, 15, 16 }, TestHelpers.TestTransform);
            var file2DataSource = new NefsItemListDataSource(items, 456, new NefsItemSize(789, file2Chunks));
            var file2 = TestHelpers.CreateFile(file2Id.Value, file2Id.Value, "file2", file2DataSource);
            items.Add(file2);

            var dir2Id = new NefsItemId(51);
            var dir1 = TestHelpers.CreateDirectory(dir2Id.Value, dir2Id.Value, "dir1");
            items.Add(dir1);

            var p7 = new NefsHeaderPart7(items);

            Assert.Equal(3, p7.EntriesByIndex.Count);

            // NOTES : Part 7 items are ordered in the same way that part 2 items are ordered (depth
            // first by name).

            /*
            dir1
            */

            Assert.Equal(51U, p7.EntriesByIndex[0].Id.Value);
            Assert.Equal(51U, p7.EntriesByIndex[0].SiblingId.Value);

            /*
            file1
            */

            Assert.Equal(31U, p7.EntriesByIndex[1].Id.Value);
            Assert.Equal(41U, p7.EntriesByIndex[1].SiblingId.Value);

            /*
            file2
            */

            Assert.Equal(41U, p7.EntriesByIndex[2].Id.Value);
            Assert.Equal(51U, p7.EntriesByIndex[2].SiblingId.Value);
        }

        [Fact]
        public void NefsHeaderPart7_NoItems_EntriesEmpty()
        {
            var items = new NefsItemList(@"C:\archive.nefs");
            var p7 = new NefsHeaderPart7(items);
            Assert.Empty(p7.EntriesByIndex);
        }
    }
}
