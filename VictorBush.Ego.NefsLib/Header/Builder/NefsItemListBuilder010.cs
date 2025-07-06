// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal class NefsItemListBuilder010(NefsHeader010 header, ILogger logger)
	: NefsItemListBuilder010Base<NefsHeader010>(header, logger)
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
	protected override (uint End, uint Transformation) GetBlock(uint blockIndex)
	{
		var block = Header.BlockTable.Entries[Convert.ToInt32(blockIndex)];
		return (block.End, block.Transformation);
	}

	/// <inheritdoc />
	protected override NefsDataTransformType GetTransformType(uint blockTransformation)
	{
		return blockTransformation switch
		{
			0 => NefsDataTransformType.None,
			1 => NefsDataTransformType.Lzss,
			_ => (NefsDataTransformType)(-1)
		};
	}
}
