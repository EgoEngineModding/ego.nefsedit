// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header part 5.
/// </summary>
public sealed class NefsHeaderPart5
{
	/// <summary>
	/// The size of header part 5.
	/// </summary>
	public const int Size = 0x10;

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart5"/> class.
	/// </summary>
	internal NefsHeaderPart5()
	{
		FirstDataOffset = Nefs20Header.DataOffsetDefault;
	}

	/// <summary>
	/// Offset into header part 3 for the name of the file containing the item data. For headless archives, it would be
	/// something like game.dat, game.bin, etc. For standard archives, it would be the name of the archive.
	/// </summary>
	public uint DataFileNameStringOffset
	{
		get => Data0x08_DataFileNameStringOffset.Value;
		init => Data0x08_DataFileNameStringOffset.Value = value;
	}

	/// <summary>
	/// The size of all item data. For headless archives, this is the size of the data file.
	/// </summary>
	public ulong DataSize
	{
		get => Data0x00_TotalItemDataSize.Value;
		init => Data0x00_TotalItemDataSize.Value = value;
	}

	/// <summary>
	/// Offset to first item data.
	/// </summary>
	public uint FirstDataOffset
	{
		get => Data0x0C_FirstDataOffset.Value;
		init => Data0x0C_FirstDataOffset.Value = value;
	}

	[FileData]
	private UInt64Type Data0x00_TotalItemDataSize { get; } = new UInt64Type(0x00);

	[FileData]
	private UInt32Type Data0x08_DataFileNameStringOffset { get; } = new UInt32Type(0x08);

	[FileData]
	private UInt32Type Data0x0C_FirstDataOffset { get; } = new UInt32Type(0x0C);
}
