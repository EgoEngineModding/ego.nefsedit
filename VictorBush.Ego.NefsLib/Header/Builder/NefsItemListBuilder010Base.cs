// See LICENSE.txt for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal abstract class NefsItemListBuilder010Base<T>(T header, ILogger logger)
	: NefsItemListBuilder<T>(header, logger)
	where T : INefsHeader
{
	protected NefsItem BuildItem(
		NefsItemId id,
		NefsTocEntry010 entry,
		NefsTocLink010 link,
		IReadOnlyList<NefsTocEntry010> entries,
		NefsItemList itemList)
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
			var extractedSize = entry.Size;

			var numBlocks = GetNumBlocks(extractedSize);
			var blocks = BuildBlockList(entry.FirstBlock, numBlocks, attributes.IsTransformed ? null : GetTransform(0));
			transform = blocks.FirstOrDefault()?.Transform ?? GetTransform(0);
			var size = new NefsItemSize(extractedSize, blocks);
			dataSource = new NefsVolumeDataSource(itemList.Volumes[(int)entry.Volume], dataOffset, size);
		}

		// Create item
		var duplicateId = new NefsItemId(GetFirstDuplicateIndex(id.Index, ref entry, entries));
		var parentId = new NefsItemId(link.ParentOffset / (uint)NefsTocEntry010.ByteCount);
		var fileName = Header.GetFileName(link.NameOffset);
		return new NefsItem(id, duplicateId, fileName, parentId, dataSource, transform, attributes);

		static NefsItemAttributes CreateAttributes(NefsTocEntry010 entry)
		{
			Debug.Assert((entry.Flags & 0xFFE0) == 0);
			var flags = (NefsTocEntryFlags010)entry.Flags;
			return new NefsItemAttributes(
				isDirectory: flags.HasFlag(NefsTocEntryFlags010.Directory),
				isDuplicated: flags.HasFlag(NefsTocEntryFlags010.Duplicated),
				isCacheable: flags.HasFlag(NefsTocEntryFlags010.Cacheable),
				isPatched: false)
			{
				IsTransformed = flags.HasFlag(NefsTocEntryFlags010.Transformed),
				IsLastSibling = false,
				Volume = Convert.ToUInt16(entry.Volume)
			};
		}

		static int GetFirstDuplicateIndex(int index, ref NefsTocEntry010 entry, IReadOnlyList<NefsTocEntry010> entries)
		{
			const uint duplicateMask = 0x0C;
			var duplicateFlags = entry.Flags & duplicateMask;
			if (duplicateFlags is 0 or 8)
			{
				return index;
			}

			for (var i = index - 1; i >= 0; --i)
			{
				var prevEntry = entries[i];
				if (prevEntry.LinkOffset != entry.LinkOffset)
				{
					continue;
				}

				var prevDuplicateFlags = prevEntry.Flags & duplicateMask;
				if (prevDuplicateFlags is 8)
				{
					return i;
				}
			}

			throw new InvalidDataException("Invalid duplicate data.");
		}
	}

	/// <inheritdoc />
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
