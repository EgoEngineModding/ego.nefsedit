// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

public static class NefsConstants
{
	// NeFS
	public const uint FourCc = 0x5346654E;

	public const int IntroSize = 128;

	/// <summary>
	/// The block index used to denote that there are no blocks for an entry.
	/// </summary>
	public const uint NoBlocksIndex = uint.MaxValue;

	/// <summary>
	/// Returns the given header as T or throws.
	/// </summary>
	public static T As<T>(this INefsHeader header)
		where T : INefsHeader
	{
		if (header is not T headerAsT)
		{
			throw new ArgumentException($"Header must be of type {typeof(T).Name}", nameof(header));
		}

		return headerAsT;
	}
}
