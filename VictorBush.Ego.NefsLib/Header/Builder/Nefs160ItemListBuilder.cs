// See LICENSE.txt for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class Nefs160ItemListBuilder(Nefs160Header header, ILogger logger)
	: NefsItemListBuilder<Nefs160Header>(header, logger)
{
	protected override NefsItem BuildItem(uint entryIndex, NefsItemList dataSourceList)
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

			// Transform
			transform = new NefsDataTransform(Header.BlockSize, false, Header.IsEncrypted ? Header.AesKey : null);

			var numBlocks = GetNumBlocks(extractedSize);
			var chunks = BuildBlockList(entry.FirstBlock, numBlocks, null);
			var size = new NefsItemSize(extractedSize, chunks);
			dataSource = new NefsItemListDataSource(dataSourceList, dataOffset, size);
		}

		// Create item
		var duplicateId = new NefsItemId(sharedEntryInfo.FirstDuplicate);
		var parentId = new NefsItemId(sharedEntryInfo.Parent);
		var fileName = Header.GetFileName(sharedEntryInfo.NameOffset);
		return new NefsItem(id, duplicateId, fileName, parentId, dataSource, transform, attributes);

		static NefsItemAttributes CreateAttributes(Nefs160TocEntryWriteable entry)
		{
			Debug.Assert((entry.Flags & 0xFFE0) == 0);
			var flags = (Nefs150TocEntryFlags)entry.Flags;
			return new NefsItemAttributes(
				v16IsTransformed: flags.HasFlag(Nefs150TocEntryFlags.Transformed),
				isDirectory: flags.HasFlag(Nefs150TocEntryFlags.Directory),
				isDuplicated: flags.HasFlag(Nefs150TocEntryFlags.Duplicated),
				isCacheable: flags.HasFlag(Nefs150TocEntryFlags.Cacheable),
				v16Unknown0x10: flags.HasFlag(Nefs150TocEntryFlags.LastSibling),
				isPatched: flags.HasFlag(Nefs150TocEntryFlags.Patched),
				part6Volume: entry.Volume);
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
			4 => NefsDataTransformType.Aes,
			7 => NefsDataTransformType.Zlib,
			_ => (NefsDataTransformType)(-1)
		};
	}
}
