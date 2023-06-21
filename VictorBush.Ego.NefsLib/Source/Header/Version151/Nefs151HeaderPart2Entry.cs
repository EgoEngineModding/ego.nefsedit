// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// An entry in header part 2 for an item in an archive.
/// </summary>
public sealed class Nefs151HeaderPart2Entry : INefsHeaderPartEntry
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151HeaderPart2Entry"/> class.
	/// </summary>
	internal Nefs151HeaderPart2Entry()
	{
	}

	/// <summary>
	/// The id of the directory this item belongs to.
	/// </summary>
	public NefsItemId DirectoryId
	{
		get => new NefsItemId(Data_DirectoryId.Value);
		init => Data_DirectoryId.Value = value.Value;
	}

	/// <summary>
	/// Gets the id of the next item in the same directory as this item. If this item is the last item in the directory,
	/// the sibling id will equal the item id.
	/// </summary>
	public NefsItemId SiblingId
	{
		get => new NefsItemId(Data_SiblingId.Value);
		init => Data_SiblingId.Value = value.Value;
	}

	/// <summary>
	/// Id of the first child of this item.
	/// - If the first child id matches this item's id, then there are no children.
	/// - If this item is a file, there won't be any children (only directories can have children).
	/// </summary>
	public NefsItemId FirstChildId
	{
		get => new NefsItemId(Data_FirstChildId.Value);
		init => Data_FirstChildId.Value = value.Value;
	}

	/// <summary>
	/// Offset into header part 3 (file strings table) for the name of this item.
	/// </summary>
	public uint OffsetIntoPart3
	{
		get => Data_OffsetIntoPart3.Value;
		init => Data_OffsetIntoPart3.Value = value;
	}

	/// <summary>
	/// Extracted size of this item.
	/// </summary>
	public uint ExtractedSize
	{
		get => Data_ExtractedSize.Value;
		init => Data_ExtractedSize.Value = value;
	}

	/// <summary>
	/// The id of the item. For duplicate items, this may not correspond to an entry in part 1. In such a case, there
	/// may be multiple part 1 entries that share a part 2 entry.
	/// </summary>
	public NefsItemId Id
	{
		get => new NefsItemId(Data_Id.Value);
		init => Data_Id.Value = value.Value;
	}

	/// <summary>
	/// So far same as other id. TODO: figure out
	/// </summary>
	public NefsItemId Id2
	{
		get => new NefsItemId(Data_Id2.Value);
		init => Data_Id2.Value = value.Value;
	}

	public int Size => Nefs151HeaderPart2.EntrySize;

	[FileData]
	private UInt32Type Data_DirectoryId { get; } = new(0x00);

	[FileData]
	private UInt32Type Data_SiblingId { get; } = new(0x04);

	[FileData]
	private UInt32Type Data_FirstChildId { get; } = new(0x08);

	[FileData]
	private UInt32Type Data_OffsetIntoPart3 { get; } = new(0x0C);

	[FileData]
	private UInt32Type Data_ExtractedSize { get; } = new(0x10);

	[FileData]
	private UInt32Type Data_Id { get; } = new(0x14);

	[FileData]
	private UInt32Type Data_Id2 { get; } = new(0x18);
}
