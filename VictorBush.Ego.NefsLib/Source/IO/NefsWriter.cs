// See LICENSE.txt for license information.

using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Source.Utility;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.IO;

/// <summary>
/// Writes NeFS archives.
/// </summary>
public class NefsWriter : INefsWriter
{
	internal const int DefaultHashBlockSize = 0x800000;

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

		// Check header version
		if (nefs.Header is not Nefs20Header header)
		{
			throw new NotSupportedException("Can only write v2.0 NeFS files right now.");
		}

		// Setup temp working directory
		var workDir = PrepareWorkingDirectory(destFilePath);

		// Write to temp file
		var tempFilePath = Path.Combine(workDir, "temp.nefs");
		using (var file = FileSystem.File.Open(tempFilePath, FileMode.Create))
		{
			newArchive = await WriteArchiveVersion20Async(file, header, nefs.Items, workDir, p);
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
				case Nefs20Header v20Header:
					newArchive = await WriteNefsInjectArchiveVersion20Async(dataFile, nefsInjectFile, v20Header, nefs.Items, workDir, p);
					break;

				case Nefs16Header v16Header:
					newArchive = await WriteNefsInjectArchiveVersion16Async(dataFile, nefsInjectFile, v16Header, nefs.Items, workDir, p);
					break;

				default:
					throw new NotSupportedException("Unsupported archive version.");
			}
		}

		// Copy to final destination
		FileSystem.File.Copy(tempDataFilePath, dataFilePath, true);
		FileSystem.File.Copy(tempNefsInjectFilePath, nefsInjectFilePath, true);

		var source = NefsArchiveSource.NefsInject(dataFilePath, nefsInjectFilePath);
		return (newArchive, source);
	}

	/// <summary>
	/// Writes the header part to an output stream.
	/// </summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="offset">The absolute offset in the stream to write at.</param>
	/// <param name="part3">The data to write.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>An async task.</returns>
	internal async Task WriteHeaderPart3Async(Stream stream, long offset, NefsHeaderPart3 part3, NefsProgress p)
	{
		stream.Seek((long)offset, SeekOrigin.Begin);

		foreach (var entry in part3.FileNames)
		{
			var fileNameBytes = Encoding.ASCII.GetBytes(entry);
			await stream.WriteAsync(fileNameBytes, 0, fileNameBytes.Length, p.CancellationToken);

			// Write null terminator
			await stream.WriteAsync(new byte[] { 0 }, 0, 1, p.CancellationToken);
		}
	}

	internal async Task WriteHeaderPartAsync(Stream stream, long offset, object part, NefsProgress p)
	{
		using (var t = p.BeginTask(1.0f))
		{
			await FileData.WriteDataAsync(stream, offset, part, p);
		}
	}

	internal async Task WriteHeaderPartWithEntriesAsync(Stream stream, long offset, IEnumerable<INefsHeaderPartEntry> entries, NefsProgress p)
	{
		foreach (var entry in entries)
		{
			await FileData.WriteDataAsync(stream, offset, entry, p);
			offset += entry.Size;
		}
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
		if (!item.DataSource.IsTransformed && item.Transform is not null)
		{
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
		// Create a new items list - original source list is not modified. The new list is returned that removes deleted
		// items and has updated metadata for the other items.
		var items = (NefsItemList)sourceItems.Clone();
		var itemsToRemove = new List<NefsItem>();

		foreach (var item in items.EnumerateById())
		{
			if (item.State == NefsItemState.Removed)
			{
				// Item was deleted; remove item from list
				itemsToRemove.Add(item);
			}
			else
			{
				// Compress any new or replaced files and update chunk sizes
				await PrepareItemAsync(item, workDir, sourceItems, p);
			}
		}

		// Remove deleted items
		foreach (var item in itemsToRemove)
		{
			items.Remove(item.Id);
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
	/// Gets the new expected hash and writes it to the header.
	/// </summary>
	/// <param name="stream">The stream containing the header.</param>
	/// <param name="headerOffset">The offset to the header in the stream.</param>
	/// <param name="header">The header.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The async task.</returns>
	private async Task UpdateHashAsync(
		Stream stream,
		long headerOffset,
		Nefs20Header header,
		NefsProgress p)
	{
		// The hash is of the entire header expect for the expected hash
		var firstOffset = headerOffset;
		var secondOffset = firstOffset + 0x24;
		var headerSize = (int)header.Intro.HeaderSize;

		// Seek to beginning of header
		stream.Seek(firstOffset, SeekOrigin.Begin);

		// Read magic num
		var dataToHash = new byte[headerSize - 0x20];
		await stream.ReadAsync(dataToHash, 0, 4);

		// Skip expected hash and read rest of header
		stream.Seek(secondOffset, SeekOrigin.Begin);
		stream.Read(dataToHash, 4, headerSize - 0x24);

		// Compute the new expected hash
		using (var hash = SHA256.Create())
		{
			byte[] hashOut = hash.ComputeHash(dataToHash);

			// Write the expected hash
			header.Intro.Data0x04_ExpectedHash.Value = hashOut;
			await header.Intro.Data0x04_ExpectedHash.WriteAsync(stream, headerOffset, p);
		}
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
	private async Task<NefsArchive> WriteArchiveVersion20Async(
		Stream stream,
		Nefs20Header sourceHeader,
		NefsItemList sourceItems,
		string workDir,
		NefsProgress p)
	{
		// Setup task weights
		var taskWeightPrepareItems = 0.45f;
		var taskWeightWriteItems = 0.45f;
		var taskWeightHeader = 0.1f;

		// Prepare items for writing
		NefsItemList items;
		using (var t = p.BeginTask(taskWeightPrepareItems, "Preparing items"))
		{
			items = await PrepareItemsAsync(sourceItems, workDir, p);
		}

		// Determine number of items
		var numItems = items.Count;

		// Update header parts 3 and 4 first (need to know their sizes)
		var p4 = new Nefs20HeaderPart4(items);
		var p3 = new NefsHeaderPart3(items);

		// Compute header size
		var introSize = NefsHeaderIntro.Size;
		var tocSize = Nefs20HeaderIntroToc.Size;
		var p1Size = numItems * NefsHeaderPart1.EntrySize; // TODO : What about duplicates?
		var p2Size = numItems * NefsHeaderPart2.EntrySize; // TODO : What about duplicates?
		var p3Size = p3.Size;
		var p4Size = p4.Size;
		var p5Size = NefsHeaderPart5.Size;
		var p6Size = numItems * Nefs20HeaderPart6.EntrySize;
		var p7Size = numItems * NefsHeaderPart7.EntrySize;
		var p8Size = sourceHeader.Intro.HeaderSize - sourceHeader.TableOfContents.OffsetToPart8;
		var headerSize = introSize + tocSize + p1Size + p2Size + p3Size + p4Size + p5Size + p6Size + p7Size + p8Size;

		var p1Offset = (uint)(introSize + tocSize);
		var p2Offset = p1Offset + (uint)p1Size;
		var p3Offset = p2Offset + (uint)p2Size;
		var p4Offset = p3Offset + (uint)p3Size;
		var p5Offset = p4Offset + (uint)p4Size;
		var p6Offset = p5Offset + (uint)p5Size;
		var p7Offset = p6Offset + (uint)p6Size;
		var p8Offset = p7Offset + (uint)p7Size;

		// Determine first data offset. There are two known offset values. If the header is large enough, the second
		// (larger) offset is used.
		var firstDataOffset = Nefs20Header.DataOffsetDefault;
		if (headerSize > firstDataOffset)
		{
			firstDataOffset = Nefs20Header.DataOffsetLarge;
		}

		// Write item data
		long dataSize;
		using (var t = p.BeginTask(taskWeightWriteItems, "Writing items"))
		{
			dataSize = await WriteItemsAsync(stream, items, firstDataOffset, p);
		}

		// Update remaining header data
		var p1 = new NefsHeaderPart1(items, p4);
		var p2 = new NefsHeaderPart2(items, p3);
		var p6 = new Nefs20HeaderPart6(items);
		var p7 = new NefsHeaderPart7(items);

		// Compute total archive size
		var p5 = new NefsHeaderPart5
		{
			DataFileNameStringOffset = p3.OffsetsByFileName[items.DataFileName],
			DataSize = (ulong)dataSize,
			FirstDataOffset = sourceHeader.Part5.FirstDataOffset,
		};

		// Update header intro
		var intro = new NefsHeaderIntro
		{
			AesKeyHexString = sourceHeader.Intro.AesKeyHexString,
			HeaderSize = (uint)headerSize,
			MagicNumber = sourceHeader.Intro.MagicNumber,
			NefsVersion = sourceHeader.Intro.NefsVersion,
			NumberOfItems = (uint)numItems,
			Unknown0x70zlib = sourceHeader.Intro.Unknown0x70zlib,
			Unknown0x78 = sourceHeader.Intro.Unknown0x78,
		};

		var toc = new Nefs20HeaderIntroToc
		{
			HashBlockSize = sourceHeader.TableOfContents.HashBlockSize,
			NumVolumes = sourceHeader.TableOfContents.NumVolumes,
			OffsetToPart1 = p1Offset,
			OffsetToPart2 = p2Offset,
			OffsetToPart3 = p3Offset,
			OffsetToPart4 = p4Offset,
			OffsetToPart5 = p5Offset,
			OffsetToPart6 = p6Offset,
			OffsetToPart7 = p7Offset,
			OffsetToPart8 = p8Offset,
			Unknown0x24 = sourceHeader.TableOfContents.Unknown0x24,
		};

		// Part 8 - not writing anything for now
		var totalCompressedDataSize = p5.FirstDataOffset - p5.DataSize;
		var hashBlockSize = toc.HashBlockSize > 0 ? toc.HashBlockSize : DefaultHashBlockSize;
		var numHashes = (int)(totalCompressedDataSize / hashBlockSize);
		var hashes = new List<Sha256Hash>();
		for (var i = 0; i < numHashes; ++i)
		{
			hashes.Add(new Sha256Hash(new byte[20]));
		}

		var p8 = new NefsHeaderPart8(hashes);

		// Create new header object
		var header = new Nefs20Header(intro, toc, p1, p2, p3, p4, p5, p6, p7, p8);

		// Write the header
		using (var t = p.BeginTask(taskWeightHeader, "Writing header"))
		{
			await WriteHeaderVersion20Async(stream, 0, header, p);
		}

		// Update hash
		await UpdateHashAsync(stream, 0, header, p);

		// Create new archive object
		return new NefsArchive(header, items);
	}

	/// <summary>
	/// Writes the header to the output stream.
	/// </summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="headerOffset">The offset into the stream to begin.</param>
	/// <param name="header">The header to write.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The async task.</returns>
	private async Task WriteHeaderVersion20Async(Stream stream, long headerOffset, Nefs20Header header, NefsProgress p)
	{
		// Calc weight of each task (8 parts + intro + table of contents)
		var weight = 1.0f / 10.0f;

		// Get table of contents
		var toc = header.TableOfContents;

		using (var t = p.BeginTask(weight, "Writing header intro"))
		{
			var offset = headerOffset + Nefs20Header.IntroOffset;
			await WriteHeaderPartAsync(stream, offset, header.Intro, p);
		}

		using (var t = p.BeginTask(weight, "Writing header intro table of contents"))
		{
			var offset = headerOffset + Nefs20HeaderIntroToc.Offset;
			await WriteHeaderPartAsync(stream, offset, header.TableOfContents, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 1"))
		{
			var offset = headerOffset + toc.OffsetToPart1;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part1.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 2"))
		{
			var offset = headerOffset + toc.OffsetToPart2;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part2.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 3"))
		{
			var offset = headerOffset + toc.OffsetToPart3;
			await WriteHeaderPart3Async(stream, offset, header.Part3, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 4"))
		{
			var offset = headerOffset + toc.OffsetToPart4;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part4.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 5"))
		{
			var offset = headerOffset + toc.OffsetToPart5;
			await WriteHeaderPartAsync(stream, offset, header.Part5, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 6"))
		{
			var offset = headerOffset + toc.OffsetToPart6;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part6.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 7"))
		{
			var offset = headerOffset + toc.OffsetToPart7;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part7.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 8"))
		{
			var offset = headerOffset + toc.OffsetToPart8;
			await WriteHeaderPartAsync(stream, offset, header.Part8, p);
		}
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
		stream.Seek((long)dataOffset, SeekOrigin.Begin);

		// Determine data source
		var srcFile = item.DataSource.FilePath;
		var srcOffset = item.DataSource.Offset;
		var srcSize = item.DataSource.Size.TransformedSize;

		// The data should already be transformed (if needed) by this point
		if (!item.DataSource.IsTransformed)
		{
			throw new InvalidOperationException($"Item data transformation should be handled before calling {nameof(this.WriteItemAsync)}.");
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
			inputFile.Seek((long)srcOffset, SeekOrigin.Begin);

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
			using (var t = p.BeginSubTask(1.0f / items.Count, $"Writing data for item {i}/{items.Count}"))
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

	private async Task<NefsArchive> WriteNefsInjectArchiveVersion16Async(
		Stream dataFileStream,
		Stream nefsInjectFileStream,
		Nefs16Header sourceHeader,
		NefsItemList sourceItems,
		string workDir,
		NefsProgress p)
	{
		// Setup task weights
		var taskWeightPrepareItems = 0.45f;
		var taskWeightWriteItems = 0.45f;
		var taskWeightHeader = 0.1f;

		// Prepare items for writing
		NefsItemList items;
		using (var t = p.BeginTask(taskWeightPrepareItems, "Preparing items"))
		{
			items = await PrepareItemsAsync(sourceItems, workDir, p);
		}

		// Write item data
		long dataFileSize;
		using (var t = p.BeginTask(taskWeightWriteItems, "Writing items"))
		{
			dataFileSize = await WriteItemsAsync(dataFileStream, items, 0, p);
		}

		var numItems = items.Count;
		var p4 = new Nefs16HeaderPart4(items, sourceHeader.Part4.UnkownEndValue);
		var p3 = new NefsHeaderPart3(items);
		var p1 = new NefsHeaderPart1(items, p4);
		var p2 = new NefsHeaderPart2(items, p3);
		var p6 = new Nefs16HeaderPart6(items);
		var p7 = new NefsHeaderPart7(items);

		var p5 = new NefsHeaderPart5
		{
			DataFileNameStringOffset = p3.OffsetsByFileName[items.DataFileName],
			DataSize = (ulong)dataFileSize,
			FirstDataOffset = sourceHeader.Part5.FirstDataOffset,
		};

		// Part 8 - not writing anything for now
		var hashBlockSize = (int)sourceHeader.TableOfContents.HashBlockSize;
		var numHashes = (int)(dataFileSize / (uint)hashBlockSize);
		var hashes = await HashHelper.HashDataFileBlocksAsync(dataFileStream, p5.FirstDataOffset, hashBlockSize, numHashes, p.CancellationToken);

		var p8 = new NefsHeaderPart8(hashes);

		var introSize = NefsHeaderIntro.Size;
		var tocSize = Nefs16HeaderIntroToc.Size;
		var p1Size = p1.Size; // TODO : What about duplicates?
		var p2Size = p2.Size; // TODO : What about duplicates?
		var p3Size = p3.Size;
		var p4Size = p4.Size;
		var p5Size = NefsHeaderPart5.Size;
		var p6Size = p6.Size;
		var p7Size = p7.Size;
		var p8Size = p8.Size;

		var primarySize = introSize +
			tocSize + p1Size + p2Size + p3Size + p4Size + p5Size +
			p8Size;

		var secondarySize = p6Size + p7Size;
		var headerSize = primarySize + secondarySize;

		var p1Offset = (uint)(introSize + tocSize);
		var p2Offset = p1Offset + (uint)p1Size;
		var p3Offset = p2Offset + (uint)p2Size;
		var p4Offset = p3Offset + (uint)p3Size;
		var p5Offset = p4Offset + (uint)p4Size;
		var p8Offset = p5Offset + (uint)p5Size;

		// Update header intro
		var intro = new NefsHeaderIntro
		{
			AesKeyHexString = sourceHeader.Intro.AesKeyHexString,
			HeaderSize = (uint)headerSize,
			MagicNumber = sourceHeader.Intro.MagicNumber,
			NefsVersion = sourceHeader.Intro.NefsVersion,
			NumberOfItems = (uint)numItems,
			Unknown0x70zlib = sourceHeader.Intro.Unknown0x70zlib,
			Unknown0x78 = sourceHeader.Intro.Unknown0x78,
		};

		var toc = new Nefs16HeaderIntroToc
		{
			BlockSize = sourceHeader.TableOfContents.BlockSize,
			HashBlockSize = sourceHeader.TableOfContents.HashBlockSize,
			NumVolumes = sourceHeader.TableOfContents.NumVolumes,
			OffsetToPart1 = p1Offset,
			OffsetToPart2 = p2Offset,
			OffsetToPart3 = p3Offset,
			OffsetToPart4 = p4Offset,
			OffsetToPart5 = p5Offset,
			OffsetToPart6 = 0,
			OffsetToPart7 = (uint)p6Size,
			OffsetToPart8 = p8Offset,
			SplitSize = sourceHeader.TableOfContents.SplitSize,
			Unknown0x28 = sourceHeader.TableOfContents.Unknown0x28,
		};

		// Create new header object
		var header = new Nefs16Header(intro, toc, p1, p2, p3, p4, p5, p6, p7, p8);

		NefsInjectHeader nefsInject;
		using (var t = p.BeginTask(taskWeightHeader, "Writing header"))
		{
			nefsInject = await WriteNefsInjectHeaderVersion16Async(nefsInjectFileStream, header, primarySize, secondarySize, p);
		}

		// Update hash
		var hash = await HashHelper.HashSplitHeaderAsync(nefsInjectFileStream, nefsInject.PrimaryOffset, nefsInject.PrimarySize, nefsInject.SecondaryOffset, nefsInject.SecondarySize);
		header.Intro.Data0x04_ExpectedHash.Value = hash.Value;
		await header.Intro.Data0x04_ExpectedHash.WriteAsync(nefsInjectFileStream, nefsInject.PrimaryOffset, p);

		// Create new archive object
		return new NefsArchive(header, items);
	}

	private async Task<NefsArchive> WriteNefsInjectArchiveVersion20Async(
		Stream dataFileStream,
		Stream nefsInjectFileStream,
		Nefs20Header sourceHeader,
		NefsItemList sourceItems,
		string workDir,
		NefsProgress p)
	{
		// Setup task weights
		var taskWeightPrepareItems = 0.45f;
		var taskWeightWriteItems = 0.45f;
		var taskWeightHeader = 0.1f;

		// Prepare items for writing
		NefsItemList items;
		using (var t = p.BeginTask(taskWeightPrepareItems, "Preparing items"))
		{
			items = await PrepareItemsAsync(sourceItems, workDir, p);
		}

		// Write item data
		long dataFileSize;
		using (var t = p.BeginTask(taskWeightWriteItems, "Writing items"))
		{
			dataFileSize = await WriteItemsAsync(dataFileStream, items, 0, p);
		}

		var numItems = items.Count;
		var p4 = new Nefs20HeaderPart4(items);
		var p3 = new NefsHeaderPart3(items);
		var p1 = new NefsHeaderPart1(items, p4);
		var p2 = new NefsHeaderPart2(items, p3);
		var p6 = new Nefs20HeaderPart6(items);
		var p7 = new NefsHeaderPart7(items);

		var p5 = new NefsHeaderPart5
		{
			DataFileNameStringOffset = p3.OffsetsByFileName[items.DataFileName],
			DataSize = (ulong)dataFileSize,
			FirstDataOffset = sourceHeader.Part5.FirstDataOffset,
		};

		// Part 8 - not writing anything for now
		//var totalCompressedDataSize = p5.FirstDataOffset - p5.ArchiveSize;
		var hashBlockSize = DefaultHashBlockSize;
		var numHashes = (int)(dataFileSize / (uint)hashBlockSize);
		var hashes = new List<Sha256Hash>();
		for (var i = 0; i < numHashes; ++i)
		{
			hashes.Add(new Sha256Hash(new byte[20]));
		}

		var p8 = new NefsHeaderPart8(hashes);

		var introSize = NefsHeaderIntro.Size;
		var tocSize = Nefs20HeaderIntroToc.Size;
		var p1Size = p1.Size; // TODO : What about duplicates?
		var p2Size = p2.Size; // TODO : What about duplicates?
		var p3Size = p3.Size;
		var p4Size = p4.Size;
		var p5Size = NefsHeaderPart5.Size;
		var p6Size = p6.Size;
		var p7Size = p7.Size;
		var p8Size = p8.Size;

		var primarySize = introSize +
			tocSize + p1Size + p2Size + p3Size + p4Size + p5Size +
			p8Size;

		var secondarySize = p6Size + p7Size;
		var headerSize = primarySize + secondarySize;

		var p1Offset = (uint)(introSize + tocSize);
		var p2Offset = p1Offset + (uint)p1Size;
		var p3Offset = p2Offset + (uint)p2Size;
		var p4Offset = p3Offset + (uint)p3Size;
		var p5Offset = p4Offset + (uint)p4Size;
		var p8Offset = p5Offset + (uint)p5Size;

		// Update header intro
		var intro = new NefsHeaderIntro
		{
			AesKeyHexString = sourceHeader.Intro.AesKeyHexString,
			HeaderSize = (uint)headerSize,
			MagicNumber = sourceHeader.Intro.MagicNumber,
			NefsVersion = sourceHeader.Intro.NefsVersion,
			NumberOfItems = (uint)numItems,
			Unknown0x70zlib = sourceHeader.Intro.Unknown0x70zlib,
			Unknown0x78 = sourceHeader.Intro.Unknown0x78,
		};

		var toc = new Nefs20HeaderIntroToc
		{
			HashBlockSize = sourceHeader.TableOfContents.HashBlockSize,
			NumVolumes = sourceHeader.TableOfContents.NumVolumes,
			OffsetToPart1 = p1Offset,
			OffsetToPart2 = p2Offset,
			OffsetToPart3 = p3Offset,
			OffsetToPart4 = p4Offset,
			OffsetToPart5 = p5Offset,
			OffsetToPart8 = p8Offset,
			OffsetToPart6 = 0,
			OffsetToPart7 = (uint)p6Size,
			Unknown0x24 = sourceHeader.TableOfContents.Unknown0x24,
		};

		// Create new header object
		var header = new Nefs20Header(intro, toc, p1, p2, p3, p4, p5, p6, p7, p8);

		NefsInjectHeader nefsInject;
		using (var t = p.BeginTask(taskWeightHeader, "Writing header"))
		{
			nefsInject = await WriteNefsInjectHeaderVersion20Async(nefsInjectFileStream, header, primarySize, secondarySize, p);
		}

		// Update hash
		var hash = await HashHelper.HashSplitHeaderAsync(nefsInjectFileStream, nefsInject.PrimaryOffset, nefsInject.PrimarySize, nefsInject.SecondaryOffset, nefsInject.SecondarySize);
		header.Intro.Data0x04_ExpectedHash.Value = hash.Value;
		await header.Intro.Data0x04_ExpectedHash.WriteAsync(nefsInjectFileStream, nefsInject.PrimaryOffset, p);

		// Create new archive object
		return new NefsArchive(header, items);
	}

	private async Task<NefsInjectHeader> WriteNefsInjectHeaderVersion16Async(Stream stream, Nefs16Header header, int primarySize, int secondarySize, NefsProgress p)
	{
		NefsInjectHeader nefsInject;

		// Calc weight of each task (nefsinject + 8 parts + intro + table of contents)
		var weight = 1.0f / 10.0f;

		// Get table of contents
		var toc = header.TableOfContents;
		var primaryOffset = NefsInjectHeader.Size;
		var secondaryOffset = primaryOffset + primarySize;

		using (p.BeginTask(weight, "Writing NesfInject header"))
		{
			nefsInject = new NefsInjectHeader(primaryOffset, primarySize, secondaryOffset, secondarySize);
			await FileData.WriteDataAsync(stream, 0, nefsInject, p);
		}

		using (var t = p.BeginTask(weight, "Writing header intro"))
		{
			var offset = primaryOffset + Nefs20Header.IntroOffset;
			await WriteHeaderPartAsync(stream, offset, header.Intro, p);
		}

		using (var t = p.BeginTask(weight, "Writing header intro table of contents"))
		{
			var offset = primaryOffset + Nefs20HeaderIntroToc.Offset;
			await WriteHeaderPartAsync(stream, offset, header.TableOfContents, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 1"))
		{
			var offset = primaryOffset + toc.OffsetToPart1;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part1.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 2"))
		{
			var offset = primaryOffset + toc.OffsetToPart2;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part2.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 3"))
		{
			var offset = primaryOffset + toc.OffsetToPart3;
			await WriteHeaderPart3Async(stream, offset, header.Part3, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 4"))
		{
			var offset = primaryOffset + toc.OffsetToPart4;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part4.EntriesByIndex, p);

			// TODO : ???
			var endValue = new UInt32Type(0);
			endValue.Value = header.Part4.UnkownEndValue;
			await endValue.WriteAsync(stream, stream.Position, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 5"))
		{
			var offset = primaryOffset + toc.OffsetToPart5;
			await WriteHeaderPartAsync(stream, offset, header.Part5, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 8"))
		{
			var offset = primaryOffset + toc.OffsetToPart8;
			await WriteHeaderPartAsync(stream, offset, header.Part8, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 6"))
		{
			var offset = secondaryOffset + toc.OffsetToPart6;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part6.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 7"))
		{
			var offset = secondaryOffset + toc.OffsetToPart7;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part7.EntriesByIndex, p);
		}

		return nefsInject;
	}

	private async Task<NefsInjectHeader> WriteNefsInjectHeaderVersion20Async(Stream stream, Nefs20Header header, int primarySize, int secondarySize, NefsProgress p)
	{
		NefsInjectHeader nefsInject;

		// Calc weight of each task (nefsinject + 8 parts + intro + table of contents)
		var weight = 1.0f / 10.0f;

		// Get table of contents
		var toc = header.TableOfContents;
		var primaryOffset = NefsInjectHeader.Size;
		var secondaryOffset = primaryOffset + primarySize;

		using (p.BeginTask(weight, "Writing NesfInject header"))
		{
			nefsInject = new NefsInjectHeader(primaryOffset, primarySize, secondaryOffset, secondarySize);
			await FileData.WriteDataAsync(stream, 0, nefsInject, p);
		}

		using (var t = p.BeginTask(weight, "Writing header intro"))
		{
			var offset = primaryOffset + Nefs20Header.IntroOffset;
			await WriteHeaderPartAsync(stream, offset, header.Intro, p);
		}

		using (var t = p.BeginTask(weight, "Writing header intro table of contents"))
		{
			var offset = primaryOffset + Nefs20HeaderIntroToc.Offset;
			await WriteHeaderPartAsync(stream, offset, header.TableOfContents, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 1"))
		{
			var offset = primaryOffset + toc.OffsetToPart1;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part1.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 2"))
		{
			var offset = primaryOffset + toc.OffsetToPart2;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part2.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 3"))
		{
			var offset = primaryOffset + toc.OffsetToPart3;
			await WriteHeaderPart3Async(stream, offset, header.Part3, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 4"))
		{
			var offset = primaryOffset + toc.OffsetToPart4;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part4.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 5"))
		{
			var offset = primaryOffset + toc.OffsetToPart5;
			await WriteHeaderPartAsync(stream, offset, header.Part5, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 8"))
		{
			var offset = primaryOffset + toc.OffsetToPart8;
			await WriteHeaderPartAsync(stream, offset, header.Part8, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 6"))
		{
			var offset = secondaryOffset + toc.OffsetToPart6;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part6.EntriesByIndex, p);
		}

		using (var t = p.BeginTask(weight, "Writing header part 7"))
		{
			var offset = secondaryOffset + toc.OffsetToPart7;
			await WriteHeaderPartWithEntriesAsync(stream, offset, header.Part7.EntriesByIndex, p);
		}

		return nefsInject;
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

		var buf = new byte[NefsHeaderIntro.Size];
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
