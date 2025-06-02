// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The block table.
/// </summary>
public sealed class NefsHeaderBlockTable150 : INefsTocTable<NefsTocBlock150>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocBlock150> Entries { get; }

	/// <summary>
	/// There is a 4-byte value at the end of header part 4. Purpose unknown.
	/// </summary>
	public uint UnkownEndValue { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderBlockTable150"/> class.
	/// </summary>
	/// <param name="entries">A collection of entries to initialize this object with.</param>
	/// <param name="unkownEndValue">Last four bytes of part 4.</param>
	internal NefsHeaderBlockTable150(IReadOnlyList<NefsTocBlock150> entries, uint unkownEndValue)
	{
		Entries = entries;
		UnkownEndValue = unkownEndValue;
	}
}
