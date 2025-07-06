// See LICENSE.txt for license information.

using System.IO.Abstractions;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.DataSource;

/// <summary>
/// Defines an item data source from an existing archive. The volume defines the path to the file that contains the
/// item data content.
/// </summary>
/// <remarks>
/// When saving an archive, the majority of items are typically unchanged. In this case, the item data is copied from
/// the existing archive into the new archive.
/// </remarks>
internal class NefsVolumeDataSource : INefsDataSource
{
	private readonly NefsVolumeSource volume;

	/// <inheritdoc />
	public string FilePath => this.volume.GetPathAtPosition(Offset);

	/// <inheritdoc />
	/// <remarks>For a volume data source, the data is already transformed.</remarks>
	public bool IsTransformed => true;

	/// <inheritdoc />
	public long Offset { get; }

	/// <inheritdoc />
	public NefsItemSize Size { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsVolumeDataSource"/> class.
	/// </summary>
	/// <param name="items">The item list that specifies the file the item's data is stored in.</param>
	/// <param name="offset">The offset in the source file where the data begins.</param>
	/// <param name="size">The size of the item's data in bytes.</param>
	/// <remarks>Used for testing purposes.</remarks>
	internal NefsVolumeDataSource(
		NefsItemList items,
		long offset,
		NefsItemSize size) : this(items.Volumes[0], offset, size)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsVolumeDataSource"/> class.
	/// </summary>
	/// <param name="volume">The volume that the item's data is stored in.</param>
	/// <param name="offset">The offset in the source file where the data begins.</param>
	/// <param name="size">The size of the item's data in bytes.</param>
	public NefsVolumeDataSource(
		NefsVolumeSource volume,
		long offset,
		NefsItemSize size)
	{
		this.volume = volume;
		Offset = offset;
		Size = size;
	}

	/// <inheritdoc />
	public Stream OpenRead(IFileSystem fileSystem)
	{
		if (!this.volume.IsSplit)
		{
			return fileSystem.File.OpenRead(this.volume.FilePath);
		}

		return new SplitFileStream(this.volume, fileSystem, new FileStreamOptions
		{
			Mode = FileMode.Open,
			Access = FileAccess.Read,
			Share = FileShare.Read
		});
	}

	/// <inheritdoc />
	public bool Exists(IFileSystem fileSystem)
	{
		if (!this.volume.IsSplit)
		{
			return fileSystem.File.Exists(this.volume.FilePath);
		}

		for (var i = this.volume.GetFileNumberAtPosition(Offset);
		     i <= this.volume.GetFileNumberAtPosition(Offset + Size.TransformedSize - 1);
		     ++i)
		{
			if (!fileSystem.File.Exists(this.volume.FilePath))
			{
				return false;
			}
		}

		return true;
	}
}
