// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;
    using VictorBush.Ego.NefsLib.Utility;

    /// <summary>
    /// Writes NeFS archives.
    /// </summary>
    public class NefsWriter : INefsWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsWriter"/> class.
        /// </summary>
        /// <param name="tempDirPath">Path to a directory that can be used to write temporary files.</param>
        /// <param name="fileSystem">The file system to use.</param>
        /// <param name="compressor">Interface used to compress data.</param>
        public NefsWriter(
            string tempDirPath,
            IFileSystem fileSystem,
            INefsCompressor compressor)
        {
            this.TempDirectoryPath = tempDirPath ?? throw new ArgumentNullException(nameof(tempDirPath));
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.Compressor = compressor ?? throw new ArgumentNullException(nameof(compressor));
        }

        /// <summary>
        /// The compressor used to compress item data.
        /// </summary>
        private INefsCompressor Compressor { get; }

        /// <summary>
        /// The file system.
        /// </summary>
        private IFileSystem FileSystem { get; }

        /// <summary>
        /// The path of the temporary directory to use when writing.
        /// </summary>
        private string TempDirectoryPath { get; }

        /// <inheritdoc/>
        public async Task<NefsArchive> WriteArchiveAsync(string destFilePath, NefsArchive nefs, NefsProgress p)
        {
            NefsArchive newArchive = null;

            // Setup temp working directory
            var workDir = this.PrepareWorkingDirectory(destFilePath);

            // Write to temp file
            var tempFilePath = Path.Combine(workDir, "temp.nefs");
            using (var file = this.FileSystem.File.Open(tempFilePath, FileMode.Create))
            {
                newArchive = await this.WriteArchiveAsync(file, nefs.Header, nefs.Items, workDir, p);
            }

            // Copy to final destination
            this.FileSystem.File.Copy(tempFilePath, destFilePath, true);

            return newArchive;
        }

        /// <summary>
        /// Writes the header intro to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="intro">The intro to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderIntroAsync(Stream stream, UInt64 offset, NefsHeaderIntro intro, NefsProgress p)
        {
            using (var t = p.BeginTask(1.0f))
            {
                await FileData.WriteDataAsync(stream, offset, intro, p);
            }
        }

        /// <summary>
        /// Writes the header intro table of contents to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="toc">The table of contents to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderIntroTocAsync(Stream stream, UInt64 offset, NefsHeaderIntroToc toc, NefsProgress p)
        {
            using (var t = p.BeginTask(1.0f))
            {
                await FileData.WriteDataAsync(stream, offset, toc, p);
            }
        }

        /// <summary>
        /// Writes the header part to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="part1">The data to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderPart1Async(Stream stream, UInt64 offset, NefsHeaderPart1 part1, NefsProgress p)
        {
            foreach (var entry in part1.EntriesByIndex)
            {
                await FileData.WriteDataAsync(stream, offset, entry, p);
                offset += NefsHeaderPart1Entry.Size;
            }
        }

        /// <summary>
        /// Writes the header part to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="part2">The data to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderPart2Async(Stream stream, UInt64 offset, NefsHeaderPart2 part2, NefsProgress p)
        {
            foreach (var entry in part2.EntriesByIndex)
            {
                await FileData.WriteDataAsync(stream, offset, entry, p);
                offset += NefsHeaderPart2Entry.Size;
            }
        }

        /// <summary>
        /// Writes the header part to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="part3">The data to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderPart3Async(Stream stream, UInt64 offset, NefsHeaderPart3 part3, NefsProgress p)
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

        /// <summary>
        /// Writes the header part to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="part4">The data to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderPart4Async(Stream stream, UInt64 offset, NefsHeaderPart4 part4, NefsProgress p)
        {
            stream.Seek((long)offset, SeekOrigin.Begin);

            foreach (var entry in part4.Entries)
            {
                foreach (var chunkSize in entry.ChunkSizes)
                {
                    var data = BitConverter.GetBytes(chunkSize);
                    await stream.WriteAsync(data, 0, data.Length, p.CancellationToken);
                }
            }
        }

        /// <summary>
        /// Writes the header part to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="part5">The data to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderPart5Async(Stream stream, UInt64 offset, NefsHeaderPart5 part5, NefsProgress p)
        {
            await FileData.WriteDataAsync(stream, offset, part5, p);
        }

        /// <summary>
        /// Writes the header part to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="part6">The data to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderPart6Async(Stream stream, UInt64 offset, NefsHeaderPart6 part6, NefsProgress p)
        {
            foreach (var entry in part6.EntriesByIndex)
            {
                await FileData.WriteDataAsync(stream, offset, entry, p);
                offset += NefsHeaderPart6Entry.Size;
            }
        }

        /// <summary>
        /// Writes the header part to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="part7">The data to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderPart7Async(Stream stream, UInt64 offset, NefsHeaderPart7 part7, NefsProgress p)
        {
            foreach (var entry in part7.EntriesByIndex)
            {
                await FileData.WriteDataAsync(stream, offset, entry, p);
                offset += NefsHeaderPart7Entry.Size;
            }
        }

        /// <summary>
        /// Writes the header part to an output stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="offset">The absolute offset in the stream to write at.</param>
        /// <param name="part8">The data to write.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>An async task.</returns>
        internal async Task WriteHeaderPart8Async(Stream stream, UInt64 offset, NefsHeaderPart8 part8, NefsProgress p)
        {
            await FileData.WriteDataAsync(stream, offset, part8, p);
        }

        /// <summary>
        /// Prepares an item's data to be written to the archive.
        /// </summary>
        /// <param name="item">The item to prepare.</param>
        /// <param name="workDir">The temporary working directory.</param>
        /// <param name="items">The source items list.</param>
        /// <param name="chunkSize">The chunk size to use.</param>
        /// <param name="p">Progress info.</param>
        private async Task PrepareItemAsync(NefsItem item, string workDir, NefsItemList items, UInt32 chunkSize, NefsProgress p)
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
            if (!this.FileSystem.File.Exists(item.DataSource.FilePath))
            {
                throw new IOException($"Cannot find source file {item.DataSource.FilePath}.");
            }

            // Compress to temp location if needed
            if (item.DataSource.ShouldCompress)
            {
                // Prepare the working directory
                var filePathInArchive = items.GetItemFilePath(item.Id);
                var filePathInArchiveHash = HashHelper.HashStringMD5(filePathInArchive);
                var fileWorkDir = Path.Combine(workDir, filePathInArchiveHash);
                this.FileSystem.ResetOrCreateDirectory(fileWorkDir);

                // Compress the file
                var destFilePath = Path.Combine(workDir, "inject.dat");
                var newSize = await this.Compressor.CompressFileAsync(item.DataSource, destFilePath, chunkSize, p);

                // Update data source to point to the compressed temp file
                var dataSource = new NefsFileDataSource(destFilePath, 0, newSize, false);
                item.UpdateDataSource(dataSource, NefsItemState.Replaced);
            }
        }

        /// <summary>
        /// Prepares a new list of items to be written out. The source list is cloned and then
        /// updated. Deleted items are removed. Item data is compressed if needed. Other item
        /// metadata is updated. The original item list is not modified; instead a new, updated list
        /// is returned.
        /// </summary>
        /// <param name="sourceItems">
        /// The source items list to prepare. This list nor its items are modified.
        /// </param>
        /// <param name="workDir">The temporary working directory.</param>
        /// <param name="chunkSize">The chunk size to use.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>A prepared item list ready for writing.</returns>
        private async Task<NefsItemList> PrepareItemsAsync(
            NefsItemList sourceItems,
            string workDir,
            UInt32 chunkSize,
            NefsProgress p)
        {
            // Create a new items list - original source list is not modified. The new list is
            // returned that removes deleted items and has updated metadata for the other items.
            var items = sourceItems.Clone() as NefsItemList;
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
                    await this.PrepareItemAsync(item, workDir, sourceItems, chunkSize, p);
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
        /// The destination nefs archive file path. This is hashed to create a unique directory name
        /// in the <see cref="NefsWriter"/>'s temporary directory <see cref="TempDirectoryPath"/>.
        /// </param>
        private string PrepareWorkingDirectory(string destFilePath)
        {
            // Create temp directory if needed
            if (!this.FileSystem.Directory.Exists(this.TempDirectoryPath))
            {
                this.FileSystem.Directory.CreateDirectory(this.TempDirectoryPath);
            }

            // Create a temp working directory using a hash of the archive's file path
            var destPathHash = HashHelper.HashStringMD5(destFilePath);
            var workDir = Path.Combine(this.TempDirectoryPath, destPathHash);

            // Setup the working directory for this archive
            this.FileSystem.ResetOrCreateDirectory(workDir);

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
            UInt64 headerOffset,
            NefsHeader header,
            NefsProgress p)
        {
            // The hash is of the entire header expect for the expected hash
            var firstOffset = (long)headerOffset;
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
        /// Writes an archive to the specified stream. A new archive obejct is returned that
        /// contains the updated header and item metadata.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="sourceHeader">Donor header information.</param>
        /// <param name="sourceItems">List of items to write. This list is not modified directly.</param>
        /// <param name="workDir">Temp working directory path.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>A new NefsArchive object containing the updated header and item metadata.</returns>
        private async Task<NefsArchive> WriteArchiveAsync(
            Stream stream,
            NefsHeader sourceHeader,
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
                items = await this.PrepareItemsAsync(sourceItems, workDir, NefsHeader.ChunkSize, p);
            }

            // Determine number of items
            var numItems = items.Count;

            // Update header parts 3 and 4 first (need to know their sizes)
            var p4 = new NefsHeaderPart4(items);
            var p3 = new NefsHeaderPart3(items);

            // Compute header size
            var introSize = NefsHeaderIntro.Size;
            var tocSize = NefsHeaderIntroToc.Size;
            var p1Size = numItems * NefsHeaderPart1Entry.Size;
            var p2Size = numItems * NefsHeaderPart2Entry.Size;
            var p3Size = p3.Size;
            var p4Size = p4.Size;
            var p5Size = NefsHeaderPart5.Size;
            var p6Size = numItems * NefsHeaderPart6Entry.Size;
            var p7Size = numItems * NefsHeaderPart7Entry.Size;
            var p8Size = sourceHeader.Intro.HeaderSize - sourceHeader.TableOfContents.OffsetToPart8;
            var headerSize = introSize + tocSize + p1Size + p2Size + p3Size + p4Size + p5Size + p6Size + p7Size + p8Size;

            // Determine first data offset. There are two known offset values. If the header is
            // large enough, the second (larger) offset is used.
            var firstDataOffset = NefsHeader.DataOffsetDefault;
            if (headerSize > firstDataOffset)
            {
                firstDataOffset = NefsHeader.DataOffsetLarge;
            }

            // Write item data
            UInt64 archiveSize;
            using (var t = p.BeginTask(taskWeightWriteItems, "Writing items"))
            {
                archiveSize = await this.WriteItemsAsync(stream, items, firstDataOffset, p);
            }

            // Update remaining header data
            var p1 = new NefsHeaderPart1(items, p4);
            var p2 = new NefsHeaderPart2(items, p3);
            var p6 = new NefsHeaderPart6(items);
            var p7 = new NefsHeaderPart7(items);

            // Compute total archive size
            var p5 = new NefsHeaderPart5();
            p5.Data0x00_ArchiveSize.Value = archiveSize;
            p5.Data0x08_ArchiveNameStringOffset.Value = p3.OffsetsByFileName[items.DataFileName];
            p5.Data0x0C_FirstDataOffset.Value = sourceHeader.Part5.FirstDataOffset;

            // Update header intro
            var intro = new NefsHeaderIntro();
            intro.Data0x00_MagicNumber.Value = sourceHeader.Intro.MagicNumber;
            intro.Data0x24_AesKeyHexString.Value = sourceHeader.Intro.AesKeyHexString;
            intro.Data0x64_HeaderSize.Value = (uint)headerSize;
            intro.Data0x68_Unknown.Value = sourceHeader.Intro.Unknown0x68;
            intro.Data0x6c_NumberOfItems.Value = (uint)numItems;
            intro.Data0x70_UnknownZlib.Value = sourceHeader.Intro.Unknown0x70zlib;
            intro.Data0x78_Unknown.Value = sourceHeader.Intro.Unknown0x78;

            var toc = new NefsHeaderIntroToc();
            toc.Data0x00_Unknown.Value = sourceHeader.TableOfContents.Unknown0x00;
            toc.Data0x04_OffsetToPart1.Value = introSize + tocSize;
            toc.Data0x0c_OffsetToPart2.Value = toc.OffsetToPart1 + (uint)p1Size;
            toc.Data0x14_OffsetToPart3.Value = toc.OffsetToPart2 + (uint)p2Size;
            toc.Data0x18_OffsetToPart4.Value = toc.OffsetToPart3 + (uint)p3Size;
            toc.Data0x1c_OffsetToPart5.Value = toc.OffsetToPart4 + (uint)p4Size;
            toc.Data0x08_OffsetToPart6.Value = toc.OffsetToPart5 + (uint)p5Size;
            toc.Data0x10_OffsetToPart7.Value = toc.OffsetToPart6 + (uint)p6Size;
            toc.Data0x20_OffsetToPart8.Value = toc.OffsetToPart7 + (uint)p7Size;
            toc.Data0x24_Unknown.Value = sourceHeader.TableOfContents.Unknown0x24;

            // Part 8 - not writing anything for now
            var p8 = new NefsHeaderPart8((uint)p8Size);

            // Create new header object
            var header = new NefsHeader(intro, toc, p1, p2, p3, p4, p5, p6, p7, p8);

            // Write the header
            using (var t = p.BeginTask(taskWeightHeader, "Writing header"))
            {
                await this.WriteHeaderAsync(stream, 0, header, p);
            }

            // Update hash
            await this.UpdateHashAsync(stream, 0, header, p);

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
        private async Task WriteHeaderAsync(Stream stream, UInt64 headerOffset, NefsHeader header, NefsProgress p)
        {
            // Calc weight of each task (8 parts + intro + table of contents)
            var weight = 1.0f / 10.0f;

            // Get table of contents
            var toc = header.TableOfContents;

            using (var t = p.BeginTask(weight, "Writing header intro"))
            {
                var offset = headerOffset + NefsHeader.IntroOffset;
                await this.WriteHeaderIntroAsync(stream, offset, header.Intro, p);
            }

            using (var t = p.BeginTask(weight, "Writing header intro table of contents"))
            {
                var offset = headerOffset + NefsHeaderIntroToc.Offset;
                await this.WriteHeaderIntroTocAsync(stream, offset, header.TableOfContents, p);
            }

            using (var t = p.BeginTask(weight, "Writing header part 1"))
            {
                var offset = headerOffset + toc.OffsetToPart1;
                await this.WriteHeaderPart1Async(stream, offset, header.Part1, p);
            }

            using (var t = p.BeginTask(weight, "Writing header part 2"))
            {
                var offset = headerOffset + toc.OffsetToPart2;
                await this.WriteHeaderPart2Async(stream, offset, header.Part2, p);
            }

            using (var t = p.BeginTask(weight, "Writing header part 3"))
            {
                var offset = headerOffset + toc.OffsetToPart3;
                await this.WriteHeaderPart3Async(stream, offset, header.Part3, p);
            }

            using (var t = p.BeginTask(weight, "Writing header part 4"))
            {
                var offset = headerOffset + toc.OffsetToPart4;
                await this.WriteHeaderPart4Async(stream, offset, header.Part4, p);
            }

            using (var t = p.BeginTask(weight, "Writing header part 5"))
            {
                var offset = headerOffset + toc.OffsetToPart5;
                await this.WriteHeaderPart5Async(stream, offset, header.Part5, p);
            }

            using (var t = p.BeginTask(weight, "Writing header part 6"))
            {
                var offset = headerOffset + toc.OffsetToPart6;
                await this.WriteHeaderPart6Async(stream, offset, header.Part6, p);
            }

            using (var t = p.BeginTask(weight, "Writing header part 7"))
            {
                var offset = headerOffset + toc.OffsetToPart7;
                await this.WriteHeaderPart7Async(stream, offset, header.Part7, p);
            }

            using (var t = p.BeginTask(weight, "Writing header part 8"))
            {
                var offset = headerOffset + toc.OffsetToPart8;
                await this.WriteHeaderPart8Async(stream, offset, header.Part8, p);
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
        private async Task<UInt64> WriteItemAsync(
            Stream stream,
            UInt64 dataOffset,
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
            if (!this.FileSystem.File.Exists(item.DataSource.FilePath))
            {
                throw new IOException($"Cannot find source file {item.DataSource.FilePath}, skipping");
            }

            // Seek to appropriate offset in destination
            stream.Seek((long)dataOffset, SeekOrigin.Begin);

            // Determine data source
            var srcFile = item.DataSource.FilePath;
            var srcOffset = item.DataSource.Offset;
            var srcSize = item.DataSource.Size.Size;

            // The data should already be compressed (if needed) by this point
            if (item.DataSource.ShouldCompress)
            {
                throw new InvalidOperationException($"Item data compresseion should be handled before calling {nameof(this.WriteItemAsync)}.");
            }

            // There are weird things with non-compressed files. Checking if:
            // - Not compressed
            // - Not the 1 byte item at the end of car archives
            // - Not in an encrypted archive
            if (item.CompressedSize == item.ExtractedSize
                && item.ExtractedSize != 1
                && item.Part6Unknown0x02 != 3)
            {
                // Add 8 bytes to the size for some reason
                srcSize += 0x8;
            }

            // Copy data from data source to the destination stream
            using (var inputFile = this.FileSystem.File.OpenRead(srcFile))
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
        /// <param name="firstDataOffset">
        /// The offset from the beginning of the stream to write the first data.
        /// </param>
        /// <param name="p">Progress info.</param>
        /// <returns>The offset to the end of the last data written.</returns>
        private async Task<UInt64> WriteItemsAsync(
            Stream stream,
            NefsItemList items,
            UInt64 firstDataOffset,
            NefsProgress p)
        {
            var nextDataOffset = firstDataOffset;

            // Prepare stream
            stream.Seek((long)firstDataOffset, SeekOrigin.Begin);

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
                    nextDataOffset = await this.WriteItemAsync(stream, itemOffset, item, p);

                    // Update item data source to point to the newly written data
                    var dataSource = new NefsItemListDataSource(items, itemOffset, itemSize);
                    item.UpdateDataSource(dataSource, NefsItemState.None);
                }

                i++;
            }

            // Return the next data offset, which is the end of the written data
            return nextDataOffset;
        }
    }
}
