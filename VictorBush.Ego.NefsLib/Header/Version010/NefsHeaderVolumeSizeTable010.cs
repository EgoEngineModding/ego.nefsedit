// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version010;

/// <summary>
/// The volume size table.
/// </summary>
public sealed class NefsHeaderVolumeSizeTable010 : NefsTocTable<NefsTocVolumeSize010>,
	INefsTocTable<NefsHeaderVolumeSizeTable010, NefsTocVolumeSize010>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderVolumeSizeTable010"/> class.
	/// </summary>
	internal NefsHeaderVolumeSizeTable010(IReadOnlyList<NefsTocVolumeSize010> entries) : base(entries)
	{
	}

	/// <inheritdoc cref="INefsTocTable{T,TData}.Create"/>
	static NefsHeaderVolumeSizeTable010 INefsTocTable<NefsHeaderVolumeSizeTable010, NefsTocVolumeSize010>.Create(
		IReadOnlyList<NefsTocVolumeSize010> entries)
	{
		return new(entries);
	}
}
