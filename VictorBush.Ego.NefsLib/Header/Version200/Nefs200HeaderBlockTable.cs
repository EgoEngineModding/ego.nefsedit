// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version200;

/// <summary>
/// The block table.
/// </summary>
public sealed class Nefs200HeaderBlockTable : INefsTocTable<Nefs200TocBlock>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs200TocBlock> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs200HeaderBlockTable"/> class.
	/// </summary>
	internal Nefs200HeaderBlockTable(IReadOnlyList<Nefs200TocBlock> entries)
	{
		Entries = entries;
	}
}
