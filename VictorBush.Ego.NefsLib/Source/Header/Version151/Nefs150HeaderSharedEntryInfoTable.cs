// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// The shared entry info table.
/// </summary>
public sealed class Nefs150HeaderSharedEntryInfoTable : INefsTocTable<Nefs150TocSharedEntryInfo>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs150TocSharedEntryInfo> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderSharedEntryInfoTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs150HeaderSharedEntryInfoTable(IReadOnlyList<Nefs150TocSharedEntryInfo> entries)
	{
		Entries = entries;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderSharedEntryInfoTable"/> class.
	/// </summary>
	/// <param name="items">The list of items in the archive.</param>
	/// <param name="part3">Header part 3.</param>
	internal Nefs150HeaderSharedEntryInfoTable(NefsItemList items, NefsHeaderPart3 part3)
	{
		var entries = new List<Nefs150TocSharedEntryInfo>();
		foreach (var item in items.EnumerateDepthFirstByName())
		{
			var entry = new Nefs150TocSharedEntryInfo
			{
				Parent = item.DirectoryId.Value,
				Size = item.DataSource.Size.ExtractedSize,
				FirstChild = items.GetItemFirstChildId(item.Id).Value,
				FirstDuplicate = item.Id.Value,
				PatchedEntry = item.Id.Value,
				NameOffset = part3.OffsetsByFileName[item.FileName],
				NextSibling = items.GetItemSiblingId(item.Id).Value
			};

			entries.Add(entry);
		}

		Entries = entries;
	}
}
