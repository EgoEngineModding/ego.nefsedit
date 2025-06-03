// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Tests.TestArchives;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.Header.Builder;

public class NefsItemListBuilder200Tests
{
	[Fact]
	public void CreateHeaderFromFile_ItemIsCompressed_ItemCreated()
	{
		var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
		var builder = new NefsItemListBuilder200((NefsHeader200)nefs.Header, NefsLog.GetLogger());
		var item = builder.BuildItem(TestArchiveNotModified.File2ItemId, nefs.Items);

		// File2 is compressed
		var expected = nefs.Items.GetItem(item.Id);
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
		Assert.True(item.Transform!.IsZlibCompressed);
		Assert.NotNull(item.Transform);
	}

	[Fact]
	public void CreateHeaderFromFile_ItemIsDirectory_ItemCreated()
	{
		var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
		var builder = new NefsItemListBuilder200((NefsHeader200)nefs.Header, NefsLog.GetLogger());
		var item = builder.BuildItem(TestArchiveNotModified.Dir1ItemId, nefs.Items);

		var expected = nefs.Items.GetItem(item.Id);
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
		var builder = new NefsItemListBuilder200((NefsHeader200)nefs.Header, NefsLog.GetLogger());
		var item = builder.BuildItem(TestArchiveNotModified.File3ItemId, nefs.Items);

		// File3 is not compressed
		var expected = nefs.Items.GetItem(item.Id);
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

	[Fact]
	public void BuiltItem_ItemHasNoBlocks_ItemCreated()
	{
		var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
		var builder = new NefsItemListBuilder200((NefsHeader200)nefs.Header, NefsLog.GetLogger());
		var item = builder.BuildItem(TestArchiveNotModified.File4ItemId, nefs.Items);

		// File4 has no blocks
		var expected = nefs.Items.GetItem(item.Id);
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
		Assert.Empty(item.DataSource.Size.Chunks);
		Assert.Equal(expected.ExtractedSize, item.DataSource.Size.TransformedSize);
		Assert.Equal(expected.ExtractedSize, item.DataSource.Size.ExtractedSize);
		Assert.Equal(expected.CompressedSize, item.DataSource.Size.TransformedSize);
		Assert.True(item.Attributes.V20IsZlib);
		Assert.Null(item.Transform);
	}
}
