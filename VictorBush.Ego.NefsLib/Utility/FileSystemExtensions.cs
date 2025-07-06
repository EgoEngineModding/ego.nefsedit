// See LICENSE.txt for license information.

using System.IO.Abstractions;
using VictorBush.Ego.NefsLib.DataSource;

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

	/// <summary>
	/// Opens the item data source stream for reading.
	/// </summary>
	/// <param name="fs">The file system.</param>
	/// <param name="dataSource">The data source.</param>
	/// <returns>The data source stream for reading.</returns>
	public static Stream OpenRead(this IFileSystem fs, INefsDataSource dataSource)
	{
		return dataSource.OpenRead(fs);
	}

	/// <summary>
	/// Determines whether the item's data source files exist.
	/// </summary>
	/// <param name="fs">The file system.</param>
	/// <param name="dataSource">The data source.</param>
	/// <returns>True if the data source files exist, otherwise false.</returns>
	public static bool Exists(this IFileSystem fs, INefsDataSource dataSource)
	{
		return dataSource.Exists(fs);
	}
}
