// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal abstract class NefsHeaderBuilder160Base<T> : NefsHeaderBuilder<T>
	where T : INefsHeader
{
	protected NefsHeaderEntryTable160 BuildEntryTable160(NefsItemList items)
	{
		var idSharedInfoMap = NefsHeaderBuilder151.BuildIdSharedInfoMap(items);
		var entries = new NefsTocEntry160[items.Count];
		var firstBlock = 0u;

		// Enumerate items sorted by id.
		foreach (var item in items.EnumerateById())
		{
			var entry = new NefsTocEntry160
			{
				Start = Convert.ToUInt64(item.DataSource.Offset),
				SharedInfo = GetSharedInfo(item),
				FirstBlock = GetFirstBlockLocal(item),
				NextDuplicate = items.GetItemNextDuplicateId(item.Id).Value
			};

			entries[item.Id.Index] = entry;
		}

		return new NefsHeaderEntryTable160(entries);

		uint GetSharedInfo(NefsItem item)
		{
			return item.IsDuplicate ? idSharedInfoMap[item.FirstDuplicateId] : idSharedInfoMap[item.Id];
		}

		uint GetFirstBlockLocal(NefsItem item)
		{
			return item.IsDuplicate ? entries[item.FirstDuplicateId.Index].FirstBlock : GetFirstBlock(item, ref firstBlock);
		}
	}

	protected abstract uint GetFirstBlock(NefsItem item, ref uint firstBlock);

	protected static NefsHeaderSharedEntryInfoTable160 BuildSharedEntryInfoTable160(NefsItemList items,
		NefsHeaderNameTable nameTable)
	{
		var entries = new List<NefsTocSharedEntryInfo160>(items.Count);
		foreach (var item in items.EnumerateDepthFirstByName())
		{
			if (item.IsDuplicate)
			{
				continue;
			}

			var entry = new NefsTocSharedEntryInfo160
			{
				Parent = item.DirectoryId.Value,
				FirstChild = items.GetItemFirstChildId(item.Id).Value,
				NameOffset = nameTable.OffsetsByFileName[item.FileName],
				Size = item.DataSource.Size.ExtractedSize,
				FirstDuplicate = item.Id.Value
			};

			entries.Add(entry);
		}

		return new NefsHeaderSharedEntryInfoTable160(entries);
	}

	protected NefsHeaderWriteableEntryTable160 BuildWriteableEntryTable160(NefsItemList items)
	{
		var entries = new NefsTocEntryWriteable160[items.Count];
		foreach (var item in items.EnumerateById())
		{
			var entry = new NefsTocEntryWriteable160
			{
				Flags = GetItemFlags(item),
				Volume = item.Attributes.Volume,
			};

			entries[item.Id.Index] = entry;
		}

		return new NefsHeaderWriteableEntryTable160(entries);
	}

	protected abstract ushort GetItemFlags(NefsItem item);

	protected static NefsHeaderWriteableSharedEntryInfoTable160 BuildWriteableSharedEntryInfoTable160(NefsItemList items)
	{
		var entries = new List<NefsTocSharedEntryInfoWriteable160>(items.Count);
		foreach (var item in items.EnumerateDepthFirstByName())
		{
			if (item.IsDuplicate)
			{
				continue;
			}

			var entry = new NefsTocSharedEntryInfoWriteable160
			{
				NextSibling = items.GetItemSiblingId(item.Id).Value,
				PatchedEntry = item.Id.Value
			};

			entries.Add(entry);
		}

		return new NefsHeaderWriteableSharedEntryInfoTable160(entries);
	}

	protected static NefsHeaderHashDigestTable160 BuildHashDigestTable160(ulong dataSize, uint hashBlockSize)
	{
		var numHashDigests = hashBlockSize == 0 ? 0 : (dataSize + hashBlockSize - 1) / hashBlockSize;
		// Create empty space for now, will update later
		return new NefsHeaderHashDigestTable160(new NefsTocHashDigest160[numHashDigests]);
	}
}
