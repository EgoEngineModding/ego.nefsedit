// See LICENSE.txt for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class Nefs200ItemListBuilder(Nefs200Header header, ILogger logger)
	: NefsItemListBuilder<Nefs200Header>(header, logger)
{
	protected override NefsItem BuildItem(uint entryIndex, NefsItemList dataSourceList)
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
		else if (entry.FirstBlock == 0xFFFFFFFFU)
		{
			// Item is not compressed
			transform = new NefsDataTransform(Header.BlockSize, attributes.V20IsZlib, Header.IsEncrypted ? Header.AesKey : null);
			var size = new NefsItemSize(extractedSize);
			dataSource = new NefsItemListDataSource(dataSourceList, dataOffset, size);
		}
		else
		{
			transform = new NefsDataTransform(Header.BlockSize, attributes.V20IsZlib, Header.IsEncrypted ? Header.AesKey : null);
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

		static NefsItemAttributes CreateAttributes(Nefs160TocEntryWriteable entry)
		{
			Debug.Assert((entry.Flags & 0xFFF0) == 0);
			var flags = (Nefs200TocEntryFlags)entry.Flags;
			return new NefsItemAttributes(
				v20IsZlib: flags.HasFlag(Nefs200TocEntryFlags.IsZlib),
				v20IsAes: flags.HasFlag(Nefs200TocEntryFlags.IsAes),
				isDirectory: flags.HasFlag(Nefs200TocEntryFlags.IsDirectory),
				isDuplicated: flags.HasFlag(Nefs200TocEntryFlags.IsDuplicated),
				v20Unknown0x10: flags.HasFlag(Nefs200TocEntryFlags.Unknown0x10),
				v20Unknown0x20: flags.HasFlag(Nefs200TocEntryFlags.Unknown0x20),
				v20Unknown0x40: flags.HasFlag(Nefs200TocEntryFlags.Unknown0x40),
				v20Unknown0x80: flags.HasFlag(Nefs200TocEntryFlags.Unknown0x80),
				part6Volume: entry.Volume);
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
