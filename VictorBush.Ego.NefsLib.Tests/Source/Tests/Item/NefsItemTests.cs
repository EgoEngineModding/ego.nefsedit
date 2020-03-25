// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Item
{
    using System.Linq;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Tests.TestArchives;
    using Xunit;

    public class NefsItemTests
    {
        [Fact]
        public void Clone_ItemCloned()
        {
            var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var itemId = new NefsItemId(TestArchiveNotModified.File3ItemId);
            var item = NefsItem.CreateFromHeader(itemId, nefs.Header, nefs.Items);
            item.UpdateState(NefsItemState.Replaced);

            var clone = item.Clone() as NefsItem;

            Assert.Equal(item.CompressedSize, clone.CompressedSize);
            Assert.Same(item.DataSource, clone.DataSource);
            Assert.Equal(item.DirectoryId, clone.DirectoryId);
            Assert.Equal(item.ExtractedSize, clone.ExtractedSize);
            Assert.Equal(item.FileName, clone.FileName);
            Assert.Equal(item.FilePathInArchive, clone.FilePathInArchive);
            Assert.Equal(item.FilePathInArchiveHash, clone.FilePathInArchiveHash);
            Assert.Equal(item.Id, clone.Id);
            Assert.Equal(item.Part6Unknown0x00, clone.Part6Unknown0x00);
            Assert.Equal(item.Part6Unknown0x01, clone.Part6Unknown0x01);
            Assert.Equal(item.Part6Unknown0x02, clone.Part6Unknown0x02);
            Assert.Equal(item.Part6Unknown0x03, clone.Part6Unknown0x03);
            Assert.Equal(item.Part7Unknown0x00, clone.Part7Unknown0x00);
            Assert.Equal(item.Part7Unknown0x04, clone.Part7Unknown0x04);
            Assert.Equal(item.State, clone.State);
            Assert.Equal(item.Type, clone.Type);
        }

        [Fact]
        public void CreateHeaderFromFile_ItemIsCompressed_ItemCreated()
        {
            var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var itemId = new NefsItemId(TestArchiveNotModified.File2ItemId);
            var item = NefsItem.CreateFromHeader(itemId, nefs.Header, nefs.Items);

            // File2 is compressed
            var expected = nefs.Items[itemId];
            Assert.Equal(expected.CompressedSize, item.CompressedSize);
            Assert.Equal(expected.DirectoryId, item.DirectoryId);
            Assert.Equal(expected.ExtractedSize, item.ExtractedSize);
            Assert.Equal(expected.FileName, item.FileName);
            Assert.Equal(expected.FilePathInArchive, item.FilePathInArchive);
            Assert.Equal(expected.FilePathInArchiveHash, item.FilePathInArchiveHash);
            Assert.Equal(expected.Id, item.Id);
            Assert.Equal(expected.State, item.State);
            Assert.Equal(NefsItemType.File, item.Type);
            Assert.Equal(@"C:\archive.nefs", item.DataSource.FilePath);
            Assert.Equal(expected.DataSource.Offset, item.DataSource.Offset);
            Assert.False(item.DataSource.ShouldCompress);
            Assert.Equal(expected.DataSource.Size.ChunkSizes.Count, item.DataSource.Size.ChunkSizes.Count);
            Assert.True(expected.DataSource.Size.ChunkSizes.SequenceEqual(item.DataSource.Size.ChunkSizes));
            Assert.Equal(expected.ExtractedSize, item.DataSource.Size.ExtractedSize);
            Assert.True(item.DataSource.Size.IsCompressed);
            Assert.Equal(expected.CompressedSize, item.DataSource.Size.Size);
        }

        [Fact]
        public void CreateHeaderFromFile_ItemIsDirectory_ItemCreated()
        {
            var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var itemId = new NefsItemId(TestArchiveNotModified.Dir1ItemId);
            var item = NefsItem.CreateFromHeader(itemId, nefs.Header, nefs.Items);

            var expected = nefs.Items[itemId];
            Assert.Equal(0U, item.CompressedSize);
            Assert.Equal(expected.DirectoryId, item.DirectoryId);
            Assert.Equal(0U, item.ExtractedSize);
            Assert.Equal(expected.FileName, item.FileName);
            Assert.Equal(expected.FilePathInArchive, item.FilePathInArchive);
            Assert.Equal(expected.FilePathInArchiveHash, item.FilePathInArchiveHash);
            Assert.Equal(expected.Id, item.Id);
            Assert.Equal(expected.State, item.State);
            Assert.Equal(NefsItemType.Directory, item.Type);
            Assert.Equal("", item.DataSource.FilePath);
            Assert.Equal(0U, item.DataSource.Offset);
            Assert.False(item.DataSource.ShouldCompress);
            Assert.Empty(item.DataSource.Size.ChunkSizes);
            Assert.Equal(0U, item.DataSource.Size.ExtractedSize);
            Assert.False(item.DataSource.Size.IsCompressed);
            Assert.Equal(0U, item.DataSource.Size.Size);
        }

        [Fact]
        public void CreateHeaderFromFile_ItemIsNotCompressed_ItemCreated()
        {
            var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var itemId = new NefsItemId(TestArchiveNotModified.File3ItemId);
            var item = NefsItem.CreateFromHeader(itemId, nefs.Header, nefs.Items);

            // File3 is not compressed
            var expected = nefs.Items[itemId];
            Assert.Equal(expected.CompressedSize, item.CompressedSize);
            Assert.Equal(expected.DirectoryId, item.DirectoryId);
            Assert.Equal(expected.ExtractedSize, item.ExtractedSize);
            Assert.Equal(expected.FileName, item.FileName);
            Assert.Equal(expected.FilePathInArchive, item.FilePathInArchive);
            Assert.Equal(expected.FilePathInArchiveHash, item.FilePathInArchiveHash);
            Assert.Equal(expected.Id, item.Id);
            Assert.Equal(expected.State, item.State);
            Assert.Equal(NefsItemType.File, item.Type);
            Assert.Equal(@"C:\archive.nefs", item.DataSource.FilePath);
            Assert.Equal(expected.DataSource.Offset, item.DataSource.Offset);
            Assert.False(item.DataSource.ShouldCompress);
            Assert.Single(item.DataSource.Size.ChunkSizes);
            Assert.Equal(expected.ExtractedSize, item.DataSource.Size.ChunkSizes[0]);
            Assert.Equal(expected.ExtractedSize, item.DataSource.Size.ExtractedSize);
            Assert.False(item.DataSource.Size.IsCompressed);
            Assert.Equal(expected.CompressedSize, item.DataSource.Size.Size);
        }
    }
}
