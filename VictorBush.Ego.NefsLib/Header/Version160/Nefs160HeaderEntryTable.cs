// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The primary table of entries.
/// </summary>
public sealed class Nefs160HeaderEntryTable : INefsTocTable<Nefs160TocEntry>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs160TocEntry> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs160HeaderEntryTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs160HeaderEntryTable(IReadOnlyList<Nefs160TocEntry> entries)
	{
		Entries = entries;
	}

	// /// <summary>
	// /// Initializes a new instance of the <see cref="Nefs160HeaderEntryTable"/> class.
	// /// </summary>
	// /// <param name="items">The list of items in the archive.</param>
	// /// <param name="part4">Header part 4.</param>
	// internal Nefs160HeaderEntryTable(NefsItemList items, INefsHeaderPart4 part4)
	// {
	// 	var indexPart2 = 0U;
	//
	// 	// Enumerate this list depth first. This determines the part 2 order. The part 1 entries will be sorted by item id.
	// 	foreach (var item in items.EnumerateDepthFirstByName())
	// 	{
	// 		var entry = new Nefs160TocEntry()
	// 		{
	// 			Guid = item.Guid,
	// 			Id = new NefsItemId(item.Id.Value),
	// 			IndexPart2 = indexPart2++,
	// 			IndexPart4 = part4.GetIndexForItem(item),
	// 			OffsetToData = (ulong)item.DataSource.Offset,
	// 		};
	//
	// 		this.entriesByGuid.Add(item.Guid, entry);
	// 	}
	// }
}
