// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;

namespace VictorBush.Ego.NefsLib.Item;

/// <summary>
/// An item in a NeFS archive (file or directory).
/// </summary>
public sealed record NefsItem
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItem"/> class.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <param name="fileName">The file name within the archive.</param>
	/// <param name="directoryId">The directory id the item is in.</param>
	/// <param name="dataSource">The data source for the item's data.</param>
	/// <param name="transform">The transform that is applied to this item's data. Can be null if no transform.</param>
	/// <param name="attributes">Additional attributes.</param>
	/// <param name="state">The item state.</param>
	internal NefsItem(
		NefsItemId id,
		string fileName,
		NefsItemId directoryId,
		INefsDataSource dataSource,
		NefsDataTransform? transform,
		NefsItemAttributes attributes,
		NefsItemState state = NefsItemState.None)
		: this(id, id, fileName, directoryId, dataSource, transform, attributes, state)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItem"/> class.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <param name="firstDuplicateId">The id of the first duplicate.</param>
	/// <param name="fileName">The file name within the archive.</param>
	/// <param name="directoryId">The directory id the item is in.</param>
	/// <param name="dataSource">The data source for the item's data.</param>
	/// <param name="transform">The transform that is applied to this item's data. Can be null if no transform.</param>
	/// <param name="attributes">Additional attributes.</param>
	/// <param name="state">The item state.</param>
	public NefsItem(
		NefsItemId id,
		NefsItemId firstDuplicateId,
		string fileName,
		NefsItemId directoryId,
		INefsDataSource dataSource,
		NefsDataTransform? transform,
		NefsItemAttributes attributes,
		NefsItemState state = NefsItemState.None)
	{
		Id = id;
		FirstDuplicateId = firstDuplicateId;
		DirectoryId = directoryId;
		DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
		State = state;
		Transform = transform;
		Attributes = attributes;

		// Save file name
		FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
	}

	/// <summary>
	/// Additional item attributes.
	/// </summary>
	public NefsItemAttributes Attributes { get; }

	/// <summary>
	/// The size of the item's data in the archive.
	/// </summary>
	public uint CompressedSize => DataSource.Size.TransformedSize;

	/// <summary>
	/// The current data source for this item.
	/// </summary>
	public INefsDataSource DataSource { get; private set; }

	/// <summary>
	/// The id of the directory item this item is in. When the item is in the root directory, the parent id is the same
	/// as the item's id.
	/// </summary>
	public NefsItemId DirectoryId { get; init; }

	/// <summary>
	/// The size of the item's data when extracted from the archive.
	/// </summary>
	public uint ExtractedSize => DataSource.Size.ExtractedSize;

	/// <summary>
	/// The item's file or directory name, depending on type.
	/// </summary>
	public string FileName { get; }

	/// <summary>
	/// The id of this item.
	/// </summary>
	public NefsItemId Id { get; init; }

	/// <summary>
	/// The id of the first duplicate.
	/// </summary>
	public NefsItemId FirstDuplicateId { get; init; }

	/// <summary>
	/// The modification state of the item. Represents any pending changes to this item. Pending changes are applied
	/// when the archive is saved.
	/// </summary>
	public NefsItemState State { get; private set; }

	/// <summary>
	/// The transform that is applied to this item's data. Is null if no transform.
	/// </summary>
	public NefsDataTransform? Transform { get; }

	/// <summary>
	/// The type of item this is.
	/// </summary>
	public NefsItemType Type => Attributes.IsDirectory ? NefsItemType.Directory : NefsItemType.File;

	/// <summary>
	/// Whether this item is a duplicate of another.
	/// </summary>
	public bool IsDuplicate => Id != FirstDuplicateId;

	/// <summary>
	/// Updates the data source for the item and the item's state. If the data source has changed, the new item data
	/// will be written to the archive when it is saved. If compression is required, the file data will be compressed
	/// when the archive is written.
	/// </summary>
	/// <param name="dataSource">The new data source to use.</param>
	/// <param name="state">The new item state.</param>
	public void UpdateDataSource(INefsDataSource dataSource, NefsItemState state)
	{
		if (Type == NefsItemType.Directory)
		{
			throw new InvalidOperationException($"Cannot perform {nameof(UpdateDataSource)} on a directory.");
		}

		DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
		State = state;
	}

	/// <summary>
	/// Updates the item state.
	/// </summary>
	/// <param name="state">The new state.</param>
	public void UpdateState(NefsItemState state)
	{
		State = state;
	}
}
