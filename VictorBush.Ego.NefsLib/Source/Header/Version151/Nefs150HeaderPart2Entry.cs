// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// An entry in header part 2 for an item in an archive.
/// </summary>
public sealed class Nefs150HeaderPart2Entry : INefsHeaderPartEntry
{
	public static readonly int EntrySize = Nefs150TocSharedEntryInfo.ByteCount;
	private readonly Nefs150TocSharedEntryInfo data;

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderPart2Entry"/> class.
	/// </summary>
	/// <param name="data">The underlying data.</param>
	internal Nefs150HeaderPart2Entry(Nefs150TocSharedEntryInfo? data = null)
	{
		this.data = data ?? new Nefs150TocSharedEntryInfo();
	}

	/// <summary>
	/// The underlying data.
	/// </summary>
	public Nefs150TocSharedEntryInfo Data => this.data;

	/// <summary>
	/// The id of the directory this item belongs to.
	/// </summary>
	public NefsItemId DirectoryId
	{
		get => new(this.data.Parent);
		init => this.data.Parent = value.Value;
	}

	/// <summary>
	/// Gets the id of the next item in the same directory as this item. If this item is the last item in the directory,
	/// the sibling id will equal the item id.
	/// </summary>
	public NefsItemId SiblingId
	{
		get => new(this.data.NextSibling);
		init => this.data.NextSibling = value.Value;
	}

	/// <summary>
	/// Id of the first child of this item.
	/// - If the first child id matches this item's id, then there are no children.
	/// - If this item is a file, there won't be any children (only directories can have children).
	/// </summary>
	public NefsItemId FirstChildId
	{
		get => new(this.data.FirstChild);
		init => this.data.FirstChild = value.Value;
	}

	/// <summary>
	/// Offset into header part 3 (file strings table) for the name of this item.
	/// </summary>
	public uint OffsetIntoPart3
	{
		get => this.data.NameOffset;
		init => this.data.NameOffset = value;
	}

	/// <summary>
	/// Extracted size of this item.
	/// </summary>
	public uint ExtractedSize
	{
		get => this.data.Size;
		init => this.data.Size = value;
	}

	/// <summary>
	/// The id of the item. For duplicate items, this may not correspond to an entry in part 1. In such a case, there
	/// may be multiple part 1 entries that share a part 2 entry.
	/// </summary>
	public NefsItemId Id
	{
		get => new(this.data.FirstDuplicate);
		init => this.data.FirstDuplicate = value.Value;
	}

	/// <summary>
	/// So far same as other id. TODO: figure out
	/// </summary>
	public NefsItemId Id2
	{
		get => new(this.data.PatchedEntry);
		init => this.data.PatchedEntry = value.Value;
	}

	public int Size => EntrySize;
}
