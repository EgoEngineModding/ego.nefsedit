// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version140;

/// <summary>
/// The volume data start table.
/// </summary>
public sealed class NefsHeaderVolumeDataStartTable140 : NefsTocTable<NefsTocVolumeDataStart140>,
	INefsTocTable<NefsHeaderVolumeDataStartTable140, NefsTocVolumeDataStart140>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderVolumeDataStartTable140"/> class.
	/// </summary>
	internal NefsHeaderVolumeDataStartTable140(IReadOnlyList<NefsTocVolumeDataStart140> entries) : base(entries)
	{
	}

	/// <inheritdoc cref="INefsTocTable{T,TData}.Create"/>
	static NefsHeaderVolumeDataStartTable140 INefsTocTable<NefsHeaderVolumeDataStartTable140, NefsTocVolumeDataStart140>.Create(
		IReadOnlyList<NefsTocVolumeDataStart140> entries)
	{
		return new(entries);
	}
}
