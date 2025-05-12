// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// Header part 2.
/// </summary>
public sealed class Nefs150HeaderPart2
{
	private readonly List<Nefs150HeaderPart2Entry> entriesByIndex;

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderPart2"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs150HeaderPart2(IList<Nefs150HeaderPart2Entry> entries)
	{
		this.entriesByIndex = new List<Nefs150HeaderPart2Entry>(entries);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderPart2"/> class.
	/// </summary>
	/// <param name="items">The list of items in the archive.</param>
	/// <param name="part3">Header part 3.</param>
	internal Nefs150HeaderPart2(NefsItemList items, NefsHeaderPart3 part3)
	{
		this.entriesByIndex = new List<Nefs150HeaderPart2Entry>();

		foreach (var item in items.EnumerateDepthFirstByName())
		{
			var entry = new Nefs150HeaderPart2Entry
			{
				DirectoryId = item.DirectoryId,
				ExtractedSize = item.DataSource.Size.ExtractedSize,
				FirstChildId = items.GetItemFirstChildId(item.Id),
				Id = item.Id,
				Id2 = item.Id,
				OffsetIntoPart3 = part3.OffsetsByFileName[item.FileName],
				SiblingId = items.GetItemSiblingId(item.Id)
			};

			this.entriesByIndex.Add(entry);
		}
	}

	/// <summary>
	/// Gets the list of entries in the order they appear in the header.
	/// </summary>
	public IList<Nefs150HeaderPart2Entry> EntriesByIndex => this.entriesByIndex;

	/// <summary>
	/// Total size (in bytes) of part 2.
	/// </summary>
	public int Size => this.entriesByIndex.Count * Nefs150HeaderPart2Entry.EntrySize;
}
