// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The shared entry info table.
/// </summary>
public sealed class NefsHeaderSharedEntryInfoTable150 : INefsTocTable<NefsTocSharedEntryInfo150>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocSharedEntryInfo150> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderSharedEntryInfoTable150"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal NefsHeaderSharedEntryInfoTable150(IReadOnlyList<NefsTocSharedEntryInfo150> entries)
	{
		Entries = entries;
	}
}
