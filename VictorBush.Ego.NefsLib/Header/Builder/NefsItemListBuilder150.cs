// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsItemListBuilder150(NefsHeader150 header, ILogger logger)
	: NefsItemListBuilder150Base<NefsHeader150>(header, logger)
{
	/// <inheritdoc />
	internal override NefsItem BuildItem(uint entryIndex, NefsItemList itemList)
	{
		var id = new NefsItemId(entryIndex);
		var entry = Header.EntryTable.Entries[id.Index];
		var sharedEntryInfo = Header.SharedEntryInfoTable.Entries[Convert.ToInt32(entry.SharedInfo)];
		return BuildItem(id, entry, sharedEntryInfo, itemList);
	}

	/// <inheritdoc />
	protected override (uint End, uint Transformation, ushort Checksum) GetBlock(uint blockIndex)
	{
		var block = Header.BlockTable.Entries[Convert.ToInt32(blockIndex)];
		return (block.End, block.Transformation, 0);
	}
}
