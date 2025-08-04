// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.Header.Version130;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsItemListBuilder130(NefsHeader130 header, ILogger logger)
	: NefsItemListBuilder010Base<NefsHeader130>(header, logger)
{
	/// <inheritdoc />
	internal override NefsItem BuildItem(uint entryIndex, NefsItemList itemList)
	{
		var id = new NefsItemId(entryIndex);
		var entry = Header.EntryTable.Entries[id.Index];
		var link = Header.LinkTable.GetEntryByOffset(entry.LinkOffset);
		return BuildItem(id, entry, link, Header.EntryTable.Entries, itemList);
	}

	/// <inheritdoc />
	protected override (uint End, uint Transformation, ushort Checksum) GetBlock(uint blockIndex)
	{
		var block = Header.BlockTable.Entries[Convert.ToInt32(blockIndex)];
		return (block.End, block.Transformation, 0);
	}
}
