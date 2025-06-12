// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The writable entry table.
/// </summary>
public sealed class NefsHeaderWriteableEntryTable160 : INefsTocTable<NefsTocEntryWriteable160>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocEntryWriteable160> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderWriteableEntryTable160"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal NefsHeaderWriteableEntryTable160(IReadOnlyList<NefsTocEntryWriteable160> entries)
	{
		Entries = entries;
	}
}
