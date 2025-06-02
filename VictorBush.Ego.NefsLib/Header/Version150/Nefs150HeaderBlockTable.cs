// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The block table.
/// </summary>
public sealed class Nefs150HeaderBlockTable : INefsTocTable<Nefs150TocBlock>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs150TocBlock> Entries { get; }

	/// <summary>
	/// There is a 4-byte value at the end of header part 4. Purpose unknown.
	/// </summary>
	public uint UnkownEndValue { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderBlockTable"/> class.
	/// </summary>
	/// <param name="entries">A collection of entries to initialize this object with.</param>
	/// <param name="unkownEndValue">Last four bytes of part 4.</param>
	internal Nefs150HeaderBlockTable(IReadOnlyList<Nefs150TocBlock> entries, uint unkownEndValue)
	{
		Entries = entries;
		UnkownEndValue = unkownEndValue;
	}
}
