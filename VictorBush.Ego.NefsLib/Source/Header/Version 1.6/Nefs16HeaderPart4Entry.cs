// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// An entry in header part 4 for an item in an archive.
/// </summary>
public sealed class Nefs16HeaderPart4Entry : INefsHeaderPartEntry
{
	internal Nefs16HeaderPart4Entry()
	{
	}

	/// <summary>
	/// Checksum of the chunk.
	/// </summary>
	public ushort Checksum
	{
		get => Data0x06_Checksum.Value;
		init => Data0x06_Checksum.Value = value;
	}

	/// <summary>
	/// Cumulative block size of this chunk.
	/// </summary>
	public uint CumulativeBlockSize
	{
		get => Data0x00_CumulativeBlockSize.Value;
		init => Data0x00_CumulativeBlockSize.Value = value;
	}

	/// <summary>
	/// The size of a part 4 entry. This is used to get the offset into part 4 from an index into part 4.
	/// </summary>
	public int Size => Nefs16HeaderPart4.EntrySize;

	/// <summary>
	/// Transformation applied to this chunk.
	/// </summary>
	public Nefs16HeaderPart4TransformType TransformType
	{
		get => (Nefs16HeaderPart4TransformType)Data0x04_TransformType.Value;
		init => Data0x04_TransformType.Value = (ushort)value;
	}

	[FileData]
	private UInt32Type Data0x00_CumulativeBlockSize { get; } = new UInt32Type(0x00);

	[FileData]
	private UInt16Type Data0x04_TransformType { get; } = new UInt16Type(0x04);

	[FileData]
	private UInt16Type Data0x06_Checksum { get; } = new UInt16Type(0x06);
}
