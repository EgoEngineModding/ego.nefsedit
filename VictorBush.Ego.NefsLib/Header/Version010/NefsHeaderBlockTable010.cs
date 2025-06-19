// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version010;

/// <summary>
/// The block table.
/// </summary>
public sealed class NefsHeaderBlockTable010 : NefsTocTable<NefsTocBlock010>,
	INefsTocTable<NefsHeaderBlockTable010, NefsTocBlock010>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderBlockTable010"/> class.
	/// </summary>
	internal NefsHeaderBlockTable010(IReadOnlyList<NefsTocBlock010> entries) : base(entries)
	{
	}

	/// <inheritdoc cref="INefsTocTable{T,TData}.Create"/>
	static NefsHeaderBlockTable010 INefsTocTable<NefsHeaderBlockTable010, NefsTocBlock010>.Create(
		IReadOnlyList<NefsTocBlock010> entries)
	{
		return new(entries);
	}
}
