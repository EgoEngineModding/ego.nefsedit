// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header part 2.
/// </summary>
public sealed class NefsHeaderPart2
{
	/// <summary>
	/// The size of a part 2 entry. This is used to get the offset into part 2 from an index into part 2.
	/// </summary>
	public const int EntrySize = 0x14;

	private readonly List<NefsHeaderPart2Entry> entriesByIndex;

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart2"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal NefsHeaderPart2(IList<NefsHeaderPart2Entry> entries)
	{
		this.entriesByIndex = new List<NefsHeaderPart2Entry>(entries);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart2"/> class.
	/// </summary>
	/// <param name="items">The list of items in the archive.</param>
	/// <param name="part3">Header part 3.</param>
	internal NefsHeaderPart2(NefsItemList items, NefsHeaderPart3 part3)
	{
		this.entriesByIndex = new List<NefsHeaderPart2Entry>();

		foreach (var item in items.EnumerateDepthFirstByName())
		{
			var entry = new NefsHeaderPart2Entry
			{
				DirectoryId = item.DirectoryId,
				ExtractedSize = item.DataSource.Size.ExtractedSize,
				FirstChildId = items.GetItemFirstChildId(item.Id),
				Id = item.Id,
				OffsetIntoPart3 = part3.OffsetsByFileName[item.FileName],
			};

			this.entriesByIndex.Add(entry);
		}
	}

	/// <summary>
	/// Gets the list of entries in the order they appear in the header.
	/// </summary>
	public IList<NefsHeaderPart2Entry> EntriesByIndex => this.entriesByIndex;

	/// <summary>
	/// Total size (in bytes) of part 2.
	/// </summary>
	public int Size => this.entriesByIndex.Count * EntrySize;
}
