// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The writable entry table.
/// </summary>
public sealed class Nefs160HeaderWriteableEntryTable : INefsTocTable<Nefs160TocEntryWriteable>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs160TocEntryWriteable> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs160HeaderWriteableEntryTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs160HeaderWriteableEntryTable(IReadOnlyList<Nefs160TocEntryWriteable> entries)
	{
		Entries = entries;
	}
}
