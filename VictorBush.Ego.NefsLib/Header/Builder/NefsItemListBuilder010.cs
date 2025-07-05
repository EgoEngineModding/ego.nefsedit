// See LICENSE.txt for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsItemListBuilder010(NefsHeader010 header, ILogger logger)
	: NefsItemListBuilder<NefsHeader010>(header, logger)
{
	internal override NefsItem BuildItem(uint entryIndex, NefsVolumeSource volume)
	{
		var id = new NefsItemId(entryIndex);
		var entry = Header.EntryTable.Entries[id.Index];
		var link = Header.LinkTable.GetEntryByOffset(entry.LinkOffset);

		// Gather attributes
		var attributes = CreateAttributes(entry);

		// Data source
		INefsDataSource dataSource;
		NefsDataTransform? transform = null;
		if (attributes.IsDirectory)
		{
			// Item is a directory
			dataSource = new NefsEmptyDataSource();
		}
		else
		{
			// Offset and size
			var dataOffset = Convert.ToInt64(entry.Start);
			var extractedSize = entry.Size;

			var numBlocks = GetNumBlocks(extractedSize);
			var blocks = BuildBlockList(entry.FirstBlock, numBlocks, null);
			transform = blocks.FirstOrDefault()?.Transform ?? GetTransform(0);
			var size = new NefsItemSize(extractedSize, blocks);
			dataSource = new NefsVolumeDataSource(volume, dataOffset, size);
		}

		// Create item
		// Haven't seen examples of duplicates in any archives
		var duplicateId = new NefsItemId(entryIndex);
		var parentId = new NefsItemId(link.ParentOffset / (uint)NefsTocEntry010.ByteCount);
		var fileName = Header.GetFileName(link.NameOffset);
		return new NefsItem(id, duplicateId, fileName, parentId, dataSource, transform, attributes);

		static NefsItemAttributes CreateAttributes(NefsTocEntry010 entry)
		{
			Debug.Assert((entry.Flags & 0xFFE0) == 0);
			var flags = (NefsTocEntryFlags010)entry.Flags;
			return new NefsItemAttributes(
				v16IsTransformed: flags.HasFlag(NefsTocEntryFlags010.Transformed),
				isDirectory: flags.HasFlag(NefsTocEntryFlags010.Directory),
				isDuplicated: flags.HasFlag(NefsTocEntryFlags010.Duplicated),
				isCacheable: flags.HasFlag(NefsTocEntryFlags010.Cacheable),
				isPatched: false,
				part6Volume: Convert.ToUInt16(entry.Volume))
			{
				IsLastSibling = false
			};
		}
	}

	protected override (uint End, uint Transformation) GetBlock(uint blockIndex)
	{
		var block = Header.BlockTable.Entries[Convert.ToInt32(blockIndex)];
		return (block.End, block.Transformation);
	}

	protected override NefsDataTransformType GetTransformType(uint blockTransformation)
	{
		return blockTransformation switch
		{
			0 => NefsDataTransformType.None,
			1 => NefsDataTransformType.Lzss,
			_ => (NefsDataTransformType)(-1)
		};
	}
}
