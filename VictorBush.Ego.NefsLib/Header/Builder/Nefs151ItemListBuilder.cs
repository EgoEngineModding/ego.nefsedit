// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class Nefs151ItemListBuilder(Nefs151Header header, ILogger logger) :
	Nefs150ItemListBuilderBase<Nefs151Header>(header, logger)
{
	internal override NefsItem BuildItem(uint entryIndex, NefsItemList dataSourceList)
	{
		var id = new NefsItemId(entryIndex);
		var entry = Header.EntryTable.Entries[id.Index];
		var sharedEntryInfo = Header.SharedEntryInfoTable.Entries[Convert.ToInt32(entry.SharedInfo)];
		return BuildItem(id, entry, sharedEntryInfo, dataSourceList);
	}

	protected override (uint End, uint Transformation) GetBlock(uint blockIndex)
	{
		var block = Header.BlockTable.Entries[Convert.ToInt32(blockIndex)];
		return (block.End, block.Transformation);
	}
}
