// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The volume info table.
/// </summary>
public sealed class NefsHeaderVolumeInfoTable150 : INefsTocTable<NefsTocVolumeInfo150>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocVolumeInfo150> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderVolumeInfoTable150"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this table with.</param>
	internal NefsHeaderVolumeInfoTable150(IReadOnlyList<NefsTocVolumeInfo150> entries)
	{
		Entries = entries;
	}
}
