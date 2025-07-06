// See LICENSE.txt for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsItemListBuilder160(NefsHeader160 header, ILogger logger)
	: NefsItemListBuilder<NefsHeader160>(header, logger)
{
	/// <inheritdoc />
	internal override NefsItem BuildItem(uint entryIndex, NefsItemList itemList)
	{
		var id = new NefsItemId(entryIndex);
		var entry = Header.EntryTable.Entries[id.Index];
		var sharedEntryInfo = Header.SharedEntryInfoTable.Entries[Convert.ToInt32(entry.SharedInfo)];
		var entryWritable = Header.WriteableEntryTable.Entries[id.Index];

		// Gather attributes
		var attributes = CreateAttributes(entryWritable);

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
			var extractedSize = sharedEntryInfo.Size;

			var numBlocks = GetNumBlocks(extractedSize);
			var blocks = BuildBlockList(entry.FirstBlock, numBlocks, attributes.IsTransformed ? null : GetTransform(0));
			transform = blocks.FirstOrDefault()?.Transform ?? GetTransform(0);
			var size = new NefsItemSize(extractedSize, blocks);
			dataSource = new NefsVolumeDataSource(itemList.Volumes[entryWritable.Volume], dataOffset, size);
		}

		// Create item
		var duplicateId = new NefsItemId(sharedEntryInfo.FirstDuplicate);
		var parentId = new NefsItemId(sharedEntryInfo.Parent);
		var fileName = Header.GetFileName(sharedEntryInfo.NameOffset);
		return new NefsItem(id, duplicateId, fileName, parentId, dataSource, transform, attributes);

		static NefsItemAttributes CreateAttributes(NefsTocEntryWriteable160 entry)
		{
			Debug.Assert((entry.Flags & 0xFFE0) == 0);
			var flags = (NefsTocEntryFlags150)entry.Flags;
			return new NefsItemAttributes(
				isDirectory: flags.HasFlag(NefsTocEntryFlags150.Directory),
				isDuplicated: flags.HasFlag(NefsTocEntryFlags150.Duplicated),
				isCacheable: flags.HasFlag(NefsTocEntryFlags150.Cacheable),
				isPatched: flags.HasFlag(NefsTocEntryFlags150.Patched))
			{
				IsTransformed = flags.HasFlag(NefsTocEntryFlags150.Transformed),
				IsLastSibling = flags.HasFlag(NefsTocEntryFlags150.LastSibling),
				Volume = entry.Volume
			};
		}
	}

	/// <inheritdoc />
	protected override (uint End, uint Transformation) GetBlock(uint blockIndex)
	{
		var block = Header.BlockTable.Entries[Convert.ToInt32(blockIndex)];
		return (block.End, block.Transformation);
	}

	/// <inheritdoc />
	protected override NefsDataTransformType GetTransformType(uint blockTransformation)
	{
		return blockTransformation switch
		{
			0 => NefsDataTransformType.None,
			1 => NefsDataTransformType.Lzss,
			4 => NefsDataTransformType.Aes,
			7 => NefsDataTransformType.Zlib,
			_ => (NefsDataTransformType)(-1)
		};
	}
}
