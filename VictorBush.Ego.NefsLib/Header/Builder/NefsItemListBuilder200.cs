// See LICENSE.txt for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsItemListBuilder200(NefsHeader200 header, ILogger logger)
	: NefsItemListBuilder<NefsHeader200>(header, logger)
{
	internal override NefsItem BuildItem(uint entryIndex, NefsItemList dataSourceList)
	{
		var id = new NefsItemId(entryIndex);
		var entry = Header.EntryTable.Entries[id.Index];
		var sharedEntryInfo = Header.SharedEntryInfoTable.Entries[Convert.ToInt32(entry.SharedInfo)];
		var entryWritable = Header.WriteableEntryTable.Entries[id.Index];

		// Gather attributes
		var attributes = CreateAttributes(entryWritable);

		// Offset and size
		var dataOffset = Convert.ToInt64(entry.Start);
		var extractedSize = sharedEntryInfo.Size;

		// Data source
		INefsDataSource dataSource;
		NefsDataTransform? transform = null;
		if (attributes.IsDirectory)
		{
			// Item is a directory
			dataSource = new NefsEmptyDataSource();
		}
		else if (entry.FirstBlock == NefsConstants.NoBlocksIndex)
		{
			// Item is not transformed
			var size = new NefsItemSize(extractedSize);
			dataSource = new NefsItemListDataSource(dataSourceList, dataOffset, size);
		}
		else
		{
			transform = new NefsDataTransform(Header.BlockSize, attributes.V20IsZlib, attributes.V20IsAes ? Header.AesKey : null);
			var numBlocks = GetNumBlocks(extractedSize);
			var blocks = BuildBlockList(entry.FirstBlock, numBlocks, transform);
			var size = new NefsItemSize(extractedSize, blocks);
			dataSource = new NefsItemListDataSource(dataSourceList, dataOffset, size);
		}

		// Create item
		var duplicateId = new NefsItemId(sharedEntryInfo.FirstDuplicate);
		var parentId = new NefsItemId(sharedEntryInfo.Parent);
		var fileName = Header.GetFileName(sharedEntryInfo.NameOffset);
		return new NefsItem(id, duplicateId, fileName, parentId, dataSource, transform, attributes);

		static NefsItemAttributes CreateAttributes(NefsTocEntryWriteable160 entry)
		{
			Debug.Assert((entry.Flags & 0xFFE0) == 0);
			var flags = (NefsTocEntryFlags200)entry.Flags;
			return new NefsItemAttributes(
				v20IsZlib: flags.HasFlag(NefsTocEntryFlags200.IsZlib),
				v20IsAes: flags.HasFlag(NefsTocEntryFlags200.IsAes),
				isDirectory: flags.HasFlag(NefsTocEntryFlags200.IsDirectory),
				isDuplicated: flags.HasFlag(NefsTocEntryFlags200.IsDuplicated),
				part6Volume: entry.Volume)
			{
				IsLastSibling = flags.HasFlag(NefsTocEntryFlags200.LastSibling)
			};
		}
	}

	protected override (uint End, uint Transformation) GetBlock(uint blockIndex)
	{
		var block = Header.BlockTable.Entries[Convert.ToInt32(blockIndex)];
		return (block.End, uint.MaxValue);
	}

	protected override NefsDataTransformType GetTransformType(uint blockTransformation)
	{
		// Blocks don't have transforms in this version
		return (NefsDataTransformType)(-1);
	}
}
