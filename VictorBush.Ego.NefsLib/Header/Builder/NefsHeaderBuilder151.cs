// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsHeaderBuilder151 : NefsHeaderBuilder<NefsHeader151>
{
	/// <inheritdoc />
	internal override uint ComputeDataOffset(NefsHeader151 sourceHeader, NefsItemList items)
	{
		var nonDuplicateCount = items.EnumerateById().Count(x => !x.IsDuplicate);
		var entryTableSize = items.Count * NefsTocEntry150.ByteCount;
		var sharedEntryInfoTableSize = nonDuplicateCount * NefsTocSharedEntryInfo150.ByteCount;
		var nameTableSize = items.EnumerateById().Where(x => !x.IsDuplicate).Distinct().Sum(x => x.FileName.Length);
		var blockTableSize = items.EnumerateById().Where(x => !x.IsDuplicate).Sum(x => x.DataSource.Size.Chunks.Count) * NefsTocBlock151.ByteCount;
		var volumeInfoTableSize = sourceHeader.Volumes.Count * NefsTocVolumeInfo150.ByteCount;

		var sizeSum = NefsTocHeader151.ByteCount + entryTableSize + sharedEntryInfoTableSize + nameTableSize +
		              blockTableSize + volumeInfoTableSize;
		var tocSize = StructEx.Align(sizeSum, NefsConstants.AesBlockSize);
		var tocFinalEnd = Convert.ToUInt32(StructEx.Align(tocSize, Convert.ToInt32(sourceHeader.BlockSize)));
		return tocFinalEnd;
	}

	/// <inheritdoc />
	internal override NefsHeader151 Build(NefsHeader151 sourceHeader, NefsItemList items, NefsProgress p)
	{
		p.CancellationToken.ThrowIfCancellationRequested();
		var entryTable = BuildEntryTable150(items);
		var nameTable = new NefsHeaderNameTable(items);
		p.CancellationToken.ThrowIfCancellationRequested();
		var sharedEntryInfoTable = BuildSharedEntryInfoTable150(items, nameTable);
		var blockTable = BuildBlockTable(items);
		p.CancellationToken.ThrowIfCancellationRequested();

		// Get sizes, offsets and account for alignment where needed
		var intro = new NefsTocHeader151
		{
			Magic = NefsConstants.FourCc,
			Version = (uint)NefsVersion.Version151,
			NumVolumes = (uint)items.Volumes.Count,
			NumEntries = (uint)items.Count,
			BlockSize = sourceHeader.Intro.BlockSize,
			SplitSize = sourceHeader.Intro.SplitSize,
			UserValue = sourceHeader.Intro.UserValue,
			RandomPadding1 = sourceHeader.Intro.RandomPadding1,
			Checksum = sourceHeader.Intro.Checksum, // TODO: figure out
			AesKey = sourceHeader.Intro.AesKey,
			Unused = sourceHeader.Intro.Unused,
			EntryTableStart = Convert.ToUInt32(NefsTocHeader151.ByteCount)
		};
		intro.SharedEntryInfoTableStart = Convert.ToUInt32(intro.EntryTableStart + entryTable.ByteCount());
		intro.NameTableStart = Convert.ToUInt32(intro.SharedEntryInfoTableStart + sharedEntryInfoTable.ByteCount());
		intro.BlockTableStart =
			Convert.ToUInt32(StructEx.Align<NefsTocBlock151>(intro.NameTableStart + nameTable.Size));
		intro.VolumeInfoTableStart =
			Convert.ToUInt32(StructEx.Align<NefsTocVolumeInfo150>(intro.BlockTableStart + blockTable.ByteCount()));

		// Align by AES key size, for toc size, and then align it to block size for data offset
		var tocEnd = intro.VolumeInfoTableStart + intro.NumVolumes * NefsTocVolumeInfo150.ByteCount;
		var tocSize = StructEx.Align(tocEnd, NefsConstants.AesBlockSize);
		var tocFinalEnd = Convert.ToUInt32(StructEx.Align(tocSize, Convert.ToInt32(sourceHeader.BlockSize)));
		intro.TocSize = Convert.ToUInt32(tocSize);

		var volumes = new NefsTocVolumeInfo150[items.Count];
		for (var i = 0; i < items.Count; ++i)
		{
			// the first volume includes headers
			var dataOffset = i == 0 ? tocFinalEnd : 0;
			var dataSize = items.EnumerateById().Where(x => x.Attributes.Volume == i).Sum(x => x.CompressedSize);
			volumes[i] = new NefsTocVolumeInfo150
			{
				Size = dataOffset + (ulong)dataSize,
				NameOffset = nameTable.OffsetsByFileName[Path.GetFileName(items.Volumes[0].FilePath)],
				DataOffset = dataOffset
			};
		}

		var volumeInfoTable = new NefsHeaderVolumeInfoTable150(volumes);
		return new NefsHeader151(sourceHeader.WriterSettings, intro, entryTable, sharedEntryInfoTable, nameTable,
			blockTable, volumeInfoTable);
	}

	private static NefsHeaderEntryTable150 BuildEntryTable150(NefsItemList items)
	{
		var idSharedInfoMap = BuildIdSharedInfoMap(items);
		var entries = new NefsTocEntry150[items.Count];
		var firstBlock = 0u;

		// Enumerate items sorted by id.
		foreach (var item in items.EnumerateById())
		{
			var entry = new NefsTocEntry150
			{
				Start = Convert.ToUInt64(item.DataSource.Offset),
				Flags = GetItemFlags(item),
				Volume = item.Attributes.Volume,
				SharedInfo = GetSharedInfo(item),
				FirstBlock = GetFirstBlockLocal(item),
				NextDuplicate = items.GetItemNextDuplicateId(item.Id).Value,
			};

			entries[item.Id.Index] = entry;
		}

		return new NefsHeaderEntryTable150(entries);

		uint GetSharedInfo(NefsItem item)
		{
			return item.IsDuplicate ? idSharedInfoMap[item.FirstDuplicateId] : idSharedInfoMap[item.Id];
		}

		uint GetFirstBlockLocal(NefsItem item)
		{
			return item.IsDuplicate ? entries[item.FirstDuplicateId.Index].FirstBlock : GetFirstBlock(item, ref firstBlock);
		}
	}

	internal static uint GetFirstBlock(NefsItem item, ref uint firstBlock)
	{
		if (item.Type == NefsItemType.Directory)
		{
			return 0;
		}

		// Item is compressed; get current block, and increment for the next item
		var current = firstBlock;
		firstBlock += (uint)item.DataSource.Size.Chunks.Count;
		return current;
	}

	internal static Dictionary<NefsItemId, uint> BuildIdSharedInfoMap(NefsItemList items)
	{
		// Enumerate this list depth first. This determines the shared info order.
		return items.EnumerateDepthFirstByName()
			.Where(x => !x.IsDuplicate)
			.Select((x, i) => (x, i))
			.ToDictionary(x => x.x.Id, x => (uint)x.i);
	}

	internal static ushort GetItemFlags(NefsItem item)
	{
		var flags = NefsTocEntryFlags150.None;
		flags |= item.Attributes.IsTransformed ? NefsTocEntryFlags150.Transformed : 0;
		flags |= item.Attributes.IsDirectory ? NefsTocEntryFlags150.Directory : 0;
		flags |= item.Attributes.IsDuplicated ? NefsTocEntryFlags150.Duplicated : 0;
		flags |= item.Attributes.IsCacheable ? NefsTocEntryFlags150.Cacheable : 0;
		flags |= item.Attributes.IsLastSibling ? NefsTocEntryFlags150.LastSibling : 0;
		flags |= item.Attributes.IsPatched ? NefsTocEntryFlags150.Patched : 0;
		return (ushort)flags;
	}

	private static NefsHeaderSharedEntryInfoTable150 BuildSharedEntryInfoTable150(NefsItemList items,
		NefsHeaderNameTable nameTable)
	{
		var entries = new List<NefsTocSharedEntryInfo150>(items.Count);
		foreach (var item in items.EnumerateDepthFirstByName())
		{
			if (item.IsDuplicate)
			{
				continue;
			}

			var entry = new NefsTocSharedEntryInfo150
			{
				Parent = item.DirectoryId.Value,
				NextSibling = items.GetItemSiblingId(item.Id).Value,
				FirstChild = items.GetItemFirstChildId(item.Id).Value,
				NameOffset = nameTable.OffsetsByFileName[item.FileName],
				Size = item.DataSource.Size.ExtractedSize,
				FirstDuplicate = item.Id.Value,
				PatchedEntry = item.Id.Value
			};

			entries.Add(entry);
		}

		return new NefsHeaderSharedEntryInfoTable150(entries);
	}

	internal static NefsHeaderBlockTable151 BuildBlockTable(NefsItemList items)
	{
		var entries = new List<NefsTocBlock151>(items.Count);
		foreach (var item in items.EnumerateById())
		{
			if (item.Type == NefsItemType.Directory || item.DataSource.Size.Chunks.Count == 0 || item.IsDuplicate)
			{
				// Item does not have blocks
				continue;
			}

			// Create entry for each data block
			foreach (var chunk in item.DataSource.Size.Chunks)
			{
				var entry = new NefsTocBlock151
				{
					End = chunk.CumulativeSize,
					Transformation = Convert.ToUInt16(GetTransform(chunk.Transform)),
					Checksum = chunk.Checksum
				};

				entries.Add(entry);
			}
		}

		return new NefsHeaderBlockTable151(entries);
	}

	private static uint GetTransform(NefsDataTransform transform)
	{
		var type = GetTransformType(transform);
		return type switch
		{
			NefsDataTransformType.None => 0,
			NefsDataTransformType.Lzss => 1,
			NefsDataTransformType.Aes => 4,
			NefsDataTransformType.Zlib => 7,
			_ => throw new NotImplementedException($"Transform type {type} is not implemented"),
		};
	}

	private static NefsDataTransformType GetTransformType(NefsDataTransform transform)
	{
		if (transform.IsZlibCompressed)
		{
			return NefsDataTransformType.Zlib;
		}

		if (transform.IsAesEncrypted)
		{
			return NefsDataTransformType.Aes;
		}

		return transform.IsLzssCompressed ? NefsDataTransformType.Lzss : NefsDataTransformType.None;
	}
}
