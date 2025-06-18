// See LICENSE.txt for license information.

using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.IO;

/// <summary>
/// Writes NeFS archives.
/// </summary>
public class NefsWriter : INefsWriter
{
	internal const uint DefaultHashBlockSize = 0x800000;

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsWriter"/> class.
	/// </summary>
	/// <param name="tempDirPath">Path to a directory that can be used to write temporary files.</param>
	/// <param name="fileSystem">The file system to use.</param>
	/// <param name="transfomer">Interface used to compress and encrypt data.</param>
	public NefsWriter(
		string tempDirPath,
		IFileSystem fileSystem,
		INefsTransformer transfomer)
	{
		TempDirectoryPath = tempDirPath ?? throw new ArgumentNullException(nameof(tempDirPath));
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		Transformer = transfomer ?? throw new ArgumentNullException(nameof(transfomer));
	}

	/// <summary>
	/// The file system.
	/// </summary>
	private IFileSystem FileSystem { get; }

	/// <summary>
	/// The path of the temporary directory to use when writing.
	/// </summary>
	private string TempDirectoryPath { get; }

	/// <summary>
	/// The transfomer used to compress and encrypt item data.
	/// </summary>
	private INefsTransformer Transformer { get; }

	/// <inheritdoc/>
	public async Task<(NefsArchive Archive, NefsArchiveSource Source)> WriteArchiveAsync(string destFilePath, NefsArchive nefs, NefsProgress p)
	{
		NefsArchive newArchive;

		// Setup temp working directory
		var workDir = PrepareWorkingDirectory(destFilePath);

		// Write to temp file
		var tempFilePath = Path.Combine(workDir, "temp.nefs");
		using (var file = FileSystem.File.Open(tempFilePath, FileMode.Create))
		{
			newArchive = await WriteArchiveAsync(file, nefs.Header, nefs.Items, workDir, p);
		}

		// Copy to final destination
		FileSystem.File.Copy(tempFilePath, destFilePath, true);

		return (newArchive, NefsArchiveSource.Standard(destFilePath));
	}

	/// <summary>
	/// Prepares an item's data to be written to the archive.
	/// </summary>
	/// <param name="item">The item to prepare.</param>
	/// <param name="workDir">The temporary working directory.</param>
	/// <param name="items">The source items list.</param>
	/// <param name="p">Progress info.</param>
	private async Task PrepareItemAsync(NefsItem item, string workDir, NefsItemList items, NefsProgress p)
	{
		// Deleted items should not be prepared
		if (item.State == NefsItemState.Removed)
		{
			throw new ArgumentException("Trying to prepare a removed item.", nameof(item));
		}

		// Nothing to do for directories
		if (item.Type == NefsItemType.Directory)
		{
			return;
		}

		// Only added or replaced files need prepared
		if (item.State != NefsItemState.Added && item.State != NefsItemState.Replaced)
		{
			return;
		}

		// Item should have a data source
		if (item.DataSource == null)
		{
			throw new ArgumentException("Item does not have a data source.", nameof(item));
		}

		// Make sure the new file still exists
		if (!FileSystem.File.Exists(item.DataSource.FilePath))
		{
			throw new IOException($"Cannot find source file {item.DataSource.FilePath}.");
		}

		// Compress to temp location if needed
		if (!item.DataSource.IsTransformed)
		{
			if (item.Transform is null)
			{
				// Take the file as-is and assume it's transformed
				var directDataSource = new NefsFileDataSource(item.DataSource.FilePath, item.DataSource.Offset,
					item.DataSource.Size, isTransformed: true);
				item.UpdateDataSource(directDataSource, NefsItemState.Replaced);
				return;
			}

			// Prepare the working directory
			var filePathInArchive = items.GetItemFilePath(item.Id);
			var filePathInArchiveHash = HashHelper.HashStringMD5(filePathInArchive);
			var fileWorkDir = Path.Combine(workDir, filePathInArchiveHash);
			FileSystem.ResetOrCreateDirectory(fileWorkDir);

			// Transform the file
			var destFilePath = Path.Combine(workDir, "inject.dat");
			var newSize = await Transformer.TransformFileAsync(item.DataSource, destFilePath, item.Transform, p);

			// Update data source to point to the transformed temp file
			var dataSource = new NefsFileDataSource(destFilePath, 0, newSize, isTransformed: true);
			item.UpdateDataSource(dataSource, NefsItemState.Replaced);
		}
	}

