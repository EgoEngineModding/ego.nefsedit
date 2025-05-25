// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The writable entry table.
/// </summary>
public sealed class Nefs160HeaderWriteableEntryTable : INefsTocTable<Nefs160TocEntryWriteable>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs160TocEntryWriteable> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs160HeaderWriteableEntryTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs160HeaderWriteableEntryTable(IReadOnlyList<Nefs160TocEntryWriteable> entries)
	{
		Entries = entries;
	}

	// /// <summary>
	// /// Initializes a new instance of the <see cref="Nefs160HeaderWriteableEntryTable"/> class from a list of items.
	// /// </summary>
	// /// <param name="items">The list of items in the archive.</param>
	// internal Nefs160HeaderWriteableEntryTable(NefsItemList items)
	// {
	// 	this.entriesByIndex = new List<Nefs160TocEntryWriteable>();
	//
	// 	// Sort part 6 by item id. Part 1 and part 6 order must match.
	// 	foreach (var item in items.EnumerateById())
	// 	{
	// 		var flags = Nefs160HeaderPart6Flags.None;
	// 		flags |= item.Attributes.V16IsTransformed ? Nefs160HeaderPart6Flags.IsTransformed : 0;
	// 		flags |= item.Attributes.IsDirectory ? Nefs160HeaderPart6Flags.IsDirectory : 0;
	// 		flags |= item.Attributes.IsDuplicated ? Nefs160HeaderPart6Flags.IsDuplicated : 0;
	// 		flags |= item.Attributes.IsCacheable ? Nefs160HeaderPart6Flags.IsCacheable : 0;
	// 		flags |= item.Attributes.V16Unknown0x10 ? Nefs160HeaderPart6Flags.Unknown0x10 : 0;
	// 		flags |= item.Attributes.IsPatched ? Nefs160HeaderPart6Flags.IsPatched : 0;
	// 		flags |= item.Attributes.V16Unknown0x40 ? Nefs160HeaderPart6Flags.Unknown0x40 : 0;
	// 		flags |= item.Attributes.V16Unknown0x80 ? Nefs160HeaderPart6Flags.Unknown0x80 : 0;
	//
	// 		var entry = new Nefs160TocEntryWriteable
	// 		{
	// 			Flags = flags,
	// 			Volume = item.Attributes.Part6Volume,
	// 		};
	//
	// 		this.entriesByIndex.Add(entry);
	// 	}
	// }
}
