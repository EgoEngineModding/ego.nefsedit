// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The primary table of entries.
/// </summary>
public sealed class Nefs150HeaderEntryTable : INefsTocTable<Nefs150TocEntry>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs150TocEntry> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs150HeaderEntryTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs150HeaderEntryTable(IReadOnlyList<Nefs150TocEntry> entries)
	{
		Entries = entries;
	}
}
