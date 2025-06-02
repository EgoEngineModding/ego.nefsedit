// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The writeable shared entry info table.
/// </summary>
public sealed class Nefs160HeaderWriteableSharedEntryInfoTable : INefsTocTable<Nefs160TocSharedEntryInfoWriteable>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs160TocSharedEntryInfoWriteable> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs160HeaderWriteableSharedEntryInfoTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs160HeaderWriteableSharedEntryInfoTable(IReadOnlyList<Nefs160TocSharedEntryInfoWriteable> entries)
	{
		Entries = entries;
	}
}