	/// <summary>
	/// Prepares a new list of items to be written out. The source list is cloned and then updated. Deleted items are
	/// removed. Item data is compressed if needed. Other item metadata is updated. The original item list is not
	/// modified; instead a new, updated list is returned.
	/// </summary>
	/// <param name="sourceItems">The source items list to prepare. This list nor its items are modified.</param>
	/// <param name="workDir">The temporary working directory.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>A prepared item list ready for writing.</returns>
	private async Task<NefsItemList> PrepareItemsAsync(
		NefsItemList sourceItems,
		string workDir,
		NefsProgress p)
	{
		// Create a new items list - the original source list is not modified.
		// The new list is returned that removes deleted items and has updated metadata for the other items.
		// There may be gaps in ids in case of removing an item. Generate a map to make ids sequential
		var idIndexMap = new Dictionary<NefsItemId, NefsItemId>();
		var items = new NefsItemList(sourceItems.DataFilePath);

		foreach (var sourceItem in sourceItems.EnumerateById())
		{
			if (sourceItem.State == NefsItemState.Removed)
			{
				// Item was deleted; remove item from list
				continue;
			}

			var currentId = new NefsItemId(idIndexMap.Count);
			idIndexMap.Add(sourceItem.Id, currentId);
			var item = sourceItem with
			{
				Id = currentId,
				DirectoryId = idIndexMap[sourceItem.DirectoryId],
				FirstDuplicateId = idIndexMap[sourceItem.FirstDuplicateId],
			};

			// Compress any new or replaced files and update chunk sizes
			await PrepareItemAsync(item, workDir, sourceItems, p);
			items.Add(item);
		}

		// Return the new list
		return items;
	}

	/// <summary>
	/// Prepares a temporary working directory.
	/// </summary>
	/// <param name="destFilePath">
	/// The destination nefs archive file path. This is hashed to create a unique directory name in the <see
	/// cref="NefsWriter"/>'s temporary directory <see cref="TempDirectoryPath"/>.
	/// </param>
	private string PrepareWorkingDirectory(string destFilePath)
	{
		// Create temp directory if needed
		if (!FileSystem.Directory.Exists(TempDirectoryPath))
		{
			FileSystem.Directory.CreateDirectory(TempDirectoryPath);
		}

		// Create a temp working directory using a hash of the archive's file path
		var destPathHash = HashHelper.HashStringMD5(destFilePath);
		var workDir = Path.Combine(TempDirectoryPath, destPathHash);

		// Setup the working directory for this archive
		FileSystem.ResetOrCreateDirectory(workDir);

		return workDir;
	}

	/// <summary>
	/// Writes an archive to the specified stream. A new archive obejct is returned that contains the updated header and
	/// item metadata.
	/// </summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="sourceHeader">Donor header information.</param>
	/// <param name="sourceItems">List of items to write. This list is not modified directly.</param>
	/// <param name="workDir">Temp working directory path.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>A new NefsArchive object containing the updated header and item metadata.</returns>
	private async Task<NefsArchive> WriteArchiveAsync(
		Stream stream,
		INefsHeader sourceHeader,
		NefsItemList sourceItems,
		string workDir,
		NefsProgress p)
	{
		// Setup task weights
		var taskWeightPrepareItems = 0.45f;
		var taskWeightWriteItems = 0.45f;
		var taskWeightHeader = 0.1f;

		// Setup helpers
		var headerBuilder = NefsHeaderBuilder.Get(sourceHeader.Version);
		var strategy = NefsWriterStrategy.Get(sourceHeader.Version);

		// Prepare items for writing
		NefsItemList items;
		using (p.BeginTask(taskWeightPrepareItems, "Preparing items"))
		{
			items = await PrepareItemsAsync(sourceItems, workDir, p);
		}

		// Write item data
		var dataOffset = headerBuilder.ComputeDataOffset(sourceHeader, items);
		long dataSize;
		using (p.BeginTask(taskWeightWriteItems, "Writing items"))
		{
			dataSize = await WriteItemsAsync(stream, items, dataOffset, p);
		}

		// TODO: update hash digest table

		// Build header
		var header = headerBuilder.Build(sourceHeader, items, p);

		// Write the header
		Debug.Assert(header.Volumes[0].DataOffset == dataOffset);
		Debug.Assert(header.Volumes[0].Size == Convert.ToUInt64(dataSize));
		using var writer = new EndianBinaryWriter(stream, header.IsLittleEndian);
		using (p.BeginTask(taskWeightHeader, "Writing header"))
		{
			await strategy.WriteHeaderAsync(writer, header, 0, p);
		}

		// TODO: support XOR
		if (false)
		{
			await EncodeXorIntroAsync(writer.BaseStream, 0, p.CancellationToken);
		}

		// Create new archive object
		return new NefsArchive(header, items);
	}

