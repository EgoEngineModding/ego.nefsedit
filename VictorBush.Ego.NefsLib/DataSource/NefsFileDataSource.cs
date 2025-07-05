// See LICENSE.txt for license information.

using System.IO.Abstractions;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.DataSource;

/// <summary>
/// Defines an item data source from a file on disk.
/// </summary>
public class NefsFileDataSource : INefsDataSource
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsFileDataSource"/> class.
	/// </summary>
	/// <param name="filePath">The path of the file that contain's the data.</param>
	/// <param name="offset">The offset in the source file where the data begins.</param>
	/// <param name="size">Size information about the item's data.</param>
	/// <param name="isTransformed">
	/// Whether the data in this data source has already been transformed (encrypted, compressed, etc).
	/// </param>
	public NefsFileDataSource(string filePath, long offset, NefsItemSize size, bool isTransformed)
	{
		FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
		Offset = offset;
		Size = size;
		IsTransformed = isTransformed;
	}

	/// <inheritdoc/>
	public string FilePath { get; }

	/// <inheritdoc/>
	public bool IsTransformed { get; }

	/// <inheritdoc/>
	public long Offset { get; }

	/// <inheritdoc/>
	public NefsItemSize Size { get; }

	/// <inheritdoc />
	public Stream OpenRead(IFileSystem fileSystem)
	{
		return fileSystem.File.OpenRead(FilePath);
	}

	/// <inheritdoc />
	public bool Exists(IFileSystem fileSystem)
	{
		return fileSystem.File.Exists(FilePath);
	}
}
