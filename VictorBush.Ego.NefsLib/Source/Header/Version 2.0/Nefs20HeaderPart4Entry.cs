// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// An entry in header part 4 for an item in an archive.
/// </summary>
public sealed class Nefs20HeaderPart4Entry : INefsHeaderPartEntry
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs20HeaderPart4Entry"/> class.
	/// </summary>
	internal Nefs20HeaderPart4Entry()
	{
	}

	/// <summary>
	/// Cumulative size of this data chunk. To get the size of this chunk, subtract the previous chunk's cumulative size.
	/// </summary>
	public uint CumulativeChunkSize
	{
		get => Data0x00_CumulativeChunkSize.Value;
		init => Data0x00_CumulativeChunkSize.Value = value;
	}

	/// <summary>
	/// The size of a part 4 entry. This is used to get the offset into part 4 from an index into part 4.
	/// </summary>
	public int Size => Nefs20HeaderPart4.EntrySize;

	[FileData]
	private UInt32Type Data0x00_CumulativeChunkSize { get; } = new UInt32Type(0x00);
}
