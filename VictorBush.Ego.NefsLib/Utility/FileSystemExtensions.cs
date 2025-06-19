// See LICENSE.txt for license information.

using System.IO.Abstractions;

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// Extensions for <see cref="IFileSystem"/>.
/// </summary>
public static class FileSystemExtensions
{
	/// <summary>
	/// Deletes the specified directory if it exists, then creates it.
	/// </summary>
	/// <param name="fs">The file system.</param>
	/// <param name="directoryPath">The directory path.</param>
	public static void ResetOrCreateDirectory(this IFileSystem fs, string directoryPath)
	{
		if (fs.Directory.Exists(directoryPath))
		{
			fs.Directory.Delete(directoryPath, true);
		}

		fs.Directory.CreateDirectory(directoryPath);
	}
}
