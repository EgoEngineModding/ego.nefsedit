// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The shared entry info table.
/// </summary>
public sealed class NefsHeaderSharedEntryInfoTable150 : INefsTocTable<NefsTocSharedEntryInfo150>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocSharedEntryInfo150> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderSharedEntryInfoTable150"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal NefsHeaderSharedEntryInfoTable150(IReadOnlyList<NefsTocSharedEntryInfo150> entries)
	{
		Entries = entries;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderSharedEntryInfoTable150"/> class.
	/// </summary>
	/// <param name="items">The list of items in the archive.</param>
	/// <param name="part3">Header part 3.</param>
	internal NefsHeaderSharedEntryInfoTable150(NefsItemList items, NefsHeaderPart3 part3)
	{
		var entries = new List<NefsTocSharedEntryInfo150>();
		foreach (var item in items.EnumerateDepthFirstByName())
		{
			var entry = new NefsTocSharedEntryInfo150
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
