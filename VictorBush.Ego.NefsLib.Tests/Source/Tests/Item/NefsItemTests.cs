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
            Assert.Equal(item.Id, clone.Id);
            Assert.Equal(item.Part6Unknown0x00, clone.Part6Unknown0x00);
            Assert.Equal(item.Part6Unknown0x01, clone.Part6Unknown0x01);
            Assert.Equal(item.Part6Unknown0x02, clone.Part6Unknown0x02);
            Assert.Equal(item.Part6Unknown0x03, clone.Part6Unknown0x03);
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
            var expected = nefs.Items.GetItem(itemId);
            Assert.Equal(expected.CompressedSize, item.CompressedSize);
            Assert.Equal(expected.DirectoryId, item.DirectoryId);
            Assert.Equal(expected.ExtractedSize, item.ExtractedSize);
            Assert.Equal(expected.FileName, item.FileName);
            Assert.Equal(expected.Id, item.Id);
            Assert.Equal(expected.State, item.State);
            Assert.Equal(NefsItemType.File, item.Type);
            Assert.Equal(@"C:\archive.nefs", item.DataSource.FilePath);
            Assert.Equal(expected.DataSource.Offset, item.DataSource.Offset);
            Assert.True(item.DataSource.IsTransformed);
            Assert.Equal(expected.DataSource.Size.Chunks.Count, item.DataSource.Size.Chunks.Count);
            Assert.True(expected.DataSource.Size.Chunks.Select(c => c.CumulativeSize).SequenceEqual(item.DataSource.Size.Chunks.Select(c => c.CumulativeSize)));
            Assert.Equal(expected.ExtractedSize, item.DataSource.Size.ExtractedSize);
            Assert.Equal(expected.CompressedSize, item.DataSource.Size.TransformedSize);
            Assert.True(item.Transform.IsZlibCompressed);
            Assert.NotNull(item.Transform);
        }

        [Fact]
        public void CreateHeaderFromFile_ItemIsDirectory_ItemCreated()
        {
            var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var itemId = new NefsItemId(TestArchiveNotModified.Dir1ItemId);
            var item = NefsItem.CreateFromHeader(itemId, nefs.Header, nefs.Items);

            var expected = nefs.Items.GetItem(itemId);
            Assert.Equal(0U, item.CompressedSize);
            Assert.Equal(expected.DirectoryId, item.DirectoryId);
            Assert.Equal(0U, item.ExtractedSize);
            Assert.Equal(expected.FileName, item.FileName);
            Assert.Equal(expected.Id, item.Id);
            Assert.Equal(expected.State, item.State);
            Assert.Equal(NefsItemType.Directory, item.Type);
            Assert.Equal("", item.DataSource.FilePath);
            Assert.Equal(0U, item.DataSource.Offset);
            Assert.True(item.DataSource.IsTransformed);
            Assert.Empty(item.DataSource.Size.Chunks);
            Assert.Equal(0U, item.DataSource.Size.ExtractedSize);
            Assert.Equal(0U, item.DataSource.Size.TransformedSize);
            Assert.Null(item.Transform);
        }

        [Fact]
        public void CreateHeaderFromFile_ItemIsNotCompressed_ItemCreated()
        {
            var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var itemId = new NefsItemId(TestArchiveNotModified.File3ItemId);
            var item = NefsItem.CreateFromHeader(itemId, nefs.Header, nefs.Items);

            // File3 is not compressed
            var expected = nefs.Items.GetItem(itemId);
            Assert.Equal(expected.CompressedSize, item.CompressedSize);
            Assert.Equal(expected.DirectoryId, item.DirectoryId);
            Assert.Equal(expected.ExtractedSize, item.ExtractedSize);
            Assert.Equal(expected.FileName, item.FileName);
            Assert.Equal(expected.Id, item.Id);
            Assert.Equal(expected.State, item.State);
            Assert.Equal(NefsItemType.File, item.Type);
            Assert.Equal(@"C:\archive.nefs", item.DataSource.FilePath);
            Assert.Equal(expected.DataSource.Offset, item.DataSource.Offset);
            Assert.True(item.DataSource.IsTransformed);
            Assert.Single(item.DataSource.Size.Chunks);
            Assert.Equal(expected.ExtractedSize, item.DataSource.Size.Chunks[0].CumulativeSize);
            Assert.Equal(expected.ExtractedSize, item.DataSource.Size.Chunks[0].Size);
            Assert.Equal(expected.ExtractedSize, item.DataSource.Size.ExtractedSize);
            Assert.Equal(expected.CompressedSize, item.DataSource.Size.TransformedSize);
            Assert.False(item.DataSource.Size.Chunks[0].Transform.IsZlibCompressed);
            Assert.NotNull(item.Transform);
        }
    }
}
