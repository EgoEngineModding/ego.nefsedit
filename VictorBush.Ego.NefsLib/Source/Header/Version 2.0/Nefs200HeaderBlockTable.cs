// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// The block table.
/// </summary>
public sealed class Nefs200HeaderBlockTable : INefsTocTable<Nefs200TocBlock>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs200TocBlock> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs200HeaderBlockTable"/> class.
	/// </summary>
	internal Nefs200HeaderBlockTable(IReadOnlyList<Nefs200TocBlock> entries)
	{
		Entries = entries;
	}

	// /// <summary>
	// /// Initializes a new instance of the <see cref="Nefs200HeaderBlockTable"/> class from a list of items.
	// /// </summary>
	// /// <param name="items">The items to initialize from.</param>
	// internal Nefs200HeaderBlockTable(NefsItemList items)
	// {
	// 	this.entriesByIndex = new List<Nefs20HeaderPart4Entry>();
	//
	// 	var nextStartIdx = 0U;
	//
	// 	foreach (var item in items.EnumerateById())
	// 	{
	// 		if (item.DataSource.Size.ExtractedSize == item.DataSource.Size.TransformedSize)
	// 		{
	// 			// Item does not have a part 4 entry since it has no compressed data
	// 			continue;
	// 		}
	//
	// 		// Log this start index to item's Guid to allow lookup later
	// 		this.indexLookup.Add(item.Guid, nextStartIdx);
	//
	// 		// Create entry for each data chunk
	// 		foreach (var chunk in item.DataSource.Size.Chunks)
	// 		{
	// 			// Create entry
	// 			var entry = new Nefs20HeaderPart4Entry
	// 			{
	// 				CumulativeChunkSize = chunk.CumulativeSize,
	// 			};
	// 			this.entriesByIndex.Add(entry);
	//
	// 			nextStartIdx++;
	// 		}
	// 	}
	// }
	//
	// /// <inheritdoc/>
	// public uint GetIndexForItem(NefsItem item)
	// {
	// 	// Get index to part 4
	// 	if (item.Type == NefsItemType.Directory)
	// 	{
	// 		// Item is a directory; the index 0
	// 		return 0;
	// 	}
	// 	else if (item.ExtractedSize == item.CompressedSize)
	// 	{
	// 		// Item is uncompressed; the index is -1 (0xFFFFFFFF)
	// 		return 0xFFFFFFFF;
	// 	}
	// 	else
	// 	{
	// 		// Item is compressed; get index into part 4
	// 		return this.indexLookup[item.Guid];
	// 	}
	// }
}
