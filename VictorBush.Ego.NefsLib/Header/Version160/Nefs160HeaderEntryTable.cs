// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The primary table of entries.
/// </summary>
public sealed class Nefs160HeaderEntryTable : INefsTocTable<Nefs160TocEntry>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs160TocEntry> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs160HeaderEntryTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs160HeaderEntryTable(IReadOnlyList<Nefs160TocEntry> entries)
	{
		Entries = entries;
	}
}
