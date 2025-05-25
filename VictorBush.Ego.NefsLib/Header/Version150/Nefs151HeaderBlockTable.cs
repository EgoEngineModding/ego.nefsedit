// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The block table.
/// </summary>
public sealed class Nefs151HeaderBlockTable : INefsTocTable<Nefs151TocBlock>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs151TocBlock> Entries { get; }

	/// <summary>
	/// There is a 4-byte value at the end of header part 4. Purpose unknown.
	/// </summary>
	public uint UnkownEndValue { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151HeaderBlockTable"/> class.
	/// </summary>
	/// <param name="entries">A collection of entries to initialize this object with.</param>
	/// <param name="unkownEndValue">Last four bytes of part 4.</param>
	internal Nefs151HeaderBlockTable(IReadOnlyList<Nefs151TocBlock> entries, uint unkownEndValue)
	{
		Entries = entries;
		UnkownEndValue = unkownEndValue;
	}

	// /// <summary>
	// /// Initializes a new instance of the <see cref="Nefs16HeaderPart4"/> class from a list of items.
	// /// </summary>
	// /// <param name="items">The items to initialize from.</param>
	// /// <param name="unkownEndValue">Last four bytes of part 4.</param>
	// internal Nefs151HeaderBlockTable(NefsItemList items, uint unkownEndValue)
	// {
	// 	this.indexLookup = new Dictionary<Guid, uint>();
	// 	UnkownEndValue = unkownEndValue;
	//
	// 	var nextStartIdx = 0U;
	//
	// 	foreach (var item in items.EnumerateById())
	// 	{
	// 		if (item.Type == NefsItemType.Directory || item.DataSource.Size.Chunks.Count == 0)
	// 		{
	// 			// Item does not have a part 4 entry
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
	// 			var entry = new Nefs151TocBlock
	// 			{
	// 				Checksum = 0x848, // TODO - How to compute this value is unknown. Writing bogus data for now.
	// 				End = chunk.CumulativeSize,
	// 				Transformation = GetTransformType(chunk.Transform),
	// 			};
	// 			Entries.Add(entry);
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
	// 	else
	// 	{
	// 		// Get index into part 4
	// 		return this.indexLookup[item.Guid];
	// 	}
	// }
	//
	// private NefsDataTransformType GetTransformType(NefsDataTransform transform)
	// {
	// 	// Can have both aes and zlib simulatneously?
	// 	if (transform.IsAesEncrypted && transform.IsZlibCompressed)
	// 	{
	// 		Log.LogWarning("Found multiple data transforms for header part 4 entry.");
	// 	}
	//
	// 	if (transform.IsAesEncrypted)
	// 	{
	// 		return NefsDataTransformType.Aes;
	// 	}
	// 	else if (transform.IsZlibCompressed)
	// 	{
	// 		return NefsDataTransformType.Zlib;
	// 	}
	// 	else if (transform.IsLzssCompressed)
	// 	{
	// 		return NefsDataTransformType.Lzss;
	// 	}
	//
	// 	return NefsDataTransformType.None;
	// }
}
