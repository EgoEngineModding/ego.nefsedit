// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header part 7.
/// </summary>
public sealed class NefsHeaderPart7
{
	/// <summary>
	/// The size of a part 7 entry.
	/// </summary>
	public const int EntrySize = 0x8;

	private readonly List<NefsHeaderPart7Entry> entriesByIndex;

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart7"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal NefsHeaderPart7(IList<NefsHeaderPart7Entry> entries)
	{
		this.entriesByIndex = new List<NefsHeaderPart7Entry>(entries);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart7"/> class from a list of items.
	/// </summary>
	/// <param name="items">The list of items in the archive.</param>
	internal NefsHeaderPart7(NefsItemList items)
	{
		this.entriesByIndex = new List<NefsHeaderPart7Entry>();

		foreach (var item in items.EnumerateDepthFirstByName())
		{
			var entry = new NefsHeaderPart7Entry
			{
				Id = item.Id,
				SiblingId = items.GetItemSiblingId(item.Id),
			};

			this.entriesByIndex.Add(entry);
		}
	}

	/// <summary>
	/// Gets the list of entries in the order they appear in the header.
	/// </summary>
	public IList<NefsHeaderPart7Entry> EntriesByIndex => this.entriesByIndex;

	/// <summary>
	/// Total size (in bytes) of part 7.
	/// </summary>
	public int Size => this.entriesByIndex.Count * EntrySize;
}
