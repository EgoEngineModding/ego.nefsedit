// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The primary table of entries.
/// </summary>
public sealed class NefsHeaderEntryTable160 : INefsTocTable<NefsTocEntry160>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocEntry160> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderEntryTable160"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal NefsHeaderEntryTable160(IReadOnlyList<NefsTocEntry160> entries)
	{
		Entries = entries;
	}
}
