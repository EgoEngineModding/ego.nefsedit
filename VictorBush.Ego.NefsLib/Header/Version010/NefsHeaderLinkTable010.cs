// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version010;

/// <summary>
/// The link table.
/// </summary>
public sealed class NefsHeaderLinkTable010 : NefsTocTable<NefsTocLink010>,
	INefsTocTable<NefsHeaderLinkTable010, NefsTocLink010>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderLinkTable010"/> class.
	/// </summary>
	internal NefsHeaderLinkTable010(IReadOnlyList<NefsTocLink010> entries) : base(entries)
	{
	}

	/// <inheritdoc cref="INefsTocTable{T,TData}.Create"/>
	static NefsHeaderLinkTable010 INefsTocTable<NefsHeaderLinkTable010, NefsTocLink010>.Create(
		IReadOnlyList<NefsTocLink010> entries)
	{
		return new(entries);
	}
}
