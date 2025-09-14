// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.Header.Version130;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.Header.Builder;

public class NefsItemListBuilder130Tests
{
	[Fact]
	public void BuiltItem_ItemHasUniqueBlockTransforms_ItemCreated()
	{
		const string volumeFilePath = "archive.nefs";
		const string fileName = "test.file";
		const uint compressedSize = 10;
		const uint extractedSize = 20;
		const long dataStart = 100;
		const uint flags = (uint)NefsTocEntryFlags010.Transformed;

		var header = new NefsHeader130(new NefsWriterSettings(),
			new NefsTocHeader130
			{
				NumVolumes = 1,
				BlockSize = extractedSize / 2,
				AesKey = new AesKeyHexBuffer(new string(Enumerable.Repeat('A', 64).ToArray()))
			},
			new NefsHeaderEntryTable010(
				[new NefsTocEntry010 { Start = dataStart, Size = extractedSize, Flags = flags }]),
			new NefsHeaderLinkTable010([new NefsTocLink010()]),
			new NefsHeaderNameTable([fileName]),
			new NefsHeaderBlockTable010([
				new NefsTocBlock010 { End = compressedSize / 2, Transformation = 1 },
				new NefsTocBlock010 { End = compressedSize, Transformation = 4 }
			]), new NefsHeaderVolumeSizeTable010([new NefsTocVolumeSize010 { Size = 2000 }]),
			new NefsHeaderVolumeNameStartTable130([new NefsTocVolumeNameStart130 { Start = 0 }]),
			new NefsHeaderNameTable([volumeFilePath]));

		var builder = new NefsItemListBuilder130(header, NefsLog.GetLogger());
		var item = builder.BuildItem(0, new NefsItemList(volumeFilePath));

		// File has 2 blocks with different transforms
		Assert.Equal(compressedSize, item.CompressedSize);
		Assert.Equal(0, item.DirectoryId.Index);
		Assert.Equal(extractedSize, item.ExtractedSize);
		Assert.Equal(fileName, item.FileName);
		Assert.Equal(0, item.Id.Index);
		Assert.Equal(NefsItemState.None, item.State);
		Assert.Equal(NefsItemType.File, item.Type);
		Assert.Equal(volumeFilePath, item.DataSource.FilePath);
		Assert.Equal(dataStart, item.DataSource.Offset);
		Assert.True(item.DataSource.IsTransformed);
		Assert.Equal(2, item.DataSource.Size.Chunks.Count);
		Assert.Equal(compressedSize / 2, item.DataSource.Size.Chunks[0].Size);
		Assert.True(item.DataSource.Size.Chunks[0].Transform.IsLzssCompressed);
		Assert.Equal(compressedSize / 2, item.DataSource.Size.Chunks[1].Size);
		Assert.True(item.DataSource.Size.Chunks[1].Transform.IsAesEncrypted);
		Assert.Equal(extractedSize, item.DataSource.Size.ExtractedSize);
		Assert.Equal(compressedSize, item.DataSource.Size.TransformedSize);
		Assert.True(item.Attributes.IsTransformed);
		Assert.NotNull(item.Transform);
		Assert.True(item.Transform.IsLzssCompressed);
	}
}
