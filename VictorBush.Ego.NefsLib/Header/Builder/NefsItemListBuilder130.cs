// See LICENSE.txt for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.Header.Version130;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsItemListBuilder130(NefsHeader130 header, ILogger logger)
	: NefsItemListBuilder<NefsHeader130>(header, logger)
{
	internal override NefsItem BuildItem(uint entryIndex, NefsItemList dataSourceList)
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
			dataSource = new NefsItemListDataSource(dataSourceList, dataOffset, size);
		}

		// Create item
		var duplicateId = new NefsItemId(GetFirstDuplicateIndex(id.Index, ref entry, Header.EntryTable.Entries));
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
					break;
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
			_ => (NefsDataTransformType)(-1)
		};
	}

	// private void Tmp()
	// {
	// 	NefsItemList dataSourceList;
	// 	IReadOnlyList<NefsTocEntry010> entries;
	// 	var nv16 = entries.Count;
	// 	var uv15 = 0;
	// 	if (nv16 > 0)
	// 	{
	// 		// lv25 = uv15.Flags;
	// 		while (true)
	// 		{
	// 			--nv16;
	// 			var currentEntry = entries[uv15]; // puv11 = lv25;
	// 			if ((currentEntry.Flags & 0x0C) == 0x08)
	// 			{
	// 				var lv23 = nv16 + 1;
	// 				var lv22 = uv15 + 1;
	// 				var uv14 = 0uL;
	// 				var uv27 = 0uL;
	// 				while (true)
	// 				{
	// 					var nextEntry = entries[lv22]; // puv12 = lv22;
	// 					var uv10 = lv22; // also nextEntry
	// 					--lv23;
	// 					if (lv23 == 0 || nextEntry.LinkOffset != currentEntry.LinkOffset)
	// 					{
	// 						break;
	// 					}
	//
	// 					if (uv14 == 0)
	// 					{
	// 						++lv22;
	// 						uv14 = (((ulong)nextEntry.Flags & 3) << 28) | ((ulong)nextEntry.Flags >> 4);
	// 						nextEntry.LinkOffset = (ulong)uv15;
	// 						currentEntry.Size = uv10;
	// 						nextEntry.Flags = (uint)(uv14 << 4) | (uint)uv14 >> 28 | 0x0C;
	// 						uv14 = ((ulong)currentEntry.Flags & 3) << 28 | (ulong)(currentEntry.Flags >> 4);
	// 						currentEntry.Flags = (uint)(uv14 << 4) | (uint)uv14 >> 28 | 8;
	// 						uv14 = (ulong)uv10;
	// 					}
	// 					else
	// 					{
	// 						var uv13 = (((ulong)nextEntry.Flags & 3) << 28) | ((ulong)nextEntry.Flags >> 4);;
	// 						nextEntry.LinkOffset = 0;
	// 						nextEntry.Size = (uint)uv15;
	// 						nextEntry.Flags = (uint)(uv13 << 4) | (uint)uv13 >> 28 | 4;
	// 						if ((int)uv27 == 0)
	// 						{
	// 							++lv22;
	// 							entries[(int)uv14].LinkOffset = uv10;
	// 							uv27 = (ulong)lv22;
	// 						}
	// 						else
	// 						{
	// 							++lv22;
	// 							entries[(int)uv27].LinkOffset = uv10;
	// 							uv27 = (ulong)lv22;
	// 						}
	// 					}
	// 				}
	// 			}
	//
	// 			if (nv16 == 0) break;
	// 			++uv15;
	// 			// lv25 = uv15.Flags;
	// 		}
	// 	}
	// }
	//
	// private void a()
	// {
	// 	NVar16 = this->m_numEntries;
	// 	uVar15 = ZEXT48(this->m_entryTable);
	// 	if (NVar16 != 0)
	// 	{
	// 		lVar25 = uVar15 + 0x1c;
	// 		while (true)
	// 		{
	// 			NVar16 = NVar16 - 1;
	// 			puVar11 = (uint*)lVar25;
	// 			if ((*puVar11 & 0xc) == 8)
	// 			{
	// 				lVar23 = (ulonglong)NVar16 + 1;
	// 				lVar22 = uVar15 + 0x3c;
	// 				uVar9 = (uint)uVar15;
	// 				uVar24 = uVar15;
	// 				uVar14 = 0;
	// 				uVar27 = 0;
	// 				while (true)
	// 				{
	// 					puVar12 = (uint*)lVar22;
	// 					uVar24 = uVar24 + 0x20;
	// 					uVar10 = (uint)uVar24;
	// 					lVar23 = lVar23 + -1;
	// 					if ((lVar23 == 0) || (*(int*)(uVar10 + 8) != *(int*)(uVar9 + 8))) break;
	// 					if ((int)uVar14 == 0)
	// 					{
	// 						lVar22 = lVar22 + 0x20;
	// 						uVar14 = ((ulonglong) * puVar12 & 3) << 0x1c | (ulonglong)(*puVar12 >> 4);
	// 						*(uint*)(uVar10 + 8) = uVar9;
	// 						puVar11[-3] = uVar10;
	// 						*puVar12 = (uint)(uVar14 << 4) | (uint)uVar14 >> 0x1c | 0xc;
	// 						uVar14 = ((ulonglong) * puVar11 & 3) << 0x1c | (ulonglong)(*puVar11 >> 4);
	// 						*puVar11 = (uint)(uVar14 << 4) | (uint)uVar14 >> 0x1c | 8;
	// 						uVar14 = uVar24;
	// 					}
	// 					else
	// 					{
	// 						uVar21 = uVar14 & 0xffffffff;
	// 						uVar13 = ((ulonglong) * puVar12 & 3) << 0x1c | (ulonglong)(*puVar12 >> 4);
	// 						*(undefined4*)(uVar10 + 8) = 0;
	// 						puVar12[-3] = uVar9;
	// 						*puVar12 = (uint)(uVar13 << 4) | (uint)uVar13 >> 0x1c | 4;
	// 						if ((int)uVar27 == 0)
	// 						{
	// 							lVar22 = lVar22 + 0x20;
	// 							*(uint*)((int)uVar14 + 8) = uVar10;
	// 							uVar27 = uVar24;
	// 						}
	// 						else
	// 						{
	// 							lVar22 = lVar22 + 0x20;
	// 							*(uint*)((int)uVar27 + 8) = uVar10;
	// 							uVar27 = uVar24;
	// 						}
	// 					}
	// 				}
	// 			}
	//
	// 			__in_chrg = (int)uVar21;
	// 			lVar25 = lVar25 + 0x20;
	// 			if (NVar16 == 0) break;
	// 			uVar15 = uVar15 + 0x20;
	// 		}
	// 	}
	// }
}
