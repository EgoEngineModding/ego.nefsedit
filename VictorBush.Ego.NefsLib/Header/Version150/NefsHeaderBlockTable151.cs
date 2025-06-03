// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

/// <summary>
/// The block table.
/// </summary>
public sealed class NefsHeaderBlockTable151 : INefsTocTable<NefsTocBlock151>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocBlock151> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderBlockTable151"/> class.
	/// </summary>
	/// <param name="entries">A collection of entries to initialize this object with.</param>
	internal NefsHeaderBlockTable151(IReadOnlyList<NefsTocBlock151> entries)
	{
		Entries = entries;
	}
}
