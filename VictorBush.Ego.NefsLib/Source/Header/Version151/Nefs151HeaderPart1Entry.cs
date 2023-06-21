// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// An entry in header part 1 for an item in an archive.
/// </summary>
public sealed class Nefs151HeaderPart1Entry : INefsHeaderPartEntry
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151HeaderPart1Entry"/> class.
	/// </summary>
	/// <param name="guid">The Guid of the item this metadata belongs to.</param>
	internal Nefs151HeaderPart1Entry(Guid guid)
	{
		Guid = guid;
	}

	/// <summary>
	/// The unique identifier of the item this data is for.
	/// </summary>
	public Guid Guid { get; init; }

	/// <summary>
	/// The absolute offset to the file's data in the archive. For directories, this is 0.
	/// </summary>
	public ulong OffsetToData
	{
		get => Data_OffsetToData.Value;
		init => Data_OffsetToData.Value = value;
	}

	/// <summary>
	/// Unknown.
	/// </summary>
	public ushort Volume
	{
		get => Data_Volume.Value;
		init => Data_Volume.Value = value;
	}

	/// <summary>
	/// A bitfield that has various flags.
	/// </summary>
	public Nefs16HeaderPart6Flags Flags
	{
		get => (Nefs16HeaderPart6Flags)Data_Flags.Value;
		init => Data_Flags.Value = (byte)value;
	}

	/// <summary>
	/// Unknown.
	/// </summary>
	public byte Unknown0x0B
	{
		get => Data0x0B_Unknown.Value;
		init => Data0x0B_Unknown.Value = value;
	}

	/// <summary>
	/// The index used for parts 2 for this item.
	/// </summary>
	public uint IndexPart2
	{
		get => Data_IndexPart2.Value;
		init => Data_IndexPart2.Value = value;
	}

	/// <summary>
	/// The index into header part 4 for this item. For the actual offset.
	/// </summary>
	public uint IndexPart4
	{
		get => Data_IndexPart4.Value;
		init => Data_IndexPart4.Value = value;
	}

	/// <summary>
	/// The id of the item. It is possible to have duplicate item's with the same id.
	/// </summary>
	public NefsItemId Id
	{
		get => new NefsItemId(Data_Id.Value);
		init => Data_Id.Value = value.Value;
	}

	public int Size => Nefs151HeaderPart1.EntrySize;

	[FileData]
	private UInt64Type Data_OffsetToData { get; } = new(0x00);

	[FileData]
	private UInt16Type Data_Volume { get; } = new(0x08);

	[FileData]
	private UInt8Type Data_Flags { get; } = new(0x0A);

	[FileData]
	private UInt8Type Data0x0B_Unknown { get; } = new(0x0B);

	[FileData]
	private UInt32Type Data_IndexPart2 { get; } = new(0x0C);

	[FileData]
	private UInt32Type Data_IndexPart4 { get; } = new(0x10);

	[FileData]
	private UInt32Type Data_Id { get; } = new(0x14);

	/// <summary>
	/// Creates a <see cref="NefsItemAttributes"/> object.
	/// </summary>
	public NefsItemAttributes CreateAttributes()
	{
		return new NefsItemAttributes(
			v16IsTransformed: Flags.HasFlag(Nefs16HeaderPart6Flags.IsTransformed),
			isDirectory: Flags.HasFlag(Nefs16HeaderPart6Flags.IsDirectory),
			isDuplicated: Flags.HasFlag(Nefs16HeaderPart6Flags.IsDuplicated),
			isCacheable: Flags.HasFlag(Nefs16HeaderPart6Flags.IsCacheable),
			v16Unknown0x10: Flags.HasFlag(Nefs16HeaderPart6Flags.Unknown0x10),
			isPatched: Flags.HasFlag(Nefs16HeaderPart6Flags.IsPatched),
			v16Unknown0x40: Flags.HasFlag(Nefs16HeaderPart6Flags.Unknown0x40),
			v16Unknown0x80: Flags.HasFlag(Nefs16HeaderPart6Flags.Unknown0x80),
			part6Volume: Volume,
			part6Unknown0x3: Unknown0x0B);
	}
}
