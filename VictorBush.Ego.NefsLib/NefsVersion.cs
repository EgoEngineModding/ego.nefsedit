// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib;

/// <summary>
/// Supported version numbers.
/// </summary>
public enum NefsVersion
{
	/// <summary>
	/// Version 0.1.0.
	/// </summary>
	Version010 = 0x00100,

	/// <summary>
	/// Version 0.2.0.
	/// </summary>
	Version020 = 0x00200,

	/// <summary>
	/// Version 1.3.0.
	/// </summary>
	Version130 = 0x10300,

	/// <summary>
	/// Version 1.4.0.
	/// </summary>
	Version140 = 0x10400,

	/// <summary>
	/// Version 1.5.0.
	/// </summary>
	Version150 = 0x10500,

	/// <summary>
	/// Version 1.5.1.
	/// </summary>
	Version151 = 0x10501,

	/// <summary>
	/// Version 1.6.0.
	/// </summary>
	Version160 = 0x10600,

	/// <summary>
	/// Version 2.0.0.
	/// </summary>
	Version200 = 0x20000,
}

public static class NefsVersionExtensions
{
	public static string ToPrettyString(this NefsVersion version)
	{
		return version switch
		{
			NefsVersion.Version010 => "Version 0.1.0",
			NefsVersion.Version020 => "Version 0.2.0",
			NefsVersion.Version130 => "Version 1.3.0",
			NefsVersion.Version140 => "Version 1.4.0",
			NefsVersion.Version150 => "Version 1.5.0",
			NefsVersion.Version151 => "Version 1.5.1",
			NefsVersion.Version160 => "Version 1.6.0",
			NefsVersion.Version200 => "Version 2.0.0",
			_ => $"Version 0x{version:X}"
		};
	}
}
