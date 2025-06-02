// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The block table.
/// </summary>
public sealed class Nefs151HeaderBlockTable : INefsTocTable<Nefs151TocBlock>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs151TocBlock> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151HeaderBlockTable"/> class.
	/// </summary>
	/// <param name="entries">A collection of entries to initialize this object with.</param>
	internal Nefs151HeaderBlockTable(IReadOnlyList<Nefs151TocBlock> entries)
	{
		Entries = entries;
	}
}
