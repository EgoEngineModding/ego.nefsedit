// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal abstract class NefsHeaderBuilder160Base<T> : NefsHeaderBuilder<T>
	where T : INefsHeader
{
	protected Nefs160HeaderEntryTable BuildEntryTable160(NefsItemList items)
	{
		var idSharedInfoMap = BuildIdSharedInfoMap(items);
		var entries = new Nefs160TocEntry[items.Count];
		var firstBlock = 0u;

		// Enumerate this list depth first. This determines the part 2 order. The part 1 entries will be sorted by item id.
		foreach (var item in items.EnumerateById())
		{
			var entry = new Nefs160TocEntry
			{
				Start = Convert.ToUInt64(item.DataSource.Offset),
				SharedInfo = GetSharedInfo(item),
				FirstBlock = GetFirstBlockLocal(item),
				NextDuplicate = items.GetItemNextDuplicateId(item.Id).Value
			};

			entries[item.Id.Index] = entry;
		}

		return new Nefs160HeaderEntryTable(entries);

		uint GetSharedInfo(NefsItem item)
		{
			return item.IsDuplicate ? idSharedInfoMap[item.FirstDuplicateId] : idSharedInfoMap[item.Id];
		}

		uint GetFirstBlockLocal(NefsItem item)
		{
			return item.IsDuplicate ? entries[item.FirstDuplicateId.Index].FirstBlock : GetFirstBlock(item, ref firstBlock);
		}

		static Dictionary<NefsItemId, uint> BuildIdSharedInfoMap(NefsItemList items)
		{
			return items.EnumerateDepthFirstByName()
				.Where(x => !x.IsDuplicate)
				.Select((x, i) => (x, i))
				.ToDictionary(x => x.x.Id, x => (uint)x.i);
		}
	}

	protected abstract uint GetFirstBlock(NefsItem item, ref uint firstBlock);

	protected static Nefs160HeaderSharedEntryInfoTable BuildSharedEntryInfoTable160(NefsItemList items,
		NefsHeaderPart3 nameTable)
	{
		var entries = new List<Nefs160TocSharedEntryInfo>(items.Count);
		foreach (var item in items.EnumerateDepthFirstByName())
		{
			if (item.IsDuplicate)
			{
				continue;
			}

			var entry = new Nefs160TocSharedEntryInfo
			{
				Parent = item.DirectoryId.Value,
				FirstChild = items.GetItemFirstChildId(item.Id).Value,
				NameOffset = nameTable.OffsetsByFileName[item.FileName],
				Size = item.DataSource.Size.ExtractedSize,
				FirstDuplicate = item.Id.Value
			};

			entries.Add(entry);
		}

		return new Nefs160HeaderSharedEntryInfoTable(entries);
	}

	protected Nefs160HeaderWriteableEntryTable BuildWriteableEntryTable160(NefsItemList items)
	{
		var entries = new Nefs160TocEntryWriteable[items.Count];
		foreach (var item in items.EnumerateById())
		{
			var entry = new Nefs160TocEntryWriteable
			{
				Flags = GetItemFlags(item),
				Volume = item.Attributes.Part6Volume,
			};

			entries[item.Id.Index] = entry;
		}

		return new Nefs160HeaderWriteableEntryTable(entries);
	}

	protected abstract ushort GetItemFlags(NefsItem item);

	protected static Nefs160HeaderWriteableSharedEntryInfoTable BuildWriteableSharedEntryInfoTable160(NefsItemList items)
	{
		var entries = new List<Nefs160TocSharedEntryInfoWriteable>(items.Count);
		foreach (var item in items.EnumerateDepthFirstByName())
		{
			if (item.IsDuplicate)
			{
				continue;
			}

			var entry = new Nefs160TocSharedEntryInfoWriteable
			{
				NextSibling = items.GetItemSiblingId(item.Id).Value,
				PatchedEntry = item.Id.Value
			};

			entries.Add(entry);
		}

		return new Nefs160HeaderWriteableSharedEntryInfoTable(entries);
	}

	protected static Nefs160HeaderHashDigestTable BuildHashDigestTable160(ulong dataSize, uint hashBlockSize)
	{
		var numHashDigests = hashBlockSize == 0 ? 0 : (dataSize + hashBlockSize - 1) / hashBlockSize;
		// Create empty space for now, will update later
		return new Nefs160HeaderHashDigestTable(new Nefs160TocHashDigest[numHashDigests]);
	}

	protected static uint GetTransform(NefsDataTransform transform)
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
