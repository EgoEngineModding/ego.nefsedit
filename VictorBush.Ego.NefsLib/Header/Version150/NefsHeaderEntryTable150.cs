// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The primary table of entries.
/// </summary>
public sealed class NefsHeaderEntryTable150 : INefsTocTable<NefsTocEntry150>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocEntry150> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderEntryTable150"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal NefsHeaderEntryTable150(IReadOnlyList<NefsTocEntry150> entries)
	{
		Entries = entries;
	}
}
