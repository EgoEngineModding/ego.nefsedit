// See LICENSE.txt for license information.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VictorBush.Ego.NefsLib.Tests")]

namespace VictorBush.Ego.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;

    // TODO : Lots of duplicate code in here.

    /// <summary>
    /// Reads NeFS archives.
    /// </summary>
    public partial class NefsReader : INefsReader
    {
        /// <summary>
        /// Reads a version 1.6 header from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header from the beginning of the stream.</param>
        /// <param name="intro">The pre-parsed header intro.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header.</returns>
        internal async Task<Nefs16Header> Read16HeaderAsync(Stream stream, ulong offset, NefsHeaderIntro intro, NefsProgress p)
        {
            Nefs16HeaderIntroToc toc = null;
            Nefs16HeaderPart1 part1 = null;
            Nefs16HeaderPart2 part2 = null;
            Nefs16HeaderPart3 part3 = null;
            Nefs16HeaderPart4 part4 = null;
            Nefs16HeaderPart5 part5 = null;
            Nefs16HeaderPart6 part6 = null;
            Nefs16HeaderPart7 part7 = null;
            Nefs16HeaderPart8 part8 = null;

            // Calc weight of each task (8 parts + table of contents)
            var weight = 1.0f / 10.0f;

            using (p.BeginTask(weight, "Reading header intro table of contents"))
            {
                toc = await this.Read16HeaderIntroTocAsync(stream, NefsHeaderIntroToc.Offset, p);
            }

            using (p.BeginTask(weight, "Reading header part 1"))
            {
                part1 = await this.Read16HeaderPart1Async(stream, toc.OffsetToPart1, toc.Part1Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 2"))
            {
                part2 = await this.Read16HeaderPart2Async(stream, toc.OffsetToPart2, toc.Part2Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 3"))
            {
                part3 = await this.Read16HeaderPart3Async(stream, toc.OffsetToPart3, toc.Part3Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 4"))
            {
                part4 = await this.Read16HeaderPart4Async(stream, toc.OffsetToPart4, toc.Part4Size, part1, part2, p);
            }

            using (p.BeginTask(weight, "Reading header part 5"))
            {
                part5 = await this.Read16HeaderPart5Async(stream, toc.OffsetToPart5, toc.Part5Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 6"))
            {
                if (toc.OffsetToPart6 == 0)
                {
                    // game.dat files don't have part 6
                    Log.LogDebug("Archive does not have header part 6.");
                    part6 = new Nefs16HeaderPart6(new List<Nefs16HeaderPart6Entry>());
                }
                else
                {
                    part6 = await this.Read16HeaderPart6Async(stream, toc.OffsetToPart6, toc.Part6Size, part2, p);
                }
            }

            using (p.BeginTask(weight, "Reading header part 7"))
            {
                if (toc.OffsetToPart6 == 0)
                {
                    // game.dat files don't have part 7. Still checking if part 6 offset is 0. For
                    // some reason, the part 7 offset still has a value, but doesn't appear to be a
                    // correct one, so skipping part 7 as well
                    Log.LogDebug("Archive does not have header part 7.");
                    part7 = new Nefs16HeaderPart7(new List<Nefs16HeaderPart7Entry>());
                }
                else
                {
                    part7 = await this.Read16HeaderPart7Async(stream, toc.OffsetToPart7, toc.Part7Size, p);
                }
            }

            using (p.BeginTask(weight, "Reading header part 8"))
            {
                var part8Size = intro.HeaderSize - toc.OffsetToPart8;
                part8 = await this.Read16HeaderPart8Async(stream, toc.OffsetToPart8, part8Size, p);
            }

            // Validate header hash
            if (!this.ValidateHash(stream, offset, intro))
            {
                Log.LogWarning("Header hash does not match expected value.");
            }

            // The header stream must be disposed
            stream.Dispose();

            return new Nefs16Header(intro, toc, part1, part2, part3, part4, part5, part6, part7, part8);
        }

        /// <summary>
        /// Reads the header intro table of contents from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">
        /// The offset to the header intro table of contents from the beginning of the stream.
        /// </param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header intro offets data.</returns>
        internal async Task<Nefs16HeaderIntroToc> Read16HeaderIntroTocAsync(Stream stream, uint offset, NefsProgress p)
        {
            var toc = new Nefs16HeaderIntroToc();
            await FileData.ReadDataAsync(stream, offset, toc, p);
            return toc;
        }

        /// <summary>
        /// Reads header part 1 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs16HeaderPart1> Read16HeaderPart1Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var entries = new List<Nefs16HeaderPart1Entry>();
            var ids = new HashSet<NefsItemId>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "1"))
            {
                return new Nefs16HeaderPart1(entries);
            }

            // Get entries in part 1
            var numEntries = size / Nefs16HeaderPart1Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    var entry = new Nefs16HeaderPart1Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, p);
                    entryOffset += Nefs16HeaderPart1Entry.Size;

                    // Check for duplicate item ids
                    var id = new NefsItemId(entry.Id.Value);
                    if (ids.Contains(id))
                    {
                        Log.LogError($"Found duplicate item id in part 1: {id.Value}");
                        continue;
                    }

                    ids.Add(id);
                    entries.Add(entry);
                }
            }

            return new Nefs16HeaderPart1(entries);
        }

        /// <summary>
        /// Reads header part 2 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs16HeaderPart2> Read16HeaderPart2Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var entries = new List<Nefs16HeaderPart2Entry>();
            var ids = new HashSet<NefsItemId>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "2"))
            {
                return new Nefs16HeaderPart2(entries);
            }

            // Get entries in part 2
            var numEntries = size / Nefs16HeaderPart2Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    var entry = new Nefs16HeaderPart2Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, p);
                    entryOffset += Nefs16HeaderPart2Entry.Size;

                    // Check for duplicate item ids
                    var id = new NefsItemId(entry.Id.Value);
                    if (ids.Contains(id))
                    {
                        Log.LogError($"Found duplicate item id in part 2: {id.Value}");
                        continue;
                    }

                    ids.Add(id);
                    entries.Add(entry);
                }
            }

            return new Nefs16HeaderPart2(entries);
        }

        /// <summary>
        /// Reads header part 3 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs16HeaderPart3> Read16HeaderPart3Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var entries = new List<string>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "3"))
            {
                return new Nefs16HeaderPart3(entries);
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

                    if (nullOffset == size)
                    {
                        // No null terminator found, assume end of part 3. There can be a few
                        // garbage bytes at the end of this part.
                        break;
                    }

                    // Get the string
                    var str = Encoding.ASCII.GetString(bytes, nextOffset, nullOffset - nextOffset);

                    // Record entry
                    entries.Add(str);

                    // Find next string
                    nextOffset = nullOffset + 1;
                }
            }

            return new Nefs16HeaderPart3(entries);
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
        internal async Task<Nefs16HeaderPart4> Read16HeaderPart4Async(
            Stream stream,
            uint offset,
            uint size,
            Nefs16HeaderPart1 part1,
            Nefs16HeaderPart2 part2,
            NefsProgress p)
        {
            var entries = new Dictionary<uint, Nefs16HeaderPart4Entry>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "4"))
            {
                return new Nefs16HeaderPart4(entries);
            }

            // Get the chunk sizes for each item in the archive
            var numItems = part1.EntriesById.Count;
            for (var i = 0; i < numItems; ++i)
            {
                using (p.BeginTask(1.0f / numItems))
                {
                    // Part 1 entry
                    var p1 = part1.EntriesByIndex[i];

                    // Part 2 entry
                    if (!part2.EntriesById.ContainsKey(p1.Id))
                    {
                        Log.LogError($"Failed to find part 2 entry for item {p1.Id} when reading part 4.");
                        continue;
                    }

                    var p2 = part2.EntriesById[p1.Id];

                    // Create part 4 entry
                    var entry = new Nefs16HeaderPart4Entry(p1.Id);

                    // Check if item has part 4 entry
                    if (p2.Data0x0c_ExtractedSize.Value == 0)
                    {
                        // Item is probably a directory
                        continue;
                    }

                    // Get number of chunks
                    var numChunks = (int)Math.Ceiling(p2.Data0x0c_ExtractedSize.Value / (double)Nefs16HeaderIntroToc.ChunkSize);
                    if (numChunks == 0)
                    {
                        Log.LogError($"Item {p1.Id} contains no compressed chunks but was expected to.");
                        continue;
                    }

                    // Seek stream to start of chunk sizes for this item
                    var itemOffset = offset + p1.OffsetIntoPart4;
                    if ((long)itemOffset + Nefs16HeaderPart4Chunk.Size > stream.Length)
                    {
                        Log.LogError($"Item {p1.Id} has part 4 entry that is outside the bounds of header part 4.");
                        continue;
                    }

                    // Seek stream
                    stream.Seek((long)itemOffset, SeekOrigin.Begin);
                    var chunkOffset = itemOffset;

                    // Process the chunk sizes
                    for (var chunkIdx = 0; chunkIdx < numChunks; ++chunkIdx)
                    {
                        var chunk = new Nefs16HeaderPart4Chunk();
                        await FileData.ReadDataAsync(stream, chunkOffset, chunk, p);
                        entry.Chunks.Add(chunk);

                        chunkOffset += Nefs16HeaderPart4Chunk.Size;
                    }

                    // Record entry
                    entries.Add(p1.IndexIntoPart4, entry);
                }
            }

            // Return part 4
            return new Nefs16HeaderPart4(entries);
        }

        /// <summary>
        /// Reads header part 5 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs16HeaderPart5> Read16HeaderPart5Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var part5 = new Nefs16HeaderPart5();

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
        /// <param name="part2">
        /// Header part 2. This is used to lookup item ids since part 6 metadata does not store item ids.
        /// </param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs16HeaderPart6> Read16HeaderPart6Async(Stream stream, uint offset, uint size, Nefs16HeaderPart2 part2, NefsProgress p)
        {
            var entries = new List<Nefs16HeaderPart6Entry>();
            var ids = new HashSet<NefsItemId>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "6"))
            {
                return new Nefs16HeaderPart6(entries);
            }

            // Get entries in part 6
            var numEntries = size / Nefs16HeaderPart6Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    // Make sure there is a corresponding index in part 2
                    if (i >= part2.EntriesByIndex.Count)
                    {
                        Log.LogError($"Could not find matching item entry for part 6 index {i} in part 2.");
                        continue;
                    }

                    // Check for duplicate item ids
                    var id = new NefsItemId(part2.EntriesByIndex[i].Id.Value);
                    if (ids.Contains(id))
                    {
                        Log.LogError($"Found duplicate item id in part 6: {id.Value}");
                        continue;
                    }

                    var entry = new Nefs16HeaderPart6Entry(id);
                    await FileData.ReadDataAsync(stream, entryOffset, entry, p);
                    entryOffset += Nefs16HeaderPart6Entry.Size;

                    ids.Add(id);
                    entries.Add(entry);
                }
            }

            return new Nefs16HeaderPart6(entries);
        }

        /// <summary>
        /// Reads header part 7 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs16HeaderPart7> Read16HeaderPart7Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var entries = new List<Nefs16HeaderPart7Entry>();
            var ids = new HashSet<NefsItemId>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "7"))
            {
                return new Nefs16HeaderPart7(entries);
            }

            // Get entries in part 7
            var numEntries = size / Nefs16HeaderPart7Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    var entry = new Nefs16HeaderPart7Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, p);

                    // Check for duplicate item ids
                    var id = new NefsItemId(entry.Id.Value);
                    if (ids.Contains(id))
                    {
                        Log.LogWarning($"Found duplicate item id in part 7: {id.Value}");
                    }

                    ids.Add(id);
                    entries.Add(entry);
                    entryOffset += Nefs16HeaderPart7Entry.Size;
                }
            }

            return new Nefs16HeaderPart7(entries);
        }

        /// <summary>
        /// Reads header part 8 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs16HeaderPart8> Read16HeaderPart8Async(Stream stream, uint offset, uint size, NefsProgress p)
        {
            var part8 = new Nefs16HeaderPart8(size);

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "8"))
            {
                return part8;
            }

            await stream.ReadAsync(part8.AllTheData.Value, 0, (int)size, p.CancellationToken);
            return part8;
        }
    }
}
