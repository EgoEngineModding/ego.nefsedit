// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// Header part 1. The "master catalog" of items in the archive.
/// </summary>
public sealed class Nefs151HeaderPart1
{
	/// <summary>
	/// The size of a part 1 entry.
	/// </summary>
	public const int EntrySize = 0x18;

	private readonly Dictionary<Guid, Nefs151HeaderPart1Entry> entriesByGuid;
	private readonly List<Nefs151HeaderPart1Entry> entriesByIndex;

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151HeaderPart1"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs151HeaderPart1(IList<Nefs151HeaderPart1Entry> entries)
	{
		this.entriesByIndex = new List<Nefs151HeaderPart1Entry>(entries);
		this.entriesByGuid = new Dictionary<Guid, Nefs151HeaderPart1Entry>(entries.ToDictionary(e => e.Guid));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151HeaderPart1"/> class.
	/// </summary>
	/// <param name="items">The list of items in the archive.</param>
	/// <param name="part4">Header part 4.</param>
	internal Nefs151HeaderPart1(NefsItemList items, INefsHeaderPart4 part4)
	{
		this.entriesByGuid = new Dictionary<Guid, Nefs151HeaderPart1Entry>();
		var indexPart2 = 0U;

		// Enumerate this list depth first. This determines the part 2 order. The part 1 entries will be sorted by item id.
		foreach (var item in items.EnumerateDepthFirstByName())
		{
			var flags = Nefs16HeaderPart6Flags.None;
			flags |= item.Attributes.V16IsTransformed ? Nefs16HeaderPart6Flags.IsTransformed : 0;
			flags |= item.Attributes.IsDirectory ? Nefs16HeaderPart6Flags.IsDirectory : 0;
			flags |= item.Attributes.IsDuplicated ? Nefs16HeaderPart6Flags.IsDuplicated : 0;
			flags |= item.Attributes.IsCacheable ? Nefs16HeaderPart6Flags.IsCacheable : 0;
			flags |= item.Attributes.V16Unknown0x10 ? Nefs16HeaderPart6Flags.Unknown0x10 : 0;
			flags |= item.Attributes.IsPatched ? Nefs16HeaderPart6Flags.IsPatched : 0;
			flags |= item.Attributes.V16Unknown0x40 ? Nefs16HeaderPart6Flags.Unknown0x40 : 0;
			flags |= item.Attributes.V16Unknown0x80 ? Nefs16HeaderPart6Flags.Unknown0x80 : 0;

			var entry = new Nefs151HeaderPart1Entry(item.Guid)
			{
				Guid = item.Guid,
				Id = new NefsItemId(item.Id.Value),
				IndexPart2 = indexPart2++,
				IndexPart4 = part4.GetIndexForItem(item),
				OffsetToData = (ulong)item.DataSource.Offset,
				Volume = item.Attributes.Part6Volume,
				Flags = flags,
				Unknown0x0B = item.Attributes.Part6Unknown0x3
			};

			this.entriesByGuid.Add(item.Guid, entry);
		}

		// Sort part 1 by item id
		this.entriesByIndex = new List<Nefs151HeaderPart1Entry>(this.entriesByGuid.Values.OrderBy(e => e.Id));
	}

	/// <summary>
	/// Gets entries for each item in the archive, accessible by Guid.
	/// </summary>
	public IReadOnlyDictionary<Guid, Nefs151HeaderPart1Entry> EntriesByGuid => this.entriesByGuid;

	/// <summary>
	/// Gets the list of entries in the order they appear in the header. Usually items are sorted by id, but this is not
	/// guaranteed (for example, DiRT Rally has a header with items out of order).
	/// </summary>
	public IList<Nefs151HeaderPart1Entry> EntriesByIndex => this.entriesByIndex;

	/// <summary>
	/// Total size (in bytes) of part 1.
	/// </summary>
	public int Size => this.entriesByIndex.Count * EntrySize;
}
