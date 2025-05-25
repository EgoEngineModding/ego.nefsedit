// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The primary table of entries.
/// </summary>
public sealed class Nefs150HeaderEntryTable : INefsTocTable<Nefs150TocEntry>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs150TocEntry> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderEntryTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs150HeaderEntryTable(IReadOnlyList<Nefs150TocEntry> entries)
	{
		Entries = entries;
	}

	// /// <summary>
	// /// Initializes a new instance of the <see cref="Nefs150HeaderEntryTable"/> class.
	// /// </summary>
	// /// <param name="items">The list of items in the archive.</param>
	// /// <param name="part4">Header part 4.</param>
	// internal Nefs150HeaderEntryTable(NefsItemList items, INefsHeaderPart4 part4)
	// {
	// 	var entries = new List<Nefs150TocEntry>();
	// 	var indexPart2 = 0U;
	//
	// 	// Enumerate this list depth first. This determines the part 2 order. The part 1 entries will be sorted by item id.
	// 	foreach (var item in items.EnumerateDepthFirstByName())
	// 	{
	// 		var flags = Nefs150TocEntryFlags.None;
	// 		flags |= item.Attributes.V16IsTransformed ? Nefs150TocEntryFlags.Transformed : 0;
	// 		flags |= item.Attributes.IsDirectory ? Nefs150TocEntryFlags.Directory : 0;
	// 		flags |= item.Attributes.IsDuplicated ? Nefs150TocEntryFlags.Duplicated : 0;
	// 		flags |= item.Attributes.IsCacheable ? Nefs150TocEntryFlags.Cacheable : 0;
	// 		flags |= item.Attributes.V16Unknown0x10 ? Nefs150TocEntryFlags.LastSibling : 0;
	// 		flags |= item.Attributes.IsPatched ? Nefs150TocEntryFlags.Patched : 0;
	//
	// 		var entry = new Nefs150TocEntry()
	// 		{
	// 			NextDuplicate = item.Id.Value,
	// 			SharedInfo = indexPart2++,
	// 			FirstBlock = part4.GetIndexForItem(item),
	// 			Start = (ulong)item.DataSource.Offset,
	// 			Volume = item.Attributes.Part6Volume,
	// 			Flags = flags
	// 		};
	//
	// 		entries.Add(entry);
	// 	}
	//
	// 	Entries = entries;
	// }
}
