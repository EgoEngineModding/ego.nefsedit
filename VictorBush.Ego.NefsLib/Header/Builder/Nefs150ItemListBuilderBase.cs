// See LICENSE.txt for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal abstract class Nefs150ItemListBuilderBase<T>(T header, ILogger logger)
	: NefsItemListBuilder<T>(header, logger)
	where T : INefsHeader
{
	protected NefsItem BuildItem(
		NefsItemId id,
		Nefs150TocEntry entry,
		Nefs150TocSharedEntryInfo sharedEntryInfo,
		NefsItemList dataSourceList)
	{
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
			var extractedSize = sharedEntryInfo.Size;

			var numBlocks = GetNumBlocks(extractedSize);
			var blocks = BuildBlockList(entry.FirstBlock, numBlocks, null);
			transform = blocks.FirstOrDefault()?.Transform ?? GetTransform(0);
			var size = new NefsItemSize(extractedSize, blocks);
			dataSource = new NefsItemListDataSource(dataSourceList, dataOffset, size);
		}

		// Create item
		var duplicateId = new NefsItemId(sharedEntryInfo.FirstDuplicate);
		var parentId = new NefsItemId(sharedEntryInfo.Parent);
		var fileName = Header.GetFileName(sharedEntryInfo.NameOffset);
		return new NefsItem(id, duplicateId, fileName, parentId, dataSource, transform, attributes);

		static NefsItemAttributes CreateAttributes(Nefs150TocEntry entry)
		{
			Debug.Assert((entry.Flags & 0xFFE0) == 0);
			var flags = (Nefs150TocEntryFlags)entry.Flags;
			return new NefsItemAttributes(
				v16IsTransformed: flags.HasFlag(Nefs150TocEntryFlags.Transformed),
				isDirectory: flags.HasFlag(Nefs150TocEntryFlags.Directory),
				isDuplicated: flags.HasFlag(Nefs150TocEntryFlags.Duplicated),
				isCacheable: flags.HasFlag(Nefs150TocEntryFlags.Cacheable),
				isPatched: flags.HasFlag(Nefs150TocEntryFlags.Patched),
				part6Volume: entry.Volume)
			{
				IsLastSibling = flags.HasFlag(Nefs150TocEntryFlags.LastSibling)
			};
		}
	}

	protected override NefsDataTransformType GetTransformType(uint blockTransformation)
	{
		return blockTransformation switch
		{
			0 => NefsDataTransformType.None,
			1 => NefsDataTransformType.Lzss,
			4 => NefsDataTransformType.Aes,
			_ => (NefsDataTransformType)(-1)
		};
	}
}
