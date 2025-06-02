// See LICENSE.txt for license information.

using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
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

	/// <inheritdoc/>
	public async Task<(NefsArchive Archive, NefsArchiveSource Source)> WriteNefsInjectArchiveAsync(string dataFilePath, string nefsInjectFilePath, NefsArchive nefs, NefsProgress p)
	{
		NefsArchive newArchive;

		// Setup temp working directory
		var workDir = PrepareWorkingDirectory(dataFilePath);

		// Write to temp file
		var tempDataFilePath = Path.Combine(workDir, "temp.dat");
		var tempNefsInjectFilePath = Path.Combine(workDir, "temp.dat.nefsinject");
		using (var dataFile = FileSystem.File.Open(tempDataFilePath, FileMode.Create))
		using (var nefsInjectFile = FileSystem.File.Open(tempNefsInjectFilePath, FileMode.Create))
		{
			switch (nefs.Header)
			{
				case Nefs200Header v20Header:
					// newArchive = await WriteNefsInjectArchiveVersion20Async(dataFile, nefsInjectFile, v20Header, nefs.Items, workDir, p);
					break;

				case Nefs160Header v16Header:
					// newArchive = await WriteNefsInjectArchiveVersion16Async(dataFile, nefsInjectFile, v16Header, nefs.Items, workDir, p);
					break;

				default:
					throw new NotSupportedException("Unsupported archive version.");
			}
		}

		// Copy to final destination
		FileSystem.File.Copy(tempDataFilePath, dataFilePath, true);
		FileSystem.File.Copy(tempNefsInjectFilePath, nefsInjectFilePath, true);

		var source = NefsArchiveSource.NefsInject(dataFilePath, nefsInjectFilePath);
		return (null, source);
		// return (newArchive, source);
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

	// private async Task<NefsArchive> WriteNefsInjectArchiveVersion16Async(
	// 	Stream dataFileStream,
	// 	Stream nefsInjectFileStream,
	// 	Nefs160Header sourceHeader,
	// 	NefsItemList sourceItems,
	// 	string workDir,
	// 	NefsProgress p)
	// {
	// 	// Setup task weights
	// 	var taskWeightPrepareItems = 0.45f;
	// 	var taskWeightWriteItems = 0.45f;
	// 	var taskWeightHeader = 0.1f;
	//
	// 	// Prepare items for writing
	// 	NefsItemList items;
	// 	using (var t = p.BeginTask(taskWeightPrepareItems, "Preparing items"))
	// 	{
	// 		items = await PrepareItemsAsync(sourceItems, workDir, p);
	// 	}
	//
	// 	// Write item data
	// 	long dataFileSize;
	// 	using (var t = p.BeginTask(taskWeightWriteItems, "Writing items"))
	// 	{
	// 		dataFileSize = await WriteItemsAsync(dataFileStream, items, 0, p);
	// 	}
	//
	// 	var numItems = items.Count;
	// 	var p4 = new Nefs151HeaderBlockTable(items, sourceHeader.BlockTable.UnkownEndValue);
	// 	var p3 = new NefsHeaderPart3(items);
	// 	var p1 = new Nefs160HeaderEntryTable(items, p4);
	// 	var p2 = new Nefs160HeaderSharedEntryInfoTable(items, p3);
	// 	var p6 = new Nefs160HeaderWriteableEntryTable(items);
	// 	var p7 = new Nefs160HeaderWriteableSharedEntryInfo(items);
	//
	// 	var p5 = new NefsHeaderPart5
	// 	{
	// 		DataFileNameStringOffset = p3.OffsetsByFileName[items.DataFileName],
	// 		DataSize = (ulong)dataFileSize,
	// 		FirstDataOffset = sourceHeader.Part5.FirstDataOffset,
	// 	};
	//
	// 	// Part 8 - not writing anything for now
	// 	var hashBlockSize = (int)sourceHeader.TableOfContents.HashBlockSize;
	// 	var numHashes = (int)(dataFileSize / (uint)hashBlockSize);
	// 	var hashes = await HashHelper.HashDataFileBlocksAsync(dataFileStream, p5.FirstDataOffset, hashBlockSize, numHashes, p.CancellationToken);
	//
	// 	var p8 = new Nefs160HeaderHashDigestTable(hashes);
	//
	// 	var introSize = Nefs160HeaderIntro.Size;
	// 	var tocSize = Nefs160HeaderIntroToc.Size;
	// 	var p1Size = p1.Size; // TODO : What about duplicates?
	// 	var p2Size = p2.Size; // TODO : What about duplicates?
	// 	var p3Size = p3.Size;
	// 	var p4Size = p4.Size;
	// 	var p5Size = NefsHeaderPart5.Size;
	// 	var p6Size = p6.Size;
	// 	var p7Size = p7.Size;
	// 	var p8Size = p8.Size;
	//
	// 	var primarySize = introSize +
	// 		tocSize + p1Size + p2Size + p3Size + p4Size + p5Size +
	// 		p8Size;
	//
	// 	var secondarySize = p6Size + p7Size;
	// 	var headerSize = primarySize + secondarySize;
	//
	// 	var p1Offset = (uint)(introSize + tocSize);
	// 	var p2Offset = p1Offset + (uint)p1Size;
	// 	var p3Offset = p2Offset + (uint)p2Size;
	// 	var p4Offset = p3Offset + (uint)p3Size;
	// 	var p5Offset = p4Offset + (uint)p4Size;
	// 	var p8Offset = p5Offset + (uint)p5Size;
	//
	// 	// Update header intro
	// 	var intro = new Nefs160HeaderIntro(sourceHeader.Intro.Data)
	// 	{
	// 		HeaderSize = (uint)headerSize,
	// 		NumberOfItems = (uint)numItems,
	// 	};
	//
	// 	var toc = new Nefs160HeaderIntroToc
	// 	{
	// 		BlockSize = sourceHeader.TableOfContents.BlockSize,
	// 		HashBlockSize = sourceHeader.TableOfContents.HashBlockSize,
	// 		NumVolumes = sourceHeader.TableOfContents.NumVolumes,
	// 		OffsetToPart1 = p1Offset,
	// 		OffsetToPart2 = p2Offset,
	// 		OffsetToPart3 = p3Offset,
	// 		OffsetToPart4 = p4Offset,
	// 		OffsetToPart5 = p5Offset,
	// 		OffsetToPart6 = 0,
	// 		OffsetToPart7 = (uint)p6Size,
	// 		OffsetToPart8 = p8Offset,
	// 		SplitSize = sourceHeader.TableOfContents.SplitSize,
	// 		Unknown0x28 = sourceHeader.TableOfContents.Unknown0x28,
	// 	};
	//
	// 	// Create new header object
	// 	var header = new Nefs160Header(intro, toc, p1, p2, p3, p4, p5, p6, p7, p8);
	//
	// 	NefsInjectHeader nefsInject;
	// 	using var writer = new EndianBinaryWriter(nefsInjectFileStream, header.Intro.IsLittleEndian);
	// 	using (var t = p.BeginTask(taskWeightHeader, "Writing header"))
	// 	{
	// 		nefsInject = await WriteNefsInjectHeaderVersion16Async(writer, header, primarySize, secondarySize, p);
	// 	}
	//
	// 	// Update hash
	// 	var hash = await HashHelper.HashSplitHeaderAsync(nefsInjectFileStream, nefsInject.PrimaryOffset, nefsInject.PrimarySize, nefsInject.SecondaryOffset, nefsInject.SecondarySize);
	// 	header.Intro.ExpectedHash = hash;
	// 	await WriteTocEntryAsync(writer, nefsInject.PrimaryOffset, header.Intro.Data, p).ConfigureAwait(false);
	//
	// 	// Create new archive object
	// 	return new NefsArchive(header, items);
	// }
	//
	// private async Task<NefsArchive> WriteNefsInjectArchiveVersion20Async(
	// 	Stream dataFileStream,
	// 	Stream nefsInjectFileStream,
	// 	Nefs20Header sourceHeader,
	// 	NefsItemList sourceItems,
	// 	string workDir,
	// 	NefsProgress p)
	// {
	// 	// Setup task weights
	// 	var taskWeightPrepareItems = 0.45f;
	// 	var taskWeightWriteItems = 0.45f;
	// 	var taskWeightHeader = 0.1f;
	//
	// 	// Prepare items for writing
	// 	NefsItemList items;
	// 	using (var t = p.BeginTask(taskWeightPrepareItems, "Preparing items"))
	// 	{
	// 		items = await PrepareItemsAsync(sourceItems, workDir, p);
	// 	}
	//
	// 	// Write item data
	// 	long dataFileSize;
	// 	using (var t = p.BeginTask(taskWeightWriteItems, "Writing items"))
	// 	{
	// 		dataFileSize = await WriteItemsAsync(dataFileStream, items, 0, p);
	// 	}
	//
	// 	var numItems = items.Count;
	// 	var p4 = new Nefs200HeaderBlockTable(items);
	// 	var p3 = new NefsHeaderPart3(items);
	// 	var p1 = new Nefs160HeaderEntryTable(items, p4);
	// 	var p2 = new Nefs160HeaderSharedEntryInfoTable(items, p3);
	// 	var p6 = new Nefs20HeaderPart6(items);
	// 	var p7 = new Nefs160HeaderWriteableSharedEntryInfo(items);
	//
	// 	var p5 = new NefsHeaderPart5
	// 	{
	// 		DataFileNameStringOffset = p3.OffsetsByFileName[items.DataFileName],
	// 		DataSize = (ulong)dataFileSize,
	// 		FirstDataOffset = sourceHeader.Part5.FirstDataOffset,
	// 	};
	//
	// 	// Part 8 - not writing anything for now
	// 	//var totalCompressedDataSize = p5.FirstDataOffset - p5.ArchiveSize;
	// 	var hashBlockSize = DefaultHashBlockSize;
	// 	var numHashes = (int)(dataFileSize / (uint)hashBlockSize);
	// 	var hashes = new List<Sha256Hash>();
	// 	for (var i = 0; i < numHashes; ++i)
	// 	{
	// 		hashes.Add(new Sha256Hash(new byte[20]));
	// 	}
	//
	// 	var p8 = new Nefs160HeaderHashDigestTable(hashes);
	//
	// 	var introSize = Nefs160HeaderIntro.Size;
	// 	var tocSize = Nefs20HeaderIntroToc.Size;
	// 	var p1Size = p1.Size; // TODO : What about duplicates?
	// 	var p2Size = p2.Size; // TODO : What about duplicates?
	// 	var p3Size = p3.Size;
	// 	var p4Size = p4.Size;
	// 	var p5Size = NefsHeaderPart5.Size;
	// 	var p6Size = p6.Size;
	// 	var p7Size = p7.Size;
	// 	var p8Size = p8.Size;
	//
	// 	var primarySize = introSize +
	// 		tocSize + p1Size + p2Size + p3Size + p4Size + p5Size +
	// 		p8Size;
	//
	// 	var secondarySize = p6Size + p7Size;
	// 	var headerSize = primarySize + secondarySize;
	//
	// 	var p1Offset = (uint)(introSize + tocSize);
	// 	var p2Offset = p1Offset + (uint)p1Size;
	// 	var p3Offset = p2Offset + (uint)p2Size;
	// 	var p4Offset = p3Offset + (uint)p3Size;
	// 	var p5Offset = p4Offset + (uint)p4Size;
	// 	var p8Offset = p5Offset + (uint)p5Size;
	//
	// 	// Update header intro
	// 	var intro = new Nefs160HeaderIntro(sourceHeader.Intro.Data)
	// 	{
	// 		HeaderSize = (uint)headerSize,
	// 		NumberOfItems = (uint)numItems
	// 	};
	//
	// 	var toc = new Nefs20HeaderIntroToc
	// 	{
	// 		HashBlockSize = sourceHeader.TableOfContents.HashBlockSize,
	// 		NumVolumes = sourceHeader.TableOfContents.NumVolumes,
	// 		OffsetToPart1 = p1Offset,
	// 		OffsetToPart2 = p2Offset,
	// 		OffsetToPart3 = p3Offset,
	// 		OffsetToPart4 = p4Offset,
	// 		OffsetToPart5 = p5Offset,
	// 		OffsetToPart8 = p8Offset,
	// 		OffsetToPart6 = 0,
	// 		OffsetToPart7 = (uint)p6Size,
	// 		Unknown0x24 = sourceHeader.TableOfContents.Unknown0x24,
	// 	};
	//
	// 	// Create new header object
	// 	var header = new Nefs20Header(intro, toc, p1, p2, p3, p4, p5, p6, p7, p8);
	//
	// 	NefsInjectHeader nefsInject;
	// 	using var writer = new EndianBinaryWriter(nefsInjectFileStream, header.Intro.IsLittleEndian);
	// 	using (var t = p.BeginTask(taskWeightHeader, "Writing header"))
	// 	{
	// 		nefsInject = await WriteNefsInjectHeaderVersion20Async(writer, header, primarySize, secondarySize, p);
	// 	}
	//
	// 	// Update hash
	// 	var hash = await HashHelper.HashSplitHeaderAsync(nefsInjectFileStream, nefsInject.PrimaryOffset, nefsInject.PrimarySize, nefsInject.SecondaryOffset, nefsInject.SecondarySize);
	// 	header.Intro.ExpectedHash = hash;
	// 	await WriteTocEntryAsync(writer, nefsInject.PrimaryOffset, header.Intro.Data, p).ConfigureAwait(false);
	//
	// 	// Create new archive object
	// 	return new NefsArchive(header, items);
	// }
	//
	// private async Task<NefsInjectHeader> WriteNefsInjectHeaderVersion16Async(EndianBinaryWriter writer,
	// 	Nefs160Header header, int primarySize, int secondarySize, NefsProgress p)
	// {
	// 	NefsInjectHeader nefsInject;
	//
	// 	// Calc weight of each task (nefsinject + 8 parts + intro + table of contents)
	// 	var weight = 1.0f / 10.0f;
	//
	// 	// Get table of contents
	// 	var toc = header.TableOfContents;
	// 	var primaryOffset = NefsInjectHeader.Size;
	// 	var secondaryOffset = primaryOffset + primarySize;
	//
	// 	var stream = writer.BaseStream;
	// 	using (p.BeginTask(weight, "Writing NesfInject header"))
	// 	{
	// 		nefsInject = new NefsInjectHeader(primaryOffset, primarySize, secondaryOffset, secondarySize);
	// 		await FileData.WriteDataAsync(stream, 0, nefsInject, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header intro"))
	// 	{
	// 		var offset = primaryOffset + Nefs20Header.IntroOffset;
	// 		await WriteHeaderPartAsync(stream, offset, header.Intro, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header intro table of contents"))
	// 	{
	// 		var offset = primaryOffset + Nefs20HeaderIntroToc.Offset;
	// 		await WriteHeaderPartAsync(stream, offset, header.TableOfContents, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 1"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart1;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.EntryTable.EntriesByIndex, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 2"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart2;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.SharedEntryInfoTable.EntriesByIndex, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 3"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart3;
	// 		await WriteHeaderPart3Async(stream, offset, header.Part3, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 4"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart4;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.BlockTable.EntriesByIndex, p);
	//
	// 		// TODO : ???
	// 		var endValue = new UInt32Type(0);
	// 		endValue.Value = header.BlockTable.UnkownEndValue;
	// 		await endValue.WriteAsync(stream, stream.Position, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 5"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart5;
	// 		await WriteTocEntryAsync(writer, offset, header.Part5.Data, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 8"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart8;
	// 		await WriteHeaderPartAsync(stream, offset, header.HashDigestTable, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 6"))
	// 	{
	// 		var offset = secondaryOffset + toc.OffsetToPart6;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.WriteableEntryTable.EntriesByIndex, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 7"))
	// 	{
	// 		var offset = secondaryOffset + toc.OffsetToPart7;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.WriteableSharedEntryInfo.EntriesByIndex, p);
	// 	}
	//
	// 	return nefsInject;
	// }
	//
	// private async Task<NefsInjectHeader> WriteNefsInjectHeaderVersion20Async(EndianBinaryWriter writer,
	// 	Nefs20Header header, int primarySize, int secondarySize, NefsProgress p)
	// {
	// 	NefsInjectHeader nefsInject;
	//
	// 	// Calc weight of each task (nefsinject + 8 parts + intro + table of contents)
	// 	var weight = 1.0f / 10.0f;
	//
	// 	// Get table of contents
	// 	var toc = header.TableOfContents;
	// 	var primaryOffset = NefsInjectHeader.Size;
	// 	var secondaryOffset = primaryOffset + primarySize;
	//
	// 	var stream = writer.BaseStream;
	// 	using (p.BeginTask(weight, "Writing NesfInject header"))
	// 	{
	// 		nefsInject = new NefsInjectHeader(primaryOffset, primarySize, secondaryOffset, secondarySize);
	// 		await FileData.WriteDataAsync(stream, 0, nefsInject, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header intro"))
	// 	{
	// 		var offset = primaryOffset + Nefs20Header.IntroOffset;
	// 		await WriteHeaderPartAsync(stream, offset, header.Intro, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header intro table of contents"))
	// 	{
	// 		var offset = primaryOffset + Nefs20HeaderIntroToc.Offset;
	// 		await WriteHeaderPartAsync(stream, offset, header.TableOfContents, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 1"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart1;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.EntryTable.EntriesByIndex, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 2"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart2;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.SharedEntryInfoTable.EntriesByIndex, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 3"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart3;
	// 		await WriteHeaderPart3Async(stream, offset, header.Part3, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 4"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart4;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.BlockTable.EntriesByIndex, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 5"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart5;
	// 		await WriteTocEntryAsync(writer, offset, header.Part5.Data, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 8"))
	// 	{
	// 		var offset = primaryOffset + toc.OffsetToPart8;
	// 		await WriteHeaderPartAsync(stream, offset, header.HashDigestTable, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 6"))
	// 	{
	// 		var offset = secondaryOffset + toc.OffsetToPart6;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.WriteableEntryTable.EntriesByIndex, p);
	// 	}
	//
	// 	using (var t = p.BeginTask(weight, "Writing header part 7"))
	// 	{
	// 		var offset = secondaryOffset + toc.OffsetToPart7;
	// 		await WriteHeaderPartWithEntriesAsync(stream, offset, header.WriteableSharedEntryInfo.EntriesByIndex, p);
	// 	}
	//
	// 	return nefsInject;
	// }

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
