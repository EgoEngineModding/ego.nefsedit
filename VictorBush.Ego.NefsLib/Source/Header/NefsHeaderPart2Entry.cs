// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// An entry in header part 2 for an item in an archive.
/// </summary>
public sealed class NefsHeaderPart2Entry : INefsHeaderPartEntry
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart2Entry"/> class.
	/// </summary>
	internal NefsHeaderPart2Entry()
	{
	}

	/// <summary>
	/// The id of the directory this item belongs to.
	/// </summary>
	public NefsItemId DirectoryId
	{
		get => new NefsItemId(Data0x00_DirectoryId.Value);
		init => Data0x00_DirectoryId.Value = value.Value;
	}

	/// <summary>
	/// Extracted sisze of this item.
	/// </summary>
	public uint ExtractedSize
	{
		get => Data0x0c_ExtractedSize.Value;
		init => Data0x0c_ExtractedSize.Value = value;
	}

	/// <summary>
	/// Id of the first child of this item.
	/// - If the first child id matches this item's id, then there are no children.
	/// - If this item is a file, there won't be any children (only directories can have children).
	/// </summary>
	public NefsItemId FirstChildId
	{
		get => new NefsItemId(Data0x04_FirstChildId.Value);
		init => Data0x04_FirstChildId.Value = value.Value;
	}

	/// <summary>
	/// The id of the item. For duplicate items, this may not correspond to an entry in part 1. In such a case, there
	/// may be multiple part 1 entries that share a part 2 entry.
	/// </summary>
	public NefsItemId Id
	{
		get => new NefsItemId(Data0x10_Id.Value);
		init => Data0x10_Id.Value = value.Value;
	}

	/// <summary>
	/// Offset into header part 3 (file strings table) for the name of this item.
	/// </summary>
	public uint OffsetIntoPart3
	{
		get => Data0x08_OffsetIntoPart3.Value;
		init => Data0x08_OffsetIntoPart3.Value = value;
	}

	public int Size => NefsHeaderPart2.EntrySize;

	[FileData]
	private UInt32Type Data0x00_DirectoryId { get; } = new UInt32Type(0x00);

	[FileData]
	private UInt32Type Data0x04_FirstChildId { get; } = new UInt32Type(0x04);

	[FileData]
	private UInt32Type Data0x08_OffsetIntoPart3 { get; } = new UInt32Type(0x08);

	[FileData]
	private UInt32Type Data0x0c_ExtractedSize { get; } = new UInt32Type(0x0c);

	[FileData]
	private UInt32Type Data0x10_Id { get; } = new UInt32Type(0x10);
}
