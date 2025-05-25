// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// The shared entry info table.
/// </summary>
public sealed class Nefs160HeaderSharedEntryInfoTable : INefsTocTable<Nefs160TocSharedEntryInfo>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs160TocSharedEntryInfo> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs160HeaderSharedEntryInfoTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs160HeaderSharedEntryInfoTable(IReadOnlyList<Nefs160TocSharedEntryInfo> entries)
	{
		Entries = entries;
	}

	// /// <summary>
	// /// Initializes a new instance of the <see cref="Nefs160HeaderSharedEntryInfoTable"/> class.
	// /// </summary>
	// /// <param name="items">The list of items in the archive.</param>
	// /// <param name="part3">Header part 3.</param>
	// internal Nefs160HeaderSharedEntryInfoTable(NefsItemList items, NefsHeaderPart3 part3)
	// {
	// 	this.entriesByIndex = new List<Nefs160TocSharedEntryInfo>();
	//
	// 	foreach (var item in items.EnumerateDepthFirstByName())
	// 	{
	// 		var entry = new Nefs160TocSharedEntryInfo
	// 		{
	// 			DirectoryId = item.DirectoryId,
	// 			ExtractedSize = item.DataSource.Size.ExtractedSize,
	// 			FirstChildId = items.GetItemFirstChildId(item.Id),
	// 			Id = item.Id,
	// 			OffsetIntoPart3 = part3.OffsetsByFileName[item.FileName],
	// 		};
	//
	// 		this.entriesByIndex.Add(entry);
	// 	}
	// }
}
