// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// An entry in header part 4 for an item in an archive.
/// </summary>
public sealed class Nefs150HeaderPart4Entry : INefsHeaderPartEntry
{
	public static readonly int EntrySize = Nefs150TocBlock.ByteCount;
	private readonly Nefs150TocBlock data;

	internal Nefs150HeaderPart4Entry(Nefs150TocBlock? data = null)
	{
		this.data = data ?? new Nefs150TocBlock();
	}

	/// <summary>
	/// The underlying data.
	/// </summary>
	public Nefs150TocBlock Data => this.data;

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
		init => this.data.Transformation = (uint)value;
	}
}
