// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// An entry in header part 1 for an item in an archive.
/// </summary>
public sealed class NefsHeaderPart1Entry : INefsHeaderPartEntry
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart1Entry"/> class.
	/// </summary>
	/// <param name="guid">The Guid of the item this metadata belongs to.</param>
	internal NefsHeaderPart1Entry(Guid guid)
	{
		Guid = guid;
	}

	/// <summary>
	/// The unique identifier of the item this data is for.
	/// </summary>
	public Guid Guid { get; init; }

	/// <summary>
	/// The id of the item. It is possible to have duplicate item's with the same id.
	/// </summary>
	public NefsItemId Id
	{
		get => new NefsItemId(Data0x10_Id.Value);
		init => Data0x10_Id.Value = value.Value;
	}

	/// <summary>
	/// The index used for parts 2 and 7 for this item.
	/// </summary>
	public uint IndexPart2
	{
		get => Data0x08_IndexPart2.Value;
		init => Data0x08_IndexPart2.Value = value;
	}

	/// <summary>
	/// The index into header part 4 for this item. For the actual offset, see <see cref="OffsetIntoPart4"/>.
	/// </summary>
	public uint IndexPart4
	{
		get => Data0x0c_IndexPart4.Value;
		init => Data0x0c_IndexPart4.Value = value;
	}

	/// <summary>
	/// The offset into header part 2.
	/// </summary>
	public ulong OffsetIntoPart2 => IndexPart2 * NefsHeaderPart2.EntrySize;

	/// <summary>
	/// The offset into header part 4.
	/// </summary>
	public ulong OffsetIntoPart4 => IndexPart4 * Nefs20HeaderPart4.EntrySize;

	/// <summary>
	/// The offset into header part 6.
	/// </summary>
	public ulong OffsetIntoPart6 => IndexPart2 * Nefs20HeaderPart6.EntrySize;

	/// <summary>
	/// The offset into header part 7.
	/// </summary>
	public ulong OffsetIntoPart7 => IndexPart2 * NefsHeaderPart7.EntrySize;

	/// <summary>
	/// The absolute offset to the file's data in the archive. For directories, this is 0.
	/// </summary>
	public ulong OffsetToData
	{
		get => Data0x00_OffsetToData.Value;
		init => Data0x00_OffsetToData.Value = value;
	}

	public int Size => NefsHeaderPart1.EntrySize;

	[FileData]
	private UInt64Type Data0x00_OffsetToData { get; } = new UInt64Type(0x00);

	[FileData]
	private UInt32Type Data0x08_IndexPart2 { get; } = new UInt32Type(0x08);

	[FileData]
	private UInt32Type Data0x0c_IndexPart4 { get; } = new UInt32Type(0x0c);

	[FileData]
	private UInt32Type Data0x10_Id { get; } = new UInt32Type(0x10);
}
