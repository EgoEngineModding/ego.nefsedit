// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version020;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsItemListBuilder020(NefsHeader020 header, ILogger logger)
	: NefsItemListBuilder010Base<NefsHeader020>(header, logger)
{
	/// <inheritdoc />
	internal override NefsItem BuildItem(uint entryIndex, NefsVolumeSource volume)
	{
		var id = new NefsItemId(entryIndex);
		var entry = Header.EntryTable.Entries[id.Index];
		var link = Header.LinkTable.GetEntryByOffset(entry.LinkOffset);
		return BuildItem(id, entry, link, Header.EntryTable.Entries, volume);
	}

	protected override (uint End, uint Transformation) GetBlock(uint blockIndex)
	{
		var block = Header.BlockTable.Entries[Convert.ToInt32(blockIndex)];
		return (block.End, block.Transformation);
	}
}
