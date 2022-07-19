// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.ArchiveSource;

/// <summary>
/// Similar to a headless archive, but the header sections are stored in the NefsInject format.
/// </summary>
public sealed class NefsInjectSource : NefsArchiveSource
{
	internal NefsInjectSource(string dataFilePath, string nefsInjectFilePath)
		: base(dataFilePath)
	{
		NefsInjectFilePath = nefsInjectFilePath ?? throw new ArgumentNullException(nameof(nefsInjectFilePath));
	}

	/// <summary>
	/// Path to the file containing the file data (i.e., game.dat, game.bin, etc.).
	/// </summary>
	public string DataFilePath => FilePath;

	/// <summary>
	/// Path to the NefsInject file that contains the archive header and related offsets.
	/// </summary>
	public string NefsInjectFilePath { get; }
}
