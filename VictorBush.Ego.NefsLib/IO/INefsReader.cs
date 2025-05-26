// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

/// <summary>
/// Reads NeFS archives.
/// </summary>
public interface INefsReader
{
	/// <summary>
	/// Loads a self-contained NeFS archive from a single file. Archive version is auto-detected.
	/// </summary>
	/// <param name="filePath">File path to the archive.</param>
	/// <param name="p">Progress information.</param>
	/// <returns>The loaded <see cref="NefsArchive"/>.</returns>
	Task<NefsArchive> ReadArchiveAsync(string filePath, NefsProgress p);

	/// <summary>
	/// Reads a NeFS archive from the specified source.
	/// </summary>
	/// <param name="source">The archive source.</param>
	/// <param name="p">Progress information.</param>
	/// <returns>The loaded <see cref="NefsArchive"/>.</returns>
	Task<NefsArchive> ReadArchiveAsync(NefsArchiveSource source, NefsProgress p);
}
