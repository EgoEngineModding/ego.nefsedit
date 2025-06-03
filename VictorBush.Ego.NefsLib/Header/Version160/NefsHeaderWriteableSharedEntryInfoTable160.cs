// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version160;

/// <summary>
/// The writeable shared entry info table.
/// </summary>
public sealed class NefsHeaderWriteableSharedEntryInfoTable160 : INefsTocTable<NefsTocSharedEntryInfoWriteable160>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocSharedEntryInfoWriteable160> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderWriteableSharedEntryInfoTable160"/> class.
	/// </summary>
	/// <param name="entries">A list of entries to instantiate this part with.</param>
	internal NefsHeaderWriteableSharedEntryInfoTable160(IReadOnlyList<NefsTocSharedEntryInfoWriteable160> entries)
	{
		Entries = entries;
	}
}
