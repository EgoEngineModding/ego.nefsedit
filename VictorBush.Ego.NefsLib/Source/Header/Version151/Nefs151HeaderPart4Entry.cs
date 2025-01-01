// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// An entry in header part 4 for an item in an archive.
/// </summary>
public sealed class Nefs151HeaderPart4Entry : INefsHeaderPartEntry
{
	public static readonly int EntrySize = Nefs151TocBlock.ByteCount;
	private readonly Nefs151TocBlock data;

	internal Nefs151HeaderPart4Entry(Nefs151TocBlock? data = null)
	{
		this.data = data ?? new Nefs151TocBlock();
	}

	/// <summary>
	/// The underlying data.
	/// </summary>
	public Nefs151TocBlock Data => this.data;

	/// <summary>
	/// Cumulative block size of this chunk.
	/// </summary>
	public uint CumulativeBlockSize
	{
		get => this.data.End;
		init => this.data.End = value;
	}

	/// <summary>
	/// The size of a part 4 entry. This is used to get the offset into part 4 from an index into part 4.
	/// </summary>
	public int Size => EntrySize;

	/// <summary>
	/// Transformation applied to this chunk.
	/// </summary>
	public Nefs16HeaderPart4TransformType TransformType
	{
		get => (Nefs16HeaderPart4TransformType)this.data.Transformation;
		init => this.data.Transformation = (ushort)value;
	}

	/// <summary>
	/// Checksum of the chunk.
	/// </summary>
	public ushort Checksum
	{
		get => this.data.Checksum;
		init => this.data.Checksum = value;
	}
}
