// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The writeable shared entry info table.
/// </summary>
public sealed class Nefs160HeaderWriteableSharedEntryInfo : INefsTocTable<Nefs160TocSharedEntryInfoWriteable>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs160TocSharedEntryInfoWriteable> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs160HeaderWriteableSharedEntryInfo"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs160HeaderWriteableSharedEntryInfo(IReadOnlyList<Nefs160TocSharedEntryInfoWriteable> entries)
	{
		Entries = entries;
	}

	// /// <summary>
	// /// Initializes a new instance of the <see cref="Nefs160HeaderWriteableSharedEntryInfo"/> class from a list of items.
	// /// </summary>
	// /// <param name="items">The list of items in the archive.</param>
	// internal Nefs160HeaderWriteableSharedEntryInfo(NefsItemList items)
	// {
	// 	this.entriesByIndex = new List<Nefs160TocSharedEntryInfoWriteable>();
	//
	// 	foreach (var item in items.EnumerateDepthFirstByName())
	// 	{
	// 		var entry = new Nefs160TocSharedEntryInfoWriteable
	// 		{
	// 			Id = item.Id,
	// 			SiblingId = items.GetItemSiblingId(item.Id),
	// 		};
	//
	// 		this.entriesByIndex.Add(entry);
	// 	}
	// }
}
