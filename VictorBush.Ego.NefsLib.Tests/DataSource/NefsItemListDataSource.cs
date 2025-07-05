// See LICENSE.txt for license information.

using System.IO.Abstractions;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Tests.DataSource;

/// <summary>
/// Defines an item data source from an existing archive. The item list defines the path to the file that contains the
/// item data content.
/// </summary>
/// <remarks>
/// When saving an archive, the majority of items are typically unchanged. In this case, the item data is copied from
/// the existing archive into the new archive. The <see cref="NefsItemListDataSource"/> allows an item to have access to
/// the file path that contains the item's data without having to have a copy of the file path string in every item.
/// </remarks>
public class NefsItemListDataSource : INefsDataSource
{
	/// <summary>
	/// Item list that defines the source file path for item data.
	/// </summary>
	private readonly NefsItemList items;

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemListDataSource"/> class.
	/// </summary>
	/// <param name="items">The items list that specifies the file the item's data is stored in.</param>
	/// <param name="offset">The offset in the source file where the data begins.</param>
	/// <param name="size">The size of the item's data in bytes.</param>
	public NefsItemListDataSource(
		NefsItemList items,
		long offset,
		NefsItemSize size)
	{
		this.items = items ?? throw new ArgumentNullException(nameof(items));
		Offset = offset;
		Size = size;
	}

	/// <inheritdoc/>
	public string FilePath => this.items.DataFilePath;

	/// <inheritdoc/>
	/// <remarks>For an item list data source, the data is already transformed.</remarks>
	public bool IsTransformed => true;

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
