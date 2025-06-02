// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The shared entry info table.
/// </summary>
public sealed class Nefs160HeaderSharedEntryInfoTable : INefsTocTable<Nefs160TocSharedEntryInfo>
{
	/// <inheritdoc />
	public IReadOnlyList<Nefs160TocSharedEntryInfo> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs160HeaderSharedEntryInfoTable"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal Nefs160HeaderSharedEntryInfoTable(IReadOnlyList<Nefs160TocSharedEntryInfo> entries)
	{
		Entries = entries;
	}
}
