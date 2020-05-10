// See LICENSE.txt for license information.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VictorBush.Ego.NefsLib.Tests")]

namespace VictorBush.Ego.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Numerics;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Progress;
    using VictorBush.Ego.NefsLib.Utility;

    /// <summary>
    /// Reads NeFS archives.
    /// </summary>
    public partial class NefsReader : INefsReader
    {
        /// <summary>
        /// The default RSA exponent for encrypted headers.
        /// </summary>
        public static readonly byte[] DefaultRsaExponent = { 01, 00, 01, 00 };

        /// <summary>
        /// The default RSA public key for encrypted headers. From DiRT Rally 2.
        /// </summary>
        public static readonly byte[] DefaultRsaPublicKey =
        {
            0xCF, 0x19, 0x63, 0x94, 0x1E, 0x0F, 0x42, 0x16, 0x35, 0xDE, 0x51, 0xD0, 0xB3, 0x3A, 0xB7, 0x67,
            0xC7, 0x1C, 0x8D, 0x3B, 0x27, 0x49, 0x40, 0x9E, 0x58, 0x43, 0xDD, 0x6D, 0xD9, 0xAA, 0xF5, 0x1B,
            0x94, 0x94, 0xC4, 0x30, 0x49, 0xBA, 0xE7, 0x72, 0x3D, 0xFA, 0xDF, 0x80, 0x17, 0x55, 0xF3, 0xAB,
            0xF8, 0x97, 0x42, 0xE6, 0xB2, 0xDF, 0x11, 0xE4, 0x93, 0x0E, 0x92, 0x1D, 0xC5, 0x4E, 0x0F, 0x87,
            0xCD, 0x46, 0x83, 0x06, 0x6B, 0x97, 0xA7, 0x00, 0x42, 0x35, 0xB0, 0x33, 0xEA, 0xEF, 0x68, 0x54,
            0xA0, 0xF9, 0x03, 0x41, 0xF7, 0x5C, 0xFF, 0xC3, 0x75, 0xE1, 0x1B, 0x00, 0x73, 0x5A, 0x7A, 0x81,
            0x68, 0xAF, 0xB4, 0x9F, 0x86, 0x3C, 0xD6, 0x09, 0x3A, 0xC0, 0x94, 0x6F, 0x18, 0xE2, 0x03, 0x38,
            0x14, 0xF7, 0xC5, 0x13, 0x91, 0x4E, 0xD0, 0x4F, 0xAC, 0x46, 0x6C, 0x70, 0x27, 0xED, 0x69, 0x99,
            00,
        };

        private static readonly ILogger Log = NefsLog.GetLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsReader"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system used by the factory.</param>
        public NefsReader(IFileSystem fileSystem)
            : this(fileSystem, null, null)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsReader"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system used by the factory.</param>
        /// <param name="rsaPublicKey">
        /// Specifies a different RSA public key to use for encrypted headers.
        /// </param>
        /// <param name="rsaExponent">Specifies a different RSA exponent to use for encrypted headers.</param>
        public NefsReader(IFileSystem fileSystem, byte[] rsaPublicKey, byte[] rsaExponent)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.RsaPublicKey = rsaPublicKey ?? DefaultRsaPublicKey;
            this.RsaExponent = rsaExponent ?? DefaultRsaExponent;
        }

        private IFileSystem FileSystem { get; }

        private byte[] RsaExponent { get; }

        private byte[] RsaPublicKey { get; }

        /// <inheritdoc/>
        public async Task<List<NefsArchiveSource>> FindHeadersAsync(string exePath, string dataFileDir, NefsProgress p)
        {
            var sources = new List<NefsArchiveSource>();
            var part6Offset = 0U;

            // Load exe into memory
            var exeBytes = this.FileSystem.File.ReadAllBytes(exePath);

            // Search for the part 6 base offset. For NeFS version 1.6 and 2.0 (maybe others?)
            // header parts 6 and 7 are stored separate from the other header parts. Not sure of a
            // good way to find this, but for now we've been lucky that its always started at the
            // beginning of the ".data" section of the exe. So we can get that offset from the PE header.
            try
            {
                if (!PeHelper.GetRawOffsetToSection(exeBytes, ".data", out var dataSectionOffset))
                {
                    Log.LogError("Failed to find part 6 offset; using 0 as offset.");
                }

                part6Offset = (uint)dataSectionOffset;
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Failed to find part 6 offset; using 0 as offset.");
            }

            // Search for headers
            var i = 0;
            while (i + 4 <= exeBytes.Length)
            {
                var offset = i;
                i += 4;

                // Check for cancel
                p.CancellationToken.ThrowIfCancellationRequested();

                // Searching for a NeFS header: Look for 4E 65 46 53 (NeFS). This is the NeFS header
                // magic number.
                if (exeBytes[offset] != 0x4E
                    || exeBytes[offset + 1] != 0x65
                    || exeBytes[offset + 2] != 0x46
                    || exeBytes[offset + 3] != 0x53)
                {
                    continue;
                }

                // Check for a known version number
                var version = BitConverter.ToUInt32(exeBytes, offset + 0x68);
                if (version != 0x20000 && version != 0x10600)
                {
                    continue;
                }

                // Try to read header intro
                try
                {
                    using (var byteStream = new MemoryStream(exeBytes))
                    {
                        var (intro, headerStream) = await this.ReadHeaderIntroAsync(byteStream, (ulong)offset, p);
                        using (headerStream)
                        {
                            // Read table of contents
                            INefsHeaderIntroToc toc;
                            if (version == (int)NefsVersion.Version200)
                            {
                                toc = await this.Read20HeaderIntroTocAsync(headerStream, Nefs20HeaderIntroToc.Offset, p);
                            }
                            else
                            {
                                toc = await this.Read16HeaderIntroTocAsync(headerStream, Nefs16HeaderIntroToc.Offset, p);
                            }

                            // Read part 5
                            var p5 = await this.ReadHeaderPart5Async(headerStream, toc.OffsetToPart5, NefsHeaderPart5.Size, p);

                            // Find file name
                            headerStream.Seek(toc.OffsetToPart3, SeekOrigin.Begin);
                            headerStream.Seek(p5.ArchiveNameStringOffset, SeekOrigin.Current);

                            // Read 256 bytes - this is overkill, probably won't have a filename
                            // that big
                            var nameBytes = new byte[256];
                            await headerStream.ReadAsync(nameBytes, 0, 256, p.CancellationToken);

                            var name = StringHelper.TryReadNullTerminatedAscii(nameBytes, 0, nameBytes.Length);
                            if (string.IsNullOrWhiteSpace(name))
                            {
                                // Failed to get name
                                Log.LogError($"Thought we found a header at {offset}, but could not read data file name.");
                                continue;
                            }

                            // Create archive source for this header
                            var dataFilePath = Path.Combine(dataFileDir, name);
                            var source = new NefsArchiveSource(exePath, (ulong)offset, part6Offset, dataFilePath);
                            sources.Add(source);

                            // Keep looking
                            offset += (int)intro.HeaderSize;
                        }
                    }
                }
                catch (Exception)
                {
                    // Failed to read header, so assume not a header
                    continue;
                }
            }

            return sources;
        }

        /// <inheritdoc/>
        public async Task<NefsArchive> ReadArchiveAsync(string filePath, NefsProgress p)
        {
            return await this.ReadArchiveAsync(filePath, Nefs20Header.IntroOffset, 0, filePath, p);
        }

        /// <inheritdoc/>
        public async Task<NefsArchive> ReadArchiveAsync(
            string headerFilePath,
            ulong headerOffset,
            ulong headerPart6Offset,
            string dataFilePath,
            NefsProgress p)
        {
            // Validate header path
            if (!this.FileSystem.File.Exists(headerFilePath))
            {
                throw new FileNotFoundException($"File not found: {headerFilePath}.");
            }

            // Validate data path
            if (!this.FileSystem.File.Exists(dataFilePath))
            {
                throw new FileNotFoundException($"File not found: {dataFilePath}.");
            }

            // Read the header
            INefsHeader header = null;
            using (var stream = this.FileSystem.File.OpenRead(headerFilePath))
            {
                header = await this.ReadHeaderAsync(stream, headerOffset, headerPart6Offset, p);
            }

            // Create items from header
            var items = header.CreateItemList(dataFilePath, p);

            // Create the archive
            return new NefsArchive(header, items);
        }

        /// <inheritdoc/>
        public async Task<NefsArchive> ReadArchiveAsync(NefsArchiveSource source, NefsProgress p)
        {
            return await this.ReadArchiveAsync(source.HeaderFilePath, source.HeaderOffset, source.HeaderPart6Offset, source.DataFilePath, p);
        }

        /// <summary>
        /// Reads a version 1.6 header from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header from the beginning of the stream.</param>
        /// <param name="part6Stream">The stream that contains part 6/7 data.</param>
        /// <param name="part6Offset">The offset to the start of part 6/7 data.</param>
        /// <param name="intro">The pre-parsed header intro.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header.</returns>
        internal async Task<Nefs16Header> Read16HeaderAsync(
            Stream stream,
            ulong offset,
            Stream part6Stream,
            ulong part6Offset,
            NefsHeaderIntro intro,
            NefsProgress p)
        {
            Nefs16HeaderIntroToc toc = null;
            NefsHeaderPart1 part1 = null;
            NefsHeaderPart2 part2 = null;
            NefsHeaderPart3 part3 = null;
            Nefs16HeaderPart4 part4 = null;
            NefsHeaderPart5 part5 = null;
            Nefs16HeaderPart6 part6 = null;
            NefsHeaderPart7 part7 = null;
            NefsHeaderPart8 part8 = null;

            // Calc weight of each task (8 parts + table of contents)
            var weight = 1.0f / 10.0f;

            using (p.BeginTask(weight, "Reading header intro table of contents"))
            {
                toc = await this.Read16HeaderIntroTocAsync(stream, Nefs16HeaderIntroToc.Offset, p);
            }

            using (p.BeginTask(weight, "Reading header part 1"))
            {
                part1 = await this.ReadHeaderPart1Async(stream, toc.OffsetToPart1, toc.Part1Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 2"))
            {
                part2 = await this.ReadHeaderPart2Async(stream, toc.OffsetToPart2, toc.Part2Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 3"))
            {
                part3 = await this.ReadHeaderPart3Async(stream, toc.OffsetToPart3, toc.Part3Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 4"))
            {
                part4 = await this.Read16HeaderPart4Async(stream, toc.OffsetToPart4, toc.Part4Size, part1, p);
            }

            using (p.BeginTask(weight, "Reading header part 5"))
            {
                part5 = await this.ReadHeaderPart5Async(stream, toc.OffsetToPart5, NefsHeaderPart5.Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 6"))
            {
                var numEntries = (uint)part1.EntriesByIndex.Count;
                part6 = await this.Read16HeaderPart6Async(part6Stream, (uint)part6Offset + toc.OffsetToPart6, numEntries, part1, p);
            }

            using (p.BeginTask(weight, "Reading header part 7"))
            {
                var numEntries = (uint)part2.EntriesByIndex.Count;
                part7 = await this.ReadHeaderPart7Async(part6Stream, (uint)part6Offset + toc.OffsetToPart7, numEntries, p);
            }

            using (p.BeginTask(weight, "Reading header part 8"))
            {
                var part8Size = intro.HeaderSize - toc.OffsetToPart8;
                part8 = await this.ReadHeaderPart8Async(stream, toc.OffsetToPart8, part8Size, p);
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
            await FileData.ReadDataAsync(stream, offset, toc, NefsVersion.Version160, p);
            return toc;
        }

        /// <summary>
        /// Reads header part 4 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="part1">Header part 1.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs16HeaderPart4> Read16HeaderPart4Async(Stream stream, uint offset, uint size, NefsHeaderPart1 part1, NefsProgress p)
        {
            var entries = new List<Nefs16HeaderPart4Entry>();
            var indexLookup = new Dictionary<Guid, uint>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "4"))
            {
                return new Nefs16HeaderPart4(entries, indexLookup);
            }

            // Get entries in part 4
            var numEntries = size / Nefs16HeaderPart4Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    var entry = new Nefs16HeaderPart4Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, NefsVersion.Version200, p);
                    entryOffset += Nefs16HeaderPart4Entry.Size;

                    entries.Add(entry);
                }
            }

            // Create a table to allow looking up a part 4 index by item Guid
            foreach (var p1 in part1.EntriesByIndex)
            {
                indexLookup.Add(p1.Guid, p1.IndexPart4);
            }

            return new Nefs16HeaderPart4(entries, indexLookup);
        }

        /// <summary>
        /// Reads header part 6 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="numItems">The number of entries.</param>
        /// <param name="part1">Header part 1. Used to match part 6 data with an item.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs16HeaderPart6> Read16HeaderPart6Async(Stream stream, uint offset, uint numItems, NefsHeaderPart1 part1, NefsProgress p)
        {
            var entries = new List<Nefs16HeaderPart6Entry>();
            var size = numItems * Nefs16HeaderPart6Entry.Size;

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "6"))
            {
                return new Nefs16HeaderPart6(entries);
            }

            // Get entries in part 6
            var entryOffset = offset;

            for (var i = 0; i < numItems; ++i)
            {
                using (p.BeginTask(1.0f / numItems))
                {
                    // Make sure there is a corresponding index in part 1
                    if (i >= part1.EntriesByIndex.Count)
                    {
                        Log.LogError($"Could not find matching item entry for part 6 index {i} in part 1.");
                        continue;
                    }

                    // Get Guid from part 1. Part 1 entry order matches part 6 entry order.
                    var guid = part1.EntriesByIndex[i].Guid;

                    // Read the entry data
                    var entry = new Nefs16HeaderPart6Entry(guid);
                    await FileData.ReadDataAsync(stream, entryOffset, entry, NefsVersion.Version160, p);
                    entryOffset += Nefs16HeaderPart6Entry.Size;

                    entries.Add(entry);
                }
            }

            return new Nefs16HeaderPart6(entries);
        }

        /// <summary>
        /// Reads a version 2.0 header from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header from the beginning of the stream.</param>
        /// <param name="part6Stream">The stream that contains part 6/7 data.</param>
        /// <param name="part6Offset">The offset to the start of part 6/7 data.</param>
        /// <param name="intro">The pre-parsed header intro.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header.</returns>
        internal async Task<Nefs20Header> Read20HeaderAsync(
            Stream stream,
            ulong offset,
            Stream part6Stream,
            ulong part6Offset,
            NefsHeaderIntro intro,
            NefsProgress p)
        {
            Nefs20HeaderIntroToc toc = null;
            NefsHeaderPart1 part1 = null;
            NefsHeaderPart2 part2 = null;
            NefsHeaderPart3 part3 = null;
            Nefs20HeaderPart4 part4 = null;
            NefsHeaderPart5 part5 = null;
            Nefs20HeaderPart6 part6 = null;
            NefsHeaderPart7 part7 = null;
            NefsHeaderPart8 part8 = null;

            // Calc weight of each task (8 parts + table of contents)
            var weight = 1.0f / 10.0f;

            using (p.BeginTask(weight, "Reading header intro table of contents"))
            {
                toc = await this.Read20HeaderIntroTocAsync(stream, Nefs20HeaderIntroToc.Offset, p);
            }

            using (p.BeginTask(weight, "Reading header part 1"))
            {
                part1 = await this.ReadHeaderPart1Async(stream, toc.OffsetToPart1, toc.Part1Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 2"))
            {
                part2 = await this.ReadHeaderPart2Async(stream, toc.OffsetToPart2, toc.Part2Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 3"))
            {
                part3 = await this.ReadHeaderPart3Async(stream, toc.OffsetToPart3, toc.Part3Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 4"))
            {
                part4 = await this.Read20HeaderPart4Async(stream, toc.OffsetToPart4, toc.Part4Size, part1, p);
            }

            using (p.BeginTask(weight, "Reading header part 5"))
            {
                part5 = await this.ReadHeaderPart5Async(stream, toc.OffsetToPart5, NefsHeaderPart5.Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 6"))
            {
                part6 = await this.Read20HeaderPart6Async(part6Stream, (uint)part6Offset + toc.OffsetToPart6, part1, p);
            }

            using (p.BeginTask(weight, "Reading header part 7"))
            {
                var numEntries = (uint)part2.EntriesByIndex.Count;
                part7 = await this.ReadHeaderPart7Async(part6Stream, (uint)part6Offset + toc.OffsetToPart7, numEntries, p);
            }

            using (p.BeginTask(weight, "Reading header part 8"))
            {
                var part8Size = intro.HeaderSize - toc.OffsetToPart8;
                part8 = await this.ReadHeaderPart8Async(stream, toc.OffsetToPart8, part8Size, p);
            }

            // Validate header hash
            if (!this.ValidateHash(stream, offset, intro))
            {
                Log.LogWarning("Header hash does not match expected value.");
            }

            // The header stream must be disposed
            stream.Dispose();

            return new Nefs20Header(intro, toc, part1, part2, part3, part4, part5, part6, part7, part8);
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
        internal async Task<Nefs20HeaderIntroToc> Read20HeaderIntroTocAsync(Stream stream, uint offset, NefsProgress p)
        {
            var toc = new Nefs20HeaderIntroToc();
            await FileData.ReadDataAsync(stream, offset, toc, NefsVersion.Version200, p);
            return toc;
        }

        /// <summary>
        /// Reads header part 4 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="size">The size of the header part.</param>
        /// <param name="part1">Header part 1.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs20HeaderPart4> Read20HeaderPart4Async(Stream stream, uint offset, uint size, NefsHeaderPart1 part1, NefsProgress p)
        {
            var entries = new List<Nefs20HeaderPart4Entry>();
            var indexLookup = new Dictionary<Guid, uint>();

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "4"))
            {
                return new Nefs20HeaderPart4(entries, indexLookup);
            }

            // Get entries in part 4
            var numEntries = size / Nefs20HeaderPart4Entry.Size;
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    var entry = new Nefs20HeaderPart4Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, NefsVersion.Version200, p);
                    entryOffset += Nefs20HeaderPart4Entry.Size;

                    entries.Add(entry);
                }
            }

            // Create a table to allow looking up a part 4 index by item Guid
            foreach (var p1 in part1.EntriesByIndex)
            {
                indexLookup.Add(p1.Guid, p1.IndexPart4);
            }

            return new Nefs20HeaderPart4(entries, indexLookup);
        }

        /// <summary>
        /// Reads header part 6 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="part1">Header part 1. Used to match part 6 data with an item.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<Nefs20HeaderPart6> Read20HeaderPart6Async(Stream stream, uint offset, NefsHeaderPart1 part1, NefsProgress p)
        {
            var entries = new List<Nefs20HeaderPart6Entry>();
            var numItems = part1.EntriesByIndex.Count;
            var size = numItems * Nefs20HeaderPart6Entry.Size;

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, (uint)size, "6"))
            {
                return new Nefs20HeaderPart6(entries);
            }

            // Get entries in part 6
            var entryOffset = offset;

            for (var i = 0; i < numItems; ++i)
            {
                using (p.BeginTask(1.0f / numItems))
                {
                    // Make sure there is a corresponding index in part 1
                    if (i >= part1.EntriesByIndex.Count)
                    {
                        Log.LogError($"Could not find matching item entry for part 6 index {i} in part 1.");
                        continue;
                    }

                    // Get Guid from part 1. Part 1 entry order matches part 6 entry order.
                    var guid = part1.EntriesByIndex[i].Guid;

                    // Read the entry data
                    var entry = new Nefs20HeaderPart6Entry(guid);
                    await FileData.ReadDataAsync(stream, entryOffset, entry, NefsVersion.Version200, p);
                    entryOffset += Nefs20HeaderPart6Entry.Size;

                    entries.Add(entry);
                }
            }

            return new Nefs20HeaderPart6(entries);
        }

        /// <summary>
        /// Reads the header from an input stream.
        /// </summary>
        /// <param name="originalStream">The stream to read from.</param>
        /// <param name="offset">The offset to the header from the beginning of the stream.</param>
        /// <param name="part6Offset">
        /// The offset to the start of part 6 data from the beginning of the stream.
        /// </param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header.</returns>
        internal async Task<INefsHeader> ReadHeaderAsync(Stream originalStream, ulong offset, ulong part6Offset, NefsProgress p)
        {
            Stream stream;
            INefsHeader header = null;
            NefsHeaderIntro intro = null;

            using (p.BeginTask(0.2f, "Reading header intro"))
            {
                // Decrypt header if needed
                (intro, stream) = await this.ReadHeaderIntroAsync(originalStream, offset, p);
            }

            using (p.BeginTask(0.8f))
            {
                if (intro.NefsVersion == 0x20000)
                {
                    // 2.0.0
                    Log.LogInformation("Detected NeFS version 2.0.");
                    header = await this.Read20HeaderAsync(stream, 0, originalStream, part6Offset, intro, p);
                }
                else if (intro.NefsVersion == 0x10600)
                {
                    // 1.6.0
                    Log.LogInformation("Detected NeFS version 1.6.");
                    header = await this.Read16HeaderAsync(stream, 0, originalStream, part6Offset, intro, p);
                }
                else
                {
                    Log.LogInformation($"Detected unkown NeFS version {intro.NefsVersion}.");
                    header = await this.Read20HeaderAsync(stream, 0, originalStream, part6Offset, intro, p);
                }
            }

            // The header stream must be disposed
            stream.Dispose();

            return header;
        }

        /// <summary>
        /// Reads the header intro from an input stream. Returns a new stream that contains the
        /// header data. This stream must be disposed by the caller. If the header is encrypted, the
        /// header data is decrypted before being placed in the new stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header intro from the beginning of the stream.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header intro and the stream to use for the rest of the header.</returns>
        internal async Task<(NefsHeaderIntro Intro, Stream HeaderStream)> ReadHeaderIntroAsync(
            Stream stream,
            ulong offset,
            NefsProgress p)
        {
            // The decrypted stream will need to be disposed by the caller
            var decryptedStream = new MemoryStream();
            NefsHeaderIntro intro;

            // Read magic number (first four bytes)
            stream.Seek((long)offset, SeekOrigin.Begin);
            var magicNum = new UInt32Type(0);
            await magicNum.ReadAsync(stream, offset, p);

            // Reset stream position
            stream.Seek((long)offset, SeekOrigin.Begin);

            // Check magic number
            if (magicNum.Value == NefsHeaderIntro.NefsMagicNumber)
            {
                // This is a non-encrypted NeFS header
                intro = new NefsHeaderIntro();
                await FileData.ReadDataAsync(stream, offset, intro, NefsVersion.Version200, p);

                // Copy the entire header to the decrypted stream (nothing to decrypt)
                stream.Seek((long)offset, SeekOrigin.Begin);
                await stream.CopyPartialAsync(decryptedStream, intro.HeaderSize, p.CancellationToken);
            }
            else
            {
                // Magic number is incorrect, assume file is encrpyted
                Log.LogInformation("Header magic number mismatch, assuming header is encrypted.");

                // Encrypted headers:
                // - Headers are "encrypted" in a two-step process. RSA-1024. No padding is used.
                // - First 0x80 bytes are signed with an RSA private key (data -> decrypt ->
                // scrambled data).
                // - Must use an RSA 1024-bit public key to unscramble the data (scrambled data ->
                // encrypt -> data).
                // - For DiRT Rally 2 this public key is stored in the main executable.
                byte[] encryptedHeader = new byte[NefsHeaderIntro.Size + 1]; // TODO : Why the +1?
                await stream.ReadAsync(encryptedHeader, 0, (int)NefsHeaderIntro.Size, p.CancellationToken);
                encryptedHeader[NefsHeaderIntro.Size] = 0;

                // Use big integers instead of RSA since the c# implementation forces the use of padding.
                var n = new BigInteger(this.RsaPublicKey);
                var e = new BigInteger(this.RsaExponent);
                var m = new BigInteger(encryptedHeader);

                // Decrypt the header intro
                byte[] decrypted = BigInteger.ModPow(m, e, n).ToByteArray();
                decryptedStream.Write(decrypted, 0, decrypted.Length);

                // Fill any leftover space with zeros
                if (decrypted.Length != NefsHeaderIntro.Size)
                {
                    for (int i = 0; i < (NefsHeaderIntro.Size - decrypted.Length); i++)
                    {
                        decryptedStream.WriteByte(0);
                    }
                }

                // Read header intro data from decrypted stream
                intro = new NefsHeaderIntro(isEncrpyted: true);
                await FileData.ReadDataAsync(decryptedStream, 0, intro, NefsVersion.Version200, p);

                // The rest of the header is encrypted using AES-256, decrypt using the key from the
                // header intro
                byte[] key = intro.GetAesKey();
                var headerSize = intro.HeaderSize;

                // Decrypt the rest of the header
                using (var rijAlg = new RijndaelManaged())
                {
                    rijAlg.KeySize = 256;
                    rijAlg.Key = key;
                    rijAlg.Mode = CipherMode.ECB;
                    rijAlg.BlockSize = 128;
                    rijAlg.Padding = PaddingMode.Zeros;

                    var decryptor = rijAlg.CreateDecryptor();
                    decryptedStream.Seek(0, SeekOrigin.End);

                    // Decrypt the data - make sure to leave open the base stream
                    using (var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read, true))
                    {
                        // Decrypt data from input stream and copy to the decrypted stream
                        await cryptoStream.CopyPartialAsync(decryptedStream, headerSize, p.CancellationToken);
                    }
                }
            }

            return (intro, decryptedStream);
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
            var entries = new List<NefsHeaderPart1Entry>();

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
                    var guid = Guid.NewGuid();
                    var entry = new NefsHeaderPart1Entry(guid);
                    await FileData.ReadDataAsync(stream, entryOffset, entry, NefsVersion.Version200, p);
                    entryOffset += NefsHeaderPart1Entry.Size;
                    entries.Add(entry);
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
            var entries = new List<NefsHeaderPart2Entry>();

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
                    await FileData.ReadDataAsync(stream, entryOffset, entry, NefsVersion.Version200, p);
                    entryOffset += NefsHeaderPart2Entry.Size;

                    entries.Add(entry);
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

            return new NefsHeaderPart3(entries);
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
            await FileData.ReadDataAsync(stream, offset, part5, NefsVersion.Version200, p);
            return part5;
        }

        /// <summary>
        /// Reads header part 7 from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">The offset to the header part from the beginning of the stream.</param>
        /// <param name="numEntries">Number of entries.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header part.</returns>
        internal async Task<NefsHeaderPart7> ReadHeaderPart7Async(Stream stream, uint offset, uint numEntries, NefsProgress p)
        {
            var entries = new List<NefsHeaderPart7Entry>();
            var size = numEntries * NefsHeaderPart7Entry.Size;

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, (uint)size, "7"))
            {
                return new NefsHeaderPart7(entries);
            }

            // Get entries in part 7
            var entryOffset = offset;

            for (var i = 0; i < numEntries; ++i)
            {
                using (p.BeginTask(1.0f / numEntries))
                {
                    // Read the entry data
                    var entry = new NefsHeaderPart7Entry();
                    await FileData.ReadDataAsync(stream, entryOffset, entry, NefsVersion.Version200, p);
                    entryOffset += NefsHeaderPart7Entry.Size;

                    entries.Add(entry);
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
            var part8 = new NefsHeaderPart8(size);

            // Validate inputs
            if (!this.ValidateHeaderPartStream(stream, offset, size, "8"))
            {
                return part8;
            }

            await stream.ReadAsync(part8.AllTheData.Value, 0, (int)size, p.CancellationToken);
            return part8;
        }

        private bool ValidateHash(Stream stream, ulong offset, NefsHeaderIntro intro)
        {
            var dataToHashSize = intro.IsEncrypted ? intro.HeaderSize - 0x22 : intro.HeaderSize - 0x20;
            byte[] dataToHash = new byte[dataToHashSize];

            stream.Seek((long)offset, SeekOrigin.Begin);
            stream.Read(dataToHash, 0, 4);

            stream.Seek(0x24, SeekOrigin.Begin);
            if (!intro.IsEncrypted)
            {
                stream.Read(dataToHash, 4, (int)intro.HeaderSize - 0x24);
            }
            else
            {
                stream.Read(dataToHash, 4, 0x5A);
                stream.Seek(0x80, SeekOrigin.Begin);
                stream.Read(dataToHash, 0x5E, (int)intro.HeaderSize - 0x80);
            }

            using (var hash = SHA256.Create())
            {
                byte[] hashOut = hash.ComputeHash(dataToHash);
                return hashOut.SequenceEqual(intro.ExpectedHash);
            }
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
