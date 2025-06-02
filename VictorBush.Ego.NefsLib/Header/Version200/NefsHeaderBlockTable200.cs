// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version200;

/// <summary>
/// The block table.
/// </summary>
public sealed class NefsHeaderBlockTable200 : INefsTocTable<NefsTocBlock200>
{
	/// <inheritdoc />
	public IReadOnlyList<NefsTocBlock200> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderBlockTable200"/> class.
	/// </summary>
	internal NefsHeaderBlockTable200(IReadOnlyList<NefsTocBlock200> entries)
	{
		Entries = entries;
	}
}
