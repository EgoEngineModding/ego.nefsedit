// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Header
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using Xunit;

    public class NefsHeaderPart4Tests
    {
        private readonly NefsItem dir1;

        private readonly NefsItem file1;

        private readonly NefsItem file2;

        private readonly NefsItem file3;

        private readonly NefsItem file4NotCompressed;

        private readonly NefsItemList testItems;

        public NefsHeaderPart4Tests()
        {
            var items = new NefsItemList(@"C:\archive.nefs");

            var file1DataSource = new NefsItemListDataSource(items, 123, new NefsItemSize(456, new List<UInt32> { 1, 11, 21 }));
            this.file1 = new NefsItem(new NefsItemId(0), "file1", "file1", new NefsItemId(0), NefsItemType.File, file1DataSource, TestHelpers.CreateUnknownData());
            items.Add(this.file1);

            var file2DataSource = new NefsItemListDataSource(items, 456, new NefsItemSize(789, new List<UInt32> { 2, 22, 52 }));
            this.file2 = new NefsItem(new NefsItemId(1), "file2", "file2", new NefsItemId(1), NefsItemType.File, file2DataSource, TestHelpers.CreateUnknownData());
            items.Add(this.file2);

            var dir1DataSource = new NefsEmptyDataSource();
            this.dir1 = new NefsItem(new NefsItemId(2), "dir1", "dir1", new NefsItemId(2), NefsItemType.Directory, dir1DataSource, TestHelpers.CreateUnknownData());
            items.Add(this.dir1);

            var file3DataSource = new NefsItemListDataSource(items, 222, new NefsItemSize(333, new List<UInt32> { 3, 13, 23 }));
            this.file3 = new NefsItem(new NefsItemId(3), "file3", "file3", this.dir1.Id, NefsItemType.File, file3DataSource, TestHelpers.CreateUnknownData());
            items.Add(this.file3);

            var file4DataSource = new NefsItemListDataSource(items, 777, new NefsItemSize(444));
            this.file4NotCompressed = new NefsItem(new NefsItemId(3), "file4", "file3", this.dir1.Id, NefsItemType.File, file4DataSource, TestHelpers.CreateUnknownData());
            items.Add(this.file4NotCompressed);

            this.testItems = items;
        }

        [Fact]
        public void GetChunkSizesForItem_ItemIsDirectory_EmptyList()
        {
            var p4 = new NefsHeaderPart4(this.testItems);
            var sizes = p4.GetChunkSizesForItem(this.dir1);
            Assert.Empty(sizes);
        }

        [Fact]
        public void GetChunkSizesForItem_ItemIsNotCompressed_EmptyList()
        {
            var p4 = new NefsHeaderPart4(this.testItems);
            var sizes = p4.GetChunkSizesForItem(this.file4NotCompressed);
            Assert.Empty(sizes);
        }

        [Fact]
        public void GetChunkSizesForItem_ItemValid_ChunkSizesReturned()
        {
            var p4 = new NefsHeaderPart4(this.testItems);

            var file1Sizes = p4.GetChunkSizesForItem(this.file1);
            Assert.Equal(3, file1Sizes.Count);
            Assert.NotSame(this.file1.DataSource.Size.ChunkSizes, file1Sizes);
            Assert.True(file1Sizes.SequenceEqual(this.file1.DataSource.Size.ChunkSizes));

            var file2Sizes = p4.GetChunkSizesForItem(this.file2);
            Assert.Equal(3, file2Sizes.Count);
            Assert.NotSame(this.file2.DataSource.Size.ChunkSizes, file2Sizes);
            Assert.True(file2Sizes.SequenceEqual(this.file2.DataSource.Size.ChunkSizes));

            var file3Sizes = p4.GetChunkSizesForItem(this.file3);
            Assert.Equal(3, file3Sizes.Count);
            Assert.NotSame(this.file3.DataSource.Size.ChunkSizes, file3Sizes);
            Assert.True(file3Sizes.SequenceEqual(this.file3.DataSource.Size.ChunkSizes));
        }

        [Fact]
        public void GetIndexForItem_ItemIsDirectory_ZeroReturned()
        {
            var p4 = new NefsHeaderPart4(this.testItems);
            Assert.Equal(0U, p4.GetIndexForItem(this.dir1));
        }

        [Fact]
        public void GetIndexForItem_ItemIsUncompressed_NegativeOneReturned()
        {
            var p4 = new NefsHeaderPart4(this.testItems);
            Assert.Equal(0xFFFFFFFFU, p4.GetIndexForItem(this.file4NotCompressed));
        }

        [Fact]
        public void GetIndexForItem_ItemIsValue_IndexReturned()
        {
            var p4 = new NefsHeaderPart4(this.testItems);
            Assert.Equal(0U, p4.GetIndexForItem(this.file1));
            Assert.Equal(3U, p4.GetIndexForItem(this.file2));
            Assert.Equal(6U, p4.GetIndexForItem(this.file3));
        }

        [Fact]
        public void NefsHeaderPart4_MultipleItems_EntriesPopulated()
        {
            var p4 = new NefsHeaderPart4(this.testItems);

            // Dir 1 and file 4 should not have entries. Only compressed files have entries in part 4.
            Assert.Equal(3, p4.EntriesByIndex.Count);

            // Last four bytes - potentially the largest compressed file size
            Assert.Equal(52U, p4.LastFourBytes);

            // Total size is (total number of chunks * bytes) + 4 bytes
            Assert.Equal(40U, p4.Size);

            // File 1
            var f1Idx = p4.GetIndexForItem(this.file1);
            Assert.Equal(1U, p4.EntriesByIndex[f1Idx].ChunkSizes[0]);
            Assert.Equal(11U, p4.EntriesByIndex[f1Idx].ChunkSizes[1]);
            Assert.Equal(21U, p4.EntriesByIndex[f1Idx].ChunkSizes[2]);
            Assert.Equal(3, p4.EntriesByIndex[f1Idx].ChunkSizes.Count);

            // File 2
            var f2Idx = p4.GetIndexForItem(this.file2);
            Assert.Equal(2U, p4.EntriesByIndex[f2Idx].ChunkSizes[0]);
            Assert.Equal(22U, p4.EntriesByIndex[f2Idx].ChunkSizes[1]);
            Assert.Equal(52U, p4.EntriesByIndex[f2Idx].ChunkSizes[2]);
            Assert.Equal(3, p4.EntriesByIndex[f2Idx].ChunkSizes.Count);

            // File 3
            var f3Idx = p4.GetIndexForItem(this.file3);
            Assert.Equal(3U, p4.EntriesByIndex[f3Idx].ChunkSizes[0]);
            Assert.Equal(13U, p4.EntriesByIndex[f3Idx].ChunkSizes[1]);
            Assert.Equal(23U, p4.EntriesByIndex[f3Idx].ChunkSizes[2]);
            Assert.Equal(3, p4.EntriesByIndex[f3Idx].ChunkSizes.Count);
        }

        [Fact]
        public void NefsHeaderPart4_NoItems_EntriesEmpty()
        {
            var items = new NefsItemList(@"C:\archive.nefs");
            var p4 = new NefsHeaderPart4(items);
            Assert.Empty(p4.EntriesByIndex);
            Assert.Equal(0U, p4.LastFourBytes);
            Assert.Equal(4U, p4.Size);
        }
    }
}
