// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.ArchiveSource;

/// <summary>
/// Header and file data are both stored in the same file.
/// </summary>
public sealed class StandardSource : NefsArchiveSource
{
	internal StandardSource(string filePath)
		: base(filePath)
	{
	}
}
