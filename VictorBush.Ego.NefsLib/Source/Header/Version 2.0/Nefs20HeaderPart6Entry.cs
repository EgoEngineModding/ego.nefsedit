// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// An entry in header part 6 for an item in an archive.
/// </summary>
public sealed class Nefs20HeaderPart6Entry : INefsHeaderPartEntry
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs20HeaderPart6Entry"/> class.
	/// </summary>
	/// <param name="guid">The Guid of the item this metadata belongs to.</param>
	public Nefs20HeaderPart6Entry(Guid guid)
	{
		Guid = guid;
	}

	/// <summary>
	/// A bitfield that has various flags.
	/// </summary>
	public Nefs20HeaderPart6Flags Flags
	{
		get => (Nefs20HeaderPart6Flags)Data0x02_Flags.Value;
		init => Data0x02_Flags.Value = (byte)value;
	}

	/// <summary>
	/// The unique identifier of the item this data is for.
	/// </summary>
	public Guid Guid { get; }

	/// <summary>
	/// The size of a part 6 entry.
	/// </summary>
	public int Size => Nefs20HeaderPart6.EntrySize;

	/// <summary>
	/// Unknown.
	/// </summary>
	public byte Unknown0x3
	{
		get => Data0x03_Unknown.Value;
		init => Data0x03_Unknown.Value = value;
	}

	/// <summary>
	/// Unknown.
	/// </summary>
	public ushort Volume
	{
		get => Data0x00_Volume.Value;
		init => Data0x00_Volume.Value = value;
	}

	[FileData]
	private UInt16Type Data0x00_Volume { get; } = new UInt16Type(0x00);

	[FileData]
	private UInt8Type Data0x02_Flags { get; } = new UInt8Type(0x02);

	[FileData]
	private UInt8Type Data0x03_Unknown { get; } = new UInt8Type(0x03);

	/// <summary>
	/// Creates a <see cref="NefsItemAttributes"/> object.
	/// </summary>
	/// <returns>The attributes.</returns>
	public NefsItemAttributes CreateAttributes()
	{
		return new NefsItemAttributes(
			v20IsZlib: Flags.HasFlag(Nefs20HeaderPart6Flags.IsZlib),
			v20IsAes: Flags.HasFlag(Nefs20HeaderPart6Flags.IsAes),
			isDirectory: Flags.HasFlag(Nefs20HeaderPart6Flags.IsDirectory),
			isDuplicated: Flags.HasFlag(Nefs20HeaderPart6Flags.IsDuplicated),
			v20Unknown0x10: Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x10),
			v20Unknown0x20: Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x20),
			v20Unknown0x40: Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x40),
			v20Unknown0x80: Flags.HasFlag(Nefs20HeaderPart6Flags.Unknown0x80),
			part6Volume: Volume,
			part6Unknown0x3: Unknown0x3);
	}
}
