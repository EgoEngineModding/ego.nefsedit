// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

public record VolumeInfo
{
	/// <summary>
	/// The file name of the volume.
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// The total size of the volume.
	/// </summary>
	public ulong Size { get; init; }

	/// <summary>
	/// The offset to the data.
	/// </summary>
	public uint DataOffset { get; init; }
}
