// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

/// <summary>
/// Represents an object that finds NeFS headers in exe files.
/// </summary>
public interface INefsExeHeaderFinder
{
	/// <summary>
	/// Find headers within the exe file.
	/// </summary>
	/// <returns>The archive source based on the found headers.</returns>
	Task<List<HeadlessSource>> FindHeadersAsync(string exePath, string dataFileDir, bool searchEntireExe,
		NefsProgress p);
}
