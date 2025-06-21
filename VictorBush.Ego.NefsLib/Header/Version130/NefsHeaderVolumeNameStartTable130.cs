// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version130;

/// <summary>
/// The volume size table.
/// </summary>
public sealed class NefsHeaderVolumeNameStartTable130 : NefsTocTable<NefsTocVolumeNameStart130>,
	INefsTocTable<NefsHeaderVolumeNameStartTable130, NefsTocVolumeNameStart130>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderVolumeNameStartTable130"/> class.
	/// </summary>
	internal NefsHeaderVolumeNameStartTable130(IReadOnlyList<NefsTocVolumeNameStart130> entries) : base(entries)
	{
	}

	/// <inheritdoc cref="INefsTocTable{T,TData}.Create"/>
	static NefsHeaderVolumeNameStartTable130 INefsTocTable<NefsHeaderVolumeNameStartTable130, NefsTocVolumeNameStart130>.Create(
		IReadOnlyList<NefsTocVolumeNameStart130> entries)
	{
		return new(entries);
	}
}
