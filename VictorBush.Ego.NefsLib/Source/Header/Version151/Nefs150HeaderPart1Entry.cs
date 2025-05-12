// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// An entry in header part 1 for an item in an archive.
/// </summary>
public sealed class Nefs150HeaderPart1Entry : INefsHeaderPartEntry
{
	public static readonly int EntrySize = Nefs150TocEntry.ByteCount;
	private readonly Nefs150TocEntry data;

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderPart1Entry"/> class.
	/// </summary>
	/// <param name="guid">The Guid of the item this metadata belongs to.</param>
	/// <param name="data">The underlying data.</param>
	internal Nefs150HeaderPart1Entry(Guid guid, Nefs150TocEntry? data = null)
	{
		Guid = guid;
		this.data = data ?? new Nefs150TocEntry();
	}

	/// <summary>
	/// The unique identifier of the item this data is for.
	/// </summary>
	public Guid Guid { get; init; }

	/// <summary>
	/// The underlying data.
	/// </summary>
	public Nefs150TocEntry Data => this.data;

	/// <summary>
	/// The absolute offset to the file's data in the archive. For directories, this is 0.
	/// </summary>
	public ulong OffsetToData
	{
		get => this.data.Start;
		init => this.data.Start = value;
	}

	/// <summary>
	/// Unknown.
	/// </summary>
	public ushort Volume
	{
		get => this.data.Volume;
		init => this.data.Volume = value;
	}

	/// <summary>
	/// A bitfield that has various flags.
	/// </summary>
	public NefsTocEntryFlags Flags
	{
		get => this.data.Flags;
		init => this.data.Flags = value;
	}

	/// <summary>
	/// The index used for parts 2 for this item.
	/// </summary>
	public uint IndexPart2
	{
		get => this.data.SharedInfo;
		init => this.data.SharedInfo = value;
	}

	/// <summary>
	/// The index into header part 4 for this item. For the actual offset.
	/// </summary>
	public uint IndexPart4
	{
		get => this.data.FirstBlock;
		init => this.data.FirstBlock = value;
	}

	/// <summary>
	/// The id of the item. It is possible to have duplicate item's with the same id.
	/// </summary>
	public NefsItemId Id
	{
		get => new(this.data.NextDuplicate);
		init => this.data.NextDuplicate = value.Value;
	}

	public int Size => EntrySize;

	/// <summary>
	/// Creates a <see cref="NefsItemAttributes"/> object.
	/// </summary>
	public NefsItemAttributes CreateAttributes()
	{
		return new NefsItemAttributes(
			v16IsTransformed: Flags.HasFlag(NefsTocEntryFlags.Transformed),
			isDirectory: Flags.HasFlag(NefsTocEntryFlags.Directory),
			isDuplicated: Flags.HasFlag(NefsTocEntryFlags.Duplicated),
			isCacheable: Flags.HasFlag(NefsTocEntryFlags.Cacheable),
			v16Unknown0x10: Flags.HasFlag(NefsTocEntryFlags.LastSibling),
			isPatched: Flags.HasFlag(NefsTocEntryFlags.Patched),
			v16Unknown0x40: false,
			v16Unknown0x80: false,
			part6Volume: Volume,
			part6Unknown0x3: 0);
	}
}