	/// <summary>
	/// Writes an item to the output stream.
	/// </summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="dataOffset">The offset into the stream to write at.</param>
	/// <param name="item">The item whose file data to write.</param>
	/// <param name="p">Progress information.</param>
	/// <returns>The data offset for the next item.</returns>
	private async Task<long> WriteItemAsync(
		Stream stream,
		long dataOffset,
		NefsItem item,
		NefsProgress p)
	{
		// Deleted items should not be written
		Debug.Assert(item.State != NefsItemState.Removed);
		if (item.State == NefsItemState.Removed)
		{
			throw new ArgumentException("Trying to write a removed item.", nameof(item));
		}

		// Nothing to write for directories
		if (item.Type == NefsItemType.Directory)
		{
			return dataOffset;
		}

		// Check if source file exists
		if (!FileSystem.File.Exists(item.DataSource.FilePath))
		{
			throw new IOException($"Cannot find source file {item.DataSource.FilePath}, skipping");
		}

		// Seek to appropriate offset in destination
		stream.Seek(dataOffset, SeekOrigin.Begin);

		// Determine data source
		var srcFile = item.DataSource.FilePath;
		var srcOffset = item.DataSource.Offset;
		var srcSize = item.DataSource.Size.TransformedSize;

		// The data should already be transformed (if needed) by this point
		if (!item.DataSource.IsTransformed)
		{
			throw new InvalidOperationException($"Item data transformation should be handled before calling {nameof(WriteItemAsync)}.");
		}

		// TODO - This must be for v2 nefs only
		//if (item.CompressedSize == item.ExtractedSize
		//    && item.ExtractedSize != 1
		//    && item.Part6Unknown0x02 != 3)
		//{
		//    // Add 8 bytes to the size for some reason
		//    srcSize += 0x8;
		//}

		// Copy data from data source to the destination stream
		using (var inputFile = FileSystem.File.OpenRead(srcFile))
		{
			// Seek source stream to correct offset
			inputFile.Seek(srcOffset, SeekOrigin.Begin);

			// Copy from source to destination
			await inputFile.CopyPartialAsync(stream, srcSize, p.CancellationToken);
		}

		// Return the data offset for the next item
		return dataOffset + srcSize;
	}

	/// <summary>
	/// Writes items' data to the output stream.
	/// </summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="items">List of items to write.</param>
	/// <param name="firstDataOffset">The offset from the beginning of the stream to write the first data.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The offset to the end of the last data written.</returns>
	private async Task<long> WriteItemsAsync(
		Stream stream,
		NefsItemList items,
		long firstDataOffset,
		NefsProgress p)
	{
		var nextDataOffset = firstDataOffset;

		// Prepare stream
		stream.Seek(firstDataOffset, SeekOrigin.Begin);

		// Update item info and write out item data
		var i = 1;
		foreach (var item in items.EnumerateById())
		{
			using (p.BeginSubTask(1.0f / items.Count, $"Writing data for item {i}/{items.Count}"))
			{
				// Get item
				var itemOffset = nextDataOffset;
				var itemSize = item.DataSource.Size;

				// Nothing to write if item is directory
				if (item.Type == NefsItemType.Directory)
				{
					continue;
				}

				// Write out item data
				nextDataOffset = await WriteItemAsync(stream, itemOffset, item, p);

				// Update item data source to point to the newly written data
				var dataSource = new NefsItemListDataSource(items, itemOffset, itemSize);
				item.UpdateDataSource(dataSource, NefsItemState.None);
			}

			i++;
		}

		// Return the next data offset, which is the end of the written data
		return nextDataOffset;
	}

	/// <summary>
	/// Encodes the intro header for file versions 1.5.1.
	/// </summary>
	/// <param name="stream">The stream containing the header.</param>
	/// <param name="offset">The offset to the header from the beginning of the stream.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The encoded header data.</returns>
	internal static async Task EncodeXorIntroAsync(Stream stream, long offset, CancellationToken cancellationToken)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		var buf = new byte[NefsConstants.IntroSize];
		await stream.ReadExactlyAsync(buf, cancellationToken).ConfigureAwait(false);
		static void Encode(byte[] buffer)
		{
			var uintBuf = MemoryMarshal.Cast<byte, uint>(buffer.AsSpan());

			var mod = uintBuf[14];
			for (var i = 15; i < 31; ++i)
			{
				uintBuf[i] ^= mod;
			}

			uintBuf[8] ^= uintBuf[14];
			uintBuf[6] ^= uintBuf[8];
			uintBuf[12] ^= uintBuf[6];
			uintBuf[0] ^= uintBuf[12];
			uintBuf[11] ^= uintBuf[0];
			uintBuf[13] ^= uintBuf[11];
			uintBuf[1] ^= uintBuf[13];
			uintBuf[10] ^= uintBuf[1];
			uintBuf[9] ^= uintBuf[10];
			uintBuf[3] ^= uintBuf[9];
			uintBuf[7] ^= uintBuf[3];
			uintBuf[4] ^= uintBuf[7];
			uintBuf[2] ^= uintBuf[4];
			uintBuf[5] ^= uintBuf[2];
			uintBuf[14] ^= uintBuf[5];
		}

		Encode(buf);
		stream.Seek(offset, SeekOrigin.Begin);
		await stream.WriteAsync(buf, cancellationToken).ConfigureAwait(false);
	}
}
