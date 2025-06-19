// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version010;

/// <summary>
/// The primary table of entries.
/// </summary>
public sealed class NefsHeaderEntryTable010 : NefsTocTable<NefsTocEntry010>,
	INefsTocTable<NefsHeaderEntryTable010, NefsTocEntry010>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderEntryTable010"/> class.
	/// </summary>
	internal NefsHeaderEntryTable010(IReadOnlyList<NefsTocEntry010> entries) : base(entries)
	{
	}

	/// <inheritdoc cref="INefsTocTable{T,TData}.Create"/>
	static NefsHeaderEntryTable010 INefsTocTable<NefsHeaderEntryTable010, NefsTocEntry010>.Create(
		IReadOnlyList<NefsTocEntry010> entries)
	{
		return new(entries);
	}
}
