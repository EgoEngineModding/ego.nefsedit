// See LICENSE.txt for license information.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VictorBush.Ego.NefsLib.Tests")]

namespace VictorBush.Ego.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// Reads NeFS archives.
    /// </summary>
    public class NefsReader : INefsReader
    {
        private static readonly ILogger Log = NefsLib.LogFactory.CreateLogger<NefsReader>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsReader"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system used by the factory.</param>
        public NefsReader(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        /// The file system used by the factory.
        /// </summary>
        private IFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public async Task<NefsArchive> ReadArchiveAsync(string filePath, NefsProgress p)
        {
            // Validate path
            if (!this.FileSystem.File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}.");
            }

            // Read the header
            NefsHeader header = null;
            using (var stream = this.FileSystem.File.OpenRead(filePath))
            {
                header = await this.ReadHeaderAsync(stream, NefsHeader.IntroOffset, p);
            }

            // Create items from header
            var items = this.CreateItems(filePath, header, p);

            // Create the archive
            return new NefsArchive(header, items);
        }

        /// <summary>
        /// Reads the header from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header from the beginning of the stream.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header.</returns>
        internal async Task<NefsHeader> ReadHeaderAsync(Stream stream, uint offset, NefsProgress p)
        {
            NefsHeaderIntro intro = null;
            NefsHeaderPart1 part1 = null;
            NefsHeaderPart2 part2 = null;
            NefsHeaderPart3 part3 = null;
            NefsHeaderPart4 part4 = null;
            NefsHeaderPart5 part5 = null;
            NefsHeaderPart6 part6 = null;
            NefsHeaderPart7 part7 = null;
            NefsHeaderPart8 part8 = null;

            // Calc weight of each task (9 parts of header to load)
            var weight = 1.0f / 9.0f;

            using (p.BeginTask(weight, "Reading header intro"))
            {
                intro = await this.ReadHeaderIntroAsync(stream, offset, p);
            }

            using (p.BeginTask(weight, "Reading header part 1"))
            {
                part1 = await this.ReadHeaderPart1Async(stream, intro.OffsetToPart1.Value, intro.Part1Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 2"))
            {
                part2 = await this.ReadHeaderPart2Async(stream, intro.OffsetToPart2.Value, intro.Part2Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 3"))
            {
                part3 = await this.ReadHeaderPart3Async(stream, intro.OffsetToPart3.Value, intro.Part3Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 4"))
            {
                part4 = await this.ReadHeaderPart4Async(stream, intro.OffsetToPart4.Value, intro.Part4Size, part1, part2, p);
            }

            using (p.BeginTask(weight, "Reading header part 5"))
            {
                part5 = await this.ReadHeaderPart5Async(stream, intro.OffsetToPart5.Value, intro.Part5Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 6"))
            {
                part6 = await this.ReadHeaderPart6Async(stream, intro.OffsetToPart6.Value, intro.Part6Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 7"))
            {
                part7 = await this.ReadHeaderPart7Async(stream, intro.OffsetToPart7.Value, intro.Part7Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 8"))
            {
                // TODO : Get correct part 8 size
                part8 = await this.ReadHeaderPart8Async(stream, intro.OffsetToPart8.Value, 0, p);
            }

            return new NefsHeader(intro, part1, part2, part3, part4, part5, part6, part7, part8);
        }

        /// <summary>
        /// Reads the header intro from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header intro from the beginning of the stream.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header intro.</returns>
        internal async Task<NefsHeaderIntro> ReadHeaderIntroAsync(Stream stream, uint offset, NefsProgress p)
        {
            // Read magic number (first four bytes)
            var magicNum = new UInt32Type(0);
            await magicNum.ReadAsync(stream, offset, p);

            // If magic number is incorrect, see if the file is encrpyted
            if (magicNum.Value != NefsHeaderIntro.NefsMagicNumber)
            {
                // RSA 1024 public key. TODO : Don't hard-code key?
                //byte[] pubk = { 0xCF, 0x19, 0x63, 0x94, 0x1E, 0x0F, 0x42, 0x16, 0x35, 0xDE, 0x51, 0xD0, 0xB3, 0x3A, 0xB7, 0x67, 0xC7, 0x1C, 0x8D, 0x3B, 0x27, 0x49, 0x40, 0x9E, 0x58, 0x43, 0xDD, 0x6D, 0xD9, 0xAA, 0xF5, 0x1B, 0x94, 0x94, 0xC4, 0x30, 0x49, 0xBA, 0xE7, 0x72, 0x3D, 0xFA, 0xDF, 0x80, 0x17, 0x55, 0xF3, 0xAB, 0xF8, 0x97, 0x42, 0xE6, 0xB2, 0xDF, 0x11, 0xE4, 0x93, 0x0E, 0x92, 0x1D, 0xC5, 0x4E, 0x0F, 0x87, 0xCD, 0x46, 0x83, 0x06, 0x6B, 0x97, 0xA7, 0x00, 0x42, 0x35, 0xB0, 0x33, 0xEA, 0xEF, 0x68, 0x54, 0xA0, 0xF9, 0x03, 0x41, 0xF7, 0x5C, 0xFF, 0xC3, 0x75, 0xE1, 0x1B, 0x00, 0x73, 0x5A, 0x7A, 0x81, 0x68, 0xAF, 0xB4, 0x9F, 0x86, 0x3C, 0xD6, 0x09, 0x3A, 0xC0, 0x94, 0x6F, 0x18, 0xE2, 0x03, 0x38, 0x14, 0xF7, 0xC5, 0x13, 0x91, 0x4E, 0xD0, 0x4F, 0xAC, 0x46, 0x6C, 0x70, 0x27, 0xED, 0x69, 0x99 };
                // TODO
            }

            var intro = new NefsHeaderIntro();
            await FileData.ReadDataAsync(stream, offset, intro, p);
            return intro;
        }

        /// <summary>
        /// Reads header part 1 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<NefsHeaderPart1> ReadHeaderPart1Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var entries = new Dictionary<NefsItemId, NefsHeaderPart1Entry>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "1"))
            {
                return new NefsHeaderPart1(entries);
            }

            // Get entries in part 1
            var numEntries = size / NefsHeaderPart1Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    var entry = new NefsHeaderPart1Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, p);
                    entries.Add(new NefsItemId(entry.Id.Value), entry);
                    entryOffset += NefsHeaderPart1Entry.Size;
                }
            }

            return new NefsHeaderPart1(entries);
        }

        /// <summary>
        /// Reads header part 2 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<NefsHeaderPart2> ReadHeaderPart2Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var entries = new Dictionary<NefsItemId, NefsHeaderPart2Entry>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "2"))
            {
                return new NefsHeaderPart2(entries);
            }

            // Get entries in part 2
            var numEntries = size / NefsHeaderPart2Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    var entry = new NefsHeaderPart2Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, p);
                    entries.Add(new NefsItemId(entry.Id.Value), entry);
                    entryOffset += NefsHeaderPart2Entry.Size;
                }
            }

            return new NefsHeaderPart2(entries);
        }

        /// <summary>
        /// Reads header part 3 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<NefsHeaderPart3> ReadHeaderPart3Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var entries = new List<string>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "3"))
            {
                return new NefsHeaderPart3(entries);
            }

            // Read in header part 3
            var bytes = new byte[size];
            stream.Seek(offset, SeekOrigin.Begin);
            await stream.ReadAsync(bytes, 0, (int)size);

            // Process all strings in the strings table
            var nextOffset = 0;
            while (nextOffset < size)
            {
                using (p.BeginTask(nextOffset / size))
                {
                    // Find the next null terminator
                    var nullOffset = (int)size;
                    for (var i = nextOffset; i < size; ++i)
                    {
                        if (bytes[i] == 0)
                        {
                            nullOffset = i;
                            break;
                        }
                    }

                    // Get the string
                    var str = Encoding.ASCII.GetString(bytes, nextOffset, nullOffset - nextOffset);

                    // Record entry
                    entries.Add(str);

                    // Find next string
                    nextOffset = nullOffset + 1;
                }
            }

            return new NefsHeaderPart3(entries);
        }

        /// <summary>
        /// Reads header part 4 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="part1">Header part 1.</param>
        /// <param name="part2">Header part 2.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<NefsHeaderPart4> ReadHeaderPart4Async(
            Stream stream,
            uint offset,
            uint size,
            NefsHeaderPart1 part1,
            NefsHeaderPart2 part2,
            NefsProgress p)
        {
            var entries = new Dictionary<uint, NefsHeaderPart4Entry>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "4"))
            {
                return new NefsHeaderPart4(entries, 0);
            }

            // Get the chunk sizes for each item in the archive
            var numItems = part1.EntriesById.Count;
            for (var i = 0; i < numItems; ++i)
            {
                using (p.BeginTask(1.0f / numItems))
                {
                    var id = new NefsItemId((uint)i);
                    var p1 = part1.EntriesById[id];
                    var p2 = part2.EntriesById[id];
                    var entry = new NefsHeaderPart4Entry(id);

                    // Check if item has part 4 entry
                    if (p1.IndexIntoPart4.Value == 0xFFFFFFFF)
                    {
                        // Item is most likely not compressed or has no data
                        continue;
                    }

                    if (p2.ExtractedSize.Value == 0)
                    {
                        // Item is probably a directory
                        continue;
                    }

                    // Get number of chunks
                    var numChunks = (int)Math.Ceiling(p2.ExtractedSize.Value / (double)NefsArchive.ChunkSize);
                    if (numChunks == 0)
                    {
                        Log.LogError($"Item {p1.Id} contains no compressed chunks but was expected to.");
                        continue;
                    }

                    // Seek stream to start of chunk sizes for this item
                    var itemOffset = offset + p1.OffsetIntoPart4;
                    if ((long)itemOffset + NefsHeaderPart4.DataSize > stream.Length)
                    {
                        Log.LogError($"Item {p1.Id} has part 4 entry that is outside the bounds of header part 4.");
                        continue;
                    }

                    // Seek stream
                    stream.Seek((long)itemOffset, SeekOrigin.Begin);

                    // Process the chunk sizes
                    for (var chunkIdx = 0; chunkIdx < numChunks; ++chunkIdx)
                    {
                        var bytes = new byte[NefsHeaderPart4.DataSize];
                        await stream.ReadAsync(bytes, 0, NefsHeaderPart4.DataSize);
                        entry.ChunkSizes.Add(BitConverter.ToUInt32(bytes, 0));
                    }

                    // Record entry
                    entries.Add(p1.IndexIntoPart4.Value, entry);
                }
            }

            // Read the last four bytes of part 4
            var lastFourBytes = new byte[4];
            stream.Seek(offset + size - NefsHeaderPart4.DataSize, SeekOrigin.Begin);
            await stream.ReadAsync(lastFourBytes, 0, NefsHeaderPart4.DataSize);
            var lastFour = BitConverter.ToUInt32(lastFourBytes, 0);

            // Return part 4
            return new NefsHeaderPart4(entries, lastFour);
        }

        /// <summary>
        /// Reads header part 5 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<NefsHeaderPart5> ReadHeaderPart5Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var part5 = new NefsHeaderPart5();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "5"))
            {
                return part5;
            }

            // Read part 5 data
            await FileData.ReadDataAsync(stream, offset, part5, p);
            return part5;
        }

        /// <summary>
        /// Reads header part 6 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<NefsHeaderPart6> ReadHeaderPart6Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var entries = new List<NefsHeaderPart6Entry>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "6"))
            {
                return new NefsHeaderPart6(entries);
            }

            // Get entries in part 6
            var numEntries = size / NefsHeaderPart6Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    var entry = new NefsHeaderPart6Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, p);
                    entries.Add(entry);
                    entryOffset += NefsHeaderPart6Entry.Size;
                }
            }

            return new NefsHeaderPart6(entries);
        }

        /// <summary>
        /// Reads header part 7 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<NefsHeaderPart7> ReadHeaderPart7Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var entries = new List<NefsHeaderPart7Entry>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "7"))
            {
                return new NefsHeaderPart7(entries);
            }

            // Get entries in part 7
            var numEntries = size / NefsHeaderPart7Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    var entry = new NefsHeaderPart7Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, p);
                    entries.Add(entry);
                    entryOffset += NefsHeaderPart7Entry.Size;
                }
            }

            return new NefsHeaderPart7(entries);
        }

        /// <summary>
        /// Reads header part 8 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<NefsHeaderPart8> ReadHeaderPart8Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "8"))
            {
                return new NefsHeaderPart8();
            }

            // TODO : Load part 8
            return new NefsHeaderPart8();
        }

        private NefsItemList CreateItems(string dataFilePath, NefsHeader h, NefsProgress p)
        {
            var items = new NefsItemList(dataFilePath);
            var numItems = h.Intro.Part1Size / NefsHeaderPart1Entry.Size;

            for (var i = 0; i < numItems; ++i)
            {
                // Create the item
                var id = new NefsItemId((uint)i);
                var item = NefsItem.CreateFromHeader(id, h, items);
                items.Add(item);
            }

            return items;
        }

        private bool ValidateHeaderPartStream(Stream stream, uint offset, uint size, string part)
        {
            if (size == 0)
            {
                Log.LogWarning($"Header part {part} has a size of 0.");
                return false;
            }

            if (offset >= stream.Length)
            {
                Log.LogError($"Header part {part} has an offset outside the bounds of the input stream.");
                return false;
            }

            if (offset + size > stream.Length)
            {
                Log.LogError($"Header part {part} has size outside the bounds of the input stream.");
                return false;
            }

            return true;
        }
    }
}
