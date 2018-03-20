using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib
{
    public class NefsItem
    {
        private static readonly ILog log = LogHelper.GetLogger();

        /// <summary>
        /// Size of data chunks a file is broken up into before
        /// each chunk is compressed and inserted into the archive.
        /// </summary>
        const int CHUNK_SIZE = 0x10000;

        NefsArchive _archive;
        List<UInt32> _chunkSizes = new List<uint>();
        UInt32 _compressedSize;
        UInt64 _dataOffset;
        UInt32 _extractedSize;
        string _fileName;
        string _fileNameHash;
        string _filePathInArchive;
        string _filePathInArchiveHash;
        UInt32 _id;
        bool _pendingInjection;
        string _fileToInject;
        UInt32 _offsetIntoPt2Raw;
        UInt32 _offsetIntoPt4Raw;
        NefsHeaderPt1Entry _pt1Entry;
        NefsHeaderPt2Entry _pt2Entry;
        NefsHeaderPt5Entry _pt5Entry;
        NefsHeaderPt6Entry _pt6Entry;
        NefsItemType _type;

        public enum NefsItemType
        {
            File,
            Directory,
            Archive
        }

        /// <summary>
        /// Loads a NeFS item with the specified id from the archive.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="archive">The NeFS archive this item is in.</param>
        /// <param name="id">The id of the item in the archive to load.</param>
        public NefsItem(FileStream file, NefsArchive archive, UInt32 id)
        {
            /* Validate inputs */
            if( file == null)
            {
                throw new ArgumentNullException("File stream required to load NeFS item.");
            }

            if (archive == null)
            {
                throw new ArgumentNullException("NeFS archive object required to load an item.");
            }

            _archive = archive;
            _id = id;

            /* Get header entries related to this item */
            _pt1Entry = archive.Header.Part1.GetEntry(id);
            _pt2Entry = archive.Header.Part2.GetEntry(_pt1Entry);
            _pt5Entry = archive.Header.Part5.GetEntry(id);
            try
            {
                _pt6Entry = archive.Header.Part6.GetEntry(id);
            }
            catch (Exception ex)
            {
                try
                {
                    /* Some items share a part 2 entry, so try to find the corresponding part 6 entry */
                    _pt6Entry = archive.Header.Part6.GetEntry(_pt2Entry.Id);
                }
                catch (Exception ex2)
                {
                    // TODO : Handle when an item doesn't have a part 6 entry
                    _pt6Entry = archive.Header.Part6.GetEntry(0);
                }
            }

            /* Determine item type */
            _type = (_pt1Entry.OffsetToData == 0) 
                ? NefsItemType.Directory 
                : NefsItemType.File;

            /* Get the filename */
            _fileName = archive.Header.Part3.GetFilename(_pt2Entry.FilenameOffset);

            /* Hash the filename */
            _fileNameHash = FilePathHelper.HashStringMD5(_fileName);

            /* Get offsets */
            _dataOffset = _pt1Entry.OffsetToData;
            _offsetIntoPt2Raw = _pt1Entry.OffsetIntoPt2Raw;
            _offsetIntoPt4Raw = _pt1Entry.OffsetIntoPt4Raw;

            /* Get extracted size */
            _extractedSize = _pt2Entry.ExtractedSize;

            /* 
             * Build the file path inside this archive
             * for example: "rootDir/childDir/file.xml". 
             */
            _filePathInArchive = _fileName;
            var currentItem = _pt2Entry;

            /* The root directory's id is equal to its parent directory id */
            while (currentItem.Id != currentItem.DirectoryId)
            {
                var pt1Entry = archive.Header.Part1.GetEntry(currentItem.DirectoryId);
                var dir = archive.Header.Part2.GetEntry(pt1Entry);
                var dirName = archive.Header.Part3.GetFilename(dir.FilenameOffset);
                _filePathInArchive = Path.Combine(dirName, _filePathInArchive);

                currentItem = dir;
            }

            /* Hash the file path in archive */
            _filePathInArchiveHash = FilePathHelper.HashStringMD5(_filePathInArchive);

            //
            // Get the compressed file chunk offsets
            //
            if (_pt1Entry.OffsetIntoPt4Raw == 0xFFFFFFFF)
            {
                // TODO : Not sure exactly what this value means yet
                // For now, just set compressed size as extracted size with not compressed chunk sizes
                _compressedSize = ExtractedSize;
            }
            else
            {
                var numChunks = (UInt32)Math.Ceiling(ExtractedSize / (double)CHUNK_SIZE);
                if (numChunks > 0)
                {
                    var firstChunkSizeEntry = _archive.Header.Part4.Offset + _pt1Entry.OffsetIntoPt4;
                    UInt32Type chunkOffset;

                    for (int i = 0; i < numChunks; i++)
                    {
                        chunkOffset = new UInt32Type(i * 4);
                        chunkOffset.Read(file, firstChunkSizeEntry);
                        _chunkSizes.Add(chunkOffset.Value);
                    }

                    _compressedSize = _chunkSizes.Last();
                }
            }
        }

        /// <summary>
        /// The archive this items belongs to.
        /// </summary>
        public NefsArchive Archive
        {
            get { return _archive; }
        }

        /// <summary>
        /// List of cumulative chunk sizes. 
        /// - First entry is size of first chunk.
        /// - Second entry is size of first + second chunk.
        /// - Last entry is size of all chunks together.
        /// To get the size of a specific chunk, simply subtract the
        /// previous chunk size entry.
        /// </summary>
        public List<UInt32> ChunkSizes
        {
            get { return _chunkSizes; }
        }

        //[TypeConverter(typeof(HexStringTypeConverter))]
        /// <summary>
        /// The total size of compressed data in the archive for this item.
        /// </summary>
        public UInt32 CompressedSize
        {
            get { return _compressedSize; }
        }

        /// <summary>
        /// Absolute offset to the item's compressed data in the archive.
        /// </summary>
        public UInt64 DataOffset
        {
            get { return _dataOffset; }
            internal set { _dataOffset = value; }
        }

        /// <summary>
        /// The id of the directory this item is in.
        /// </summary>
        public UInt32 DirectoryId
        {
            get { return _pt2Entry.DirectoryId; }
        }

        /// <summary>
        /// Size of extracted file.
        /// </summary>
        public UInt32 ExtractedSize
        {
            get { return _extractedSize; }
        }

        /// <summary>
        /// The item's filename/directory name.
        /// </summary>
        public string Filename
        {
            get { return _fileName; }
        }

        /// <summary>
        /// Hash of the item's filename/directory name.
        /// </summary>
        public string FileNameHash
        {
            get { return _fileNameHash; }
        }

        /// <summary>
        /// Gets path to the item within the archive.
        /// Example: cars/models/fr2/interior/fr2.xml
        /// </summary>
        public string FilePathInArchive
        {
            get { return _filePathInArchive; }
        }

        /// <summary>
        /// Gets path to the item within the archive.
        /// Example: cars/models/fr2/interior/fr2.xml
        /// </summary>
        public string FilePathInArchiveHash
        {
            get { return _filePathInArchiveHash; }
        }

        /// <summary>
        /// The id of this item.
        /// </summary>
        public UInt32 Id
        {
            get { return _id; }
        }

        /// <summary>
        /// The number of chunks this item was broken into.
        /// </summary>
        public UInt32 NumberOfChunks
        {
            get { return (uint)_chunkSizes.Count; }
        }

        /// <summary>
        /// Path to single compressed file that is to be injected
        /// into the archive for this item.
        /// </summary>
        public string FileToInject
        {
            get { return _fileToInject; }
        }

        /// <summary>
        /// Scaled offset into header part 2 for this item's part 2 entry.
        /// </summary>
        public UInt32 OffsetIntoPt2
        {
            get { return _offsetIntoPt2Raw * 20; }
        }

        /// <summary>
        /// Raw (stored) offset into header part 2 for this item's part 2 entry.
        /// </summary>
        public UInt32 OffsetIntoPt2Raw
        {
            get { return _offsetIntoPt2Raw; }
            internal set { _offsetIntoPt2Raw = value; }
        }

        /// <summary>
        /// Scaled offset into header part 4 for this item's part 4 entry.
        /// </summary>
        public UInt32 OffsetIntoPt4
        {
            get { return _offsetIntoPt4Raw * 4; }
        }

        /// <summary>
        /// Raw (stored) offset into header part 4 for this item's part 4 entry.
        /// </summary>
        public UInt32 OffsetIntoPt4Raw
        {
            get { return _offsetIntoPt4Raw; }
            internal set { _offsetIntoPt4Raw = value; }
        }

        /// <summary>
        /// The header part 1 entry for this item.
        /// </summary>
        public NefsHeaderPt1Entry Part1Entry
        {
            get { return _pt1Entry; }
        }

        /// <summary>
        /// The header part 2 entry for this item.
        /// </summary>
        public NefsHeaderPt2Entry Part2Entry
        {
            get { return _pt2Entry; }
        }

        /// <summary>
        /// The header part 5 entry for this item.
        /// </summary>
        public NefsHeaderPt5Entry Part5Entry
        {
            get { return _pt5Entry; }
        }

        /// <summary>
        /// The header part 6 entry for this item.
        /// </summary>
        public NefsHeaderPt6Entry Part6Entry
        {
            get { return _pt6Entry; }
        }

        /// <summary>
        /// This item it pending data injection using the FileToInject.
        /// </summary>
        public bool PendingInjection
        {
            get { return _pendingInjection; }
        }

        /// <summary>
        /// The type of item this is.
        /// </summary>
        public NefsItemType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Extracts this item from the archive.
        /// </summary>
        /// <param name="outputFileName">Full output filename (including directory).</param>
        /// <param name="p">Progress info.</param>
        public void Extract(string outputFileName, NefsProgressInfo p)
        {
            var outputDir = Path.GetDirectoryName(outputFileName);
            var outputFile = Path.GetFileName(outputFileName);
            Extract(outputDir, outputFile, p);
        }

        /// <summary>
        /// Extracts this item from the archive.
        /// </summary>
        /// <param name="outputDir">The directory where to put the extracted file.</param>
        /// <param name="outputFileName">The name of the extracted file (without the directory).</param>
        /// <param name="p">Progress info.</param>
        public void Extract(string outputDir, string outputFileName, NefsProgressInfo p)
        {
            /* Create output dir if doesn't exist */
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            /*
             * Handle directories
             */
            if (Type == NefsItemType.Directory)
            {
                p.BeginTask(1.0f, String.Format("Extracting {0}...", FilePathInArchive));

                /* Get a list of items in this directory */
                var children = GetChildren();

                var i = 0;
                var numChildren = children.Count();

                foreach (var child in children)
                {
                    p.BeginSubTask(1.0f / numChildren, String.Format("Extracting child {0}/{1}...", i + 1, numChildren));

                    /* Since this is a directory, Filename will be the directory name */
                    var dir = Path.Combine(outputDir, Filename);

                    try
                    {
                        child.Extract(dir, child.Filename, p);
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("Error extracting item {0}.", child.FilePathInArchive));
                    }

                    i++;
                    p.EndTask();
                }

                p.EndTask();
                return;
            }

            /*
             * Item is file, extract it
             */
            using (var inFile = new FileStream(Archive.FilePath, FileMode.Open))
            using (var outFile = new FileStream(Path.Combine(outputDir, outputFileName), FileMode.Create))
            {
                p.BeginTask(1.0f, String.Format("Extracting {0}...", FilePathInArchive));

                /* Seek to the compressed data in the archive */
                inFile.Seek((long)this.DataOffset, SeekOrigin.Begin);

                var numChunks = ChunkSizes.Count;

                /* Some files may not be compressed in the archive, check that */
                if ((numChunks == 0 && ExtractedSize > 0)
                 || (ExtractedSize == CompressedSize))
                {
                    /* Just copy the data directly from acrhive without decompression */
                    var data = new byte[ExtractedSize];
                    inFile.Read(data, 0, (int)ExtractedSize);
                    outFile.Write(data, 0, (int)ExtractedSize);

                    return;
                }

                /* For each compressed chunk, decompress it and write it to the output file */
                for (int i = 0; i < numChunks; i++)
                {
                    p.BeginSubTask(1.0f / numChunks, String.Format("Extracting chunk {0}/{1}...", i + 1, numChunks));

                    /* Get chunk size */
                    var chunkSize = ChunkSizes[i];

                    /* Remember that values in the ChunkSize list are cumulative, so to get the 
                     * actual chunk size we need to subtract the previous ChunkSize entry */
                    if (i > 0)
                    {
                        chunkSize = chunkSize - ChunkSizes[i - 1];
                    }

                    /* Read in the comrpessed chunk */
                    var chunk = new byte[chunkSize];
                    inFile.Read(chunk, 0, (int)chunkSize);

                    /* Copy the comrpessed chunk to a memory stream and decompress (inflate) it */
                    using (var ms = new MemoryStream(chunk))
                    using (var inflater = new DeflateStream(ms, CompressionMode.Decompress))
                    {
                        /* Decompress the chunk and write it to the output file */
                        inflater.CopyTo(outFile);
                    }

                    p.EndTask();
                }

                p.EndTask();
            }
        }

        /// <summary>
        /// Gets children of this item.
        /// </summary>
        public IEnumerable<NefsItem> GetChildren()
        {
            var children = from child in Archive.Items
                           where child.DirectoryId == Id && child.Id != Id
                           select child;

            return children;
        }

        /// <summary>
        /// Compresses the specified input file and updates the archive
        /// header entries. The item is marked as pending injection
        /// so the comrpessed data is injected when the archive is saved.
        /// </summary>
        /// <param name="inputFilePath">Path to the uncompressed file to compress and inject.</param>
        public void Inject(string inputFilePath, NefsProgressInfo p)
        {
            float taskWeightPrep = 0.05f;
            float taskWeightCompress = 0.85f;
            float taskWeightUpdateMetadata = 0.05f;
            float taskWeightCleanup = 0.05f;

            p.BeginTask(taskWeightPrep, "Preparing file injection...");

            /* Prepare the temporary working directory */
            if (!Directory.Exists(FilePathHelper.TempDirectory))
            {
                Directory.CreateDirectory(FilePathHelper.TempDirectory);
            }

            /*
             * Create a temporary directory to compress input file in
             */
             
            /* Temp working directory */
            var workDir = Path.Combine(FilePathHelper.TempDirectory, _archive.FilePathHash, FilePathInArchiveHash);

            /* Delete the working directory if exists and recreate it */
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, true);
            }

            Directory.CreateDirectory(workDir);

            p.EndTask();

            /*
             * SPLIT INPUT FILE INTO CHUNKS AND COMPRESS THEM
             */
            p.BeginTask(taskWeightCompress, "Compressing file...");

            int compressedSizeDiff = 0;
            int currentChunk = 0;
            var destFilePath = Path.Combine(workDir, "inject.dat");
            int numChunks = 0;
            int numChunksDiff = 0;
            int oldNumChunks = ChunkSizes.Count();
            bool compressChunks = true;
            
            /* is this file a non-compressed file? */
            if (CompressedSize == ExtractedSize)
            {
                /* this file should not be compressed */
                compressChunks = false;
            }

            /* Open the input file */
            using (var inputFile = new FileStream(inputFilePath, FileMode.Open))
            {
                inputFile.Seek(0, SeekOrigin.Begin);

                /* Determine how many chunks to split file into */
                numChunks = (int)Math.Ceiling(inputFile.Length / (double)CHUNK_SIZE);
                _extractedSize = (UInt32)inputFile.Length;

                /* Clear out chunk sizes list so we can rebuild it */
                ChunkSizes.Clear();

                int lastBytesRead = 0;
                int totalBytesRead = 0;
                int lastChunkSize = 0;
                int totalChunkSize = 0;

                using (var outputFile = new FileStream(destFilePath, FileMode.Create))
                {
                    do
                    {
                        p.BeginTask(1.0f / (float)numChunks, String.Format("Compressing chunk {0}/{1}...", currentChunk + 1, numChunks));

                        if (compressChunks)
                        {
                            /* Compress this chunk and write it to the output file */
                            lastBytesRead = DeflateHelper.DeflateToFile(inputFile, CHUNK_SIZE, outputFile, out lastChunkSize);
                        }
                        else
                        {
                            /* copy extracted chunk to output file */
                            var bytes = new byte[CHUNK_SIZE];
                            lastBytesRead = inputFile.Read(bytes, 0, CHUNK_SIZE);
                            lastChunkSize = lastBytesRead;
                            outputFile.Write(bytes, 0, lastBytesRead);
                        }

                        totalBytesRead += lastBytesRead;
                        totalChunkSize += lastChunkSize;
                        currentChunk++;

                        /* Record the total compressed size after this chunk */
                        ChunkSizes.Add((uint)totalChunkSize);

                        p.EndTask();

                    } while (lastBytesRead == CHUNK_SIZE);

                    /* Get difference in number of chunks compared to item we replaced */
                    numChunksDiff = numChunks - oldNumChunks;

                    /* Get the difference in compressed size */
                    compressedSizeDiff = (int)outputFile.Length - (int)_compressedSize;

                    /* Update new compressed size */
                    _compressedSize = (uint)outputFile.Length;

                    /* Quick sanity check */
                    if (_compressedSize != totalChunkSize)
                    {
                        log.Error("Compressed file size different than what was expected.");
                    }
                }
            }
            
            /* Quick sanity check */
            if (currentChunk != numChunks)
            {
                log.Error("Did not create the expected number of chunks.");
            }

            p.EndTask();

            /*
             * UPDATE METADATA FOR ITEMS AFTER THIS ONE
             */
            p.BeginTask(taskWeightUpdateMetadata, "Updating archive metadata...");

            /* Update data offsets for each item AFTER this one */
            for (int i = 0; i < Archive.Items.Count; i++)
            {
                var item = Archive.Items[i];
                
                /* Directories don't have data offsets, skip them */
                if (item.Type == NefsItemType.Directory)
                {
                    continue;
                }

                /* Update an item's data offset if it was after the item we are updating now */
                if (item.DataOffset > this.DataOffset)
                {
                    /* Update the data offset */
                    UInt64 prevOffset = item.DataOffset;
                    UInt64 newOffset = (ulong)((long)prevOffset + (long)compressedSizeDiff);

                    if (newOffset < 0)
                    {
                        throw new Exception(String.Format(
                            "New data offset less than zero. [file={0}]",
                            item.Filename));
                    }

                    item.DataOffset = newOffset;
                }
                                
                /* Skip the weird 0xFFFFFFFF offsets into pt 4 */
                if (item.OffsetIntoPt4Raw == 0xFFFFFFFF)
                {
                    continue;
                }

                /* Update an item's part 4 offset if it was after the item we are updating now */
                if (item.OffsetIntoPt4Raw > this.OffsetIntoPt4Raw)
                {
                    /* Update the header part 4 offset */
                    int prevOffsetIntoPt4Raw = (int)item.OffsetIntoPt4Raw;
                    int newOffsetIntoPt4Raw = prevOffsetIntoPt4Raw + numChunksDiff;

                    if (newOffsetIntoPt4Raw < 0)
                    {
                        throw new Exception(String.Format(
                            "New offset into part 4 less than zero. [file={0}]",
                            item.Filename));
                    }

                    item.OffsetIntoPt4Raw = (UInt32)newOffsetIntoPt4Raw;
                }
            }
            
            p.EndTask();

            /*
             * CLEANUP
             */
            p.BeginTask(taskWeightCleanup, "Cleaning up...");

            /* Whenever the archive is saved, this data will be injected */
            _pendingInjection = true;
            _fileToInject = destFilePath;
            _archive.Modified = true;

            p.EndTask();
        }

        /// <summary>
        /// Writes the item's compressed data to the file stream. 
        /// </summary>
        /// <param name="destFile">The file stream to write to.</param>
        /// <param name="p">Progress info.</param>
        public void Write(FileStream destFile, NefsProgressInfo p)
        {
            /*
             * NOTES:
             * - This function just writes the file's compressed data.
             * - If this file is not pending injection/replacement, then the data from the current archive is used.
             * - All header entries for this item should already have been updated.
             * - Header entries are written by their respective header objects.
             */

            /* Check if there is data to write */
            if (DataOffset == 0 || CompressedSize == 0)
            {
                return;
            }

            if (PendingInjection)
            {
                /*
                 * File is pending injection
                 */

                /* Open the file to inject - this should be one file that
                 * was compressed using NefsLib and is in the application's 
                 * temp directory. */
                using (var fileToInject = new FileStream(FileToInject, FileMode.Open))
                {
                    if (CompressedSize != fileToInject.Length)
                    {
                        throw new Exception(String.Format("Compressed file size different than expected [{0}].", FileToInject));
                    }

                    /* Read in the file to a temp buffer */
                    var temp = new byte[fileToInject.Length];
                    fileToInject.Seek(0, SeekOrigin.Begin);
                    fileToInject.Read(temp, 0, (int)fileToInject.Length);

                    /* Write the new compressed data to the destination */
                    destFile.Seek((long)DataOffset, SeekOrigin.Begin);
                    destFile.Write(temp, 0, (int)CompressedSize);
                }
            }
            else
            {
                /*
                 * Copy the original compressed data to the destination.
                 */

                /* Open the current archive */
                using (var sourceFile = new FileStream(Archive.FilePath, FileMode.Open))
                {
                    /* Read the source file from the original data offset */
                    sourceFile.Seek((long)_pt1Entry.OffsetToData, SeekOrigin.Begin);

                    /* Write to the dest with the updated data offset */
                    destFile.Seek((long)DataOffset, SeekOrigin.Begin);

                    /* Read from source nefs file */
                    var size = CompressedSize;
                    var temp = new byte[size];
                    sourceFile.Read(temp, 0, (int)size);

                    /* Write to the destination nefs file */
                    destFile.Write(temp, 0, (int)size);
                }
            }
        }

        /// <summary>
        /// Clears the item's pending injection flag.
        /// </summary>
        internal void ClearPendingInjection()
        {
            _pendingInjection = false;
        }
    }
}
