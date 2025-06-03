// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The shared entry info table.
/// </summary>
public sealed class NefsHeaderSharedEntryInfoTable160 : INefsTocTable<NefsTocSharedEntryInfo160>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocSharedEntryInfo160> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderSharedEntryInfoTable160"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal NefsHeaderSharedEntryInfoTable160(IReadOnlyList<NefsTocSharedEntryInfo160> entries)
	{
		Entries = entries;
	}
}
