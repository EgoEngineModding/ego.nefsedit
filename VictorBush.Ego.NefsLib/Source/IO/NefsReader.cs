// See LICENSE.txt for license information.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VictorBush.Ego.NefsLib.Tests")]

namespace VictorBush.Ego.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Numerics;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using VictorBush.Ego.NefsLib.DataTypes;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;
    using VictorBush.Ego.NefsLib.Utility;

    /// <summary>
    /// Reads NeFS archives.
    /// </summary>
    public class NefsReader : INefsReader
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
        public async Task<NefsArchive> ReadArchiveAsync(string filePath, NefsProgress p)
        {
            return await this.ReadArchiveAsync(filePath, NefsHeader.IntroOffset, filePath, p);
        }

        /// <inheritdoc/>
        public async Task<NefsArchive> ReadArchiveAsync(
            string headerFilePath,
            ulong headerOffset,
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
            NefsHeader header = null;
            using (var stream = this.FileSystem.File.OpenRead(headerFilePath))
            {
                header = await this.ReadHeaderAsync(stream, headerOffset, p);
            }

            // Create items from header
            var items = this.CreateItems(dataFilePath, header, p);

            // Create the archive
            return new NefsArchive(header, items);
        }

        /// <inheritdoc/>
        public async Task<NefsArchive> ReadArchiveAsync(NefsArchiveSource source, NefsProgress p)
        {
            return await this.ReadArchiveAsync(source.HeaderFilePath, source.HeaderOffset, source.DataFilePath, p);
        }

        /// <summary>
        /// Reads the header from an input stream.
        /// </summary>
        /// <param name="originalStream">The stream to read from.</param>
        /// <param name="offset">The offset to the header from the beginning of the stream.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header.</returns>
        internal async Task<NefsHeader> ReadHeaderAsync(Stream originalStream, ulong offset, NefsProgress p)
        {
            Stream stream;
            NefsHeaderIntro intro = null;
            NefsHeaderIntroToc toc = null;
            NefsHeaderPart1 part1 = null;
            NefsHeaderPart2 part2 = null;
            NefsHeaderPart3 part3 = null;
            NefsHeaderPart4 part4 = null;
            NefsHeaderPart5 part5 = null;
            NefsHeaderPart6 part6 = null;
            NefsHeaderPart7 part7 = null;
            NefsHeaderPart8 part8 = null;

            // Calc weight of each task (8 parts + intro + table of contents)
            var weight = 1.0f / 10.0f;

            using (p.BeginTask(weight, "Reading header intro"))
            {
                // Decrypt header if needed
                (intro, stream) = await this.ReadHeaderIntroAsync(originalStream, offset, p);
            }

            using (p.BeginTask(weight, "Reading header intro table of contents"))
            {
                toc = await this.ReadHeaderIntroTocAsync(stream, NefsHeaderIntroToc.Offset, p);
            }

            using (p.BeginTask(weight, "Reading header part 1"))
            {
                part1 = await this.ReadHeaderPart1Async(stream, toc.OffsetToPart1.Value, toc.Part1Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 2"))
            {
                part2 = await this.ReadHeaderPart2Async(stream, toc.OffsetToPart2.Value, toc.Part2Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 3"))
            {
                part3 = await this.ReadHeaderPart3Async(stream, toc.OffsetToPart3.Value, toc.Part3Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 4"))
            {
                part4 = await this.ReadHeaderPart4Async(stream, toc.OffsetToPart4.Value, toc.Part4Size, part1, part2, p);
            }

            using (p.BeginTask(weight, "Reading header part 5"))
            {
                part5 = await this.ReadHeaderPart5Async(stream, toc.OffsetToPart5.Value, toc.Part5Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 6"))
            {
                part6 = await this.ReadHeaderPart6Async(stream, toc.OffsetToPart6.Value, toc.Part6Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 7"))
            {
                part7 = await this.ReadHeaderPart7Async(stream, toc.OffsetToPart7.Value, toc.Part7Size, p);
            }

            using (p.BeginTask(weight, "Reading header part 8"))
            {
                // TODO : Get correct part 8 size
                part8 = await this.ReadHeaderPart8Async(stream, toc.OffsetToPart8.Value, 0, p);
            }

            // The header stream must be disposed
            stream.Dispose();

            return new NefsHeader(intro, toc, part1, part2, part3, part4, part5, part6, part7, part8);
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
                await FileData.ReadDataAsync(stream, offset, intro, p);

                // Copy the entire header to the decrypted stream (nothing to decrypt)
                stream.Seek((long)offset, SeekOrigin.Begin);
                await stream.CopyPartialAsync(decryptedStream, intro.HeaderSize.Value, p.CancellationToken);
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
                await FileData.ReadDataAsync(decryptedStream, 0, intro, p);

                // The rest of the header is encrypted using AES-256, decrypt using the key from the
                // header intro
                byte[] key = intro.GetAesKey();
                var headerSize = intro.HeaderSize.Value;

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
        /// Reads the header intro table of contents from an input stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="offset">
        /// The offset to the header intro table of contents from the beginning of the stream.
        /// </param>
        /// <param name="p">Progress info.</param>
        /// <returns>The loaded header intro offets data.</returns>
        internal async Task<NefsHeaderIntroToc> ReadHeaderIntroTocAsync(Stream stream, uint offset, NefsProgress p)
        {
            var toc = new NefsHeaderIntroToc();
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

                    var id = new NefsItemId(entry.Id.Value);
                    if (entries.ContainsKey(id))
                    {
                        Log.LogError($"Found duplicate item id in part 1: {id.Value}");
                        continue;
                    }

                    entries.Add(id, entry);
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

                    var id = new NefsItemId(entry.Id.Value);
                    if (entries.ContainsKey(id))
                    {
                        Log.LogError($"Found duplicate item id in part 2: {id.Value}");
                        continue;
                    }

                    entries.Add(id, entry);
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

                    // Part 1 entry
                    if (!part1.EntriesById.ContainsKey(id))
                    {
                        Log.LogError($"Failed to find part 1 entry for item {id} when reading part 4.");
                        continue;
                    }

                    var p1 = part1.EntriesById[id];

                    // Part 2 entry
                    if (!part2.EntriesById.ContainsKey(id))
                    {
                        Log.LogError($"Failed to find part 2 entry for item {id} when reading part 4.");
                        continue;
                    }

                    var p2 = part2.EntriesById[id];

                    // Create part 4 entry
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
            return await Task.FromResult(new NefsHeaderPart8());
        }

        private NefsItemList CreateItems(string dataFilePath, NefsHeader h, NefsProgress p)
        {
            var items = new NefsItemList(dataFilePath);
            var numItems = h.TableOfContents.Part1Size / NefsHeaderPart1Entry.Size;

            for (var i = 0; i < numItems; ++i)
            {
                // Create the item
                var id = new NefsItemId((uint)i);

                try
                {
                    var item = NefsItem.CreateFromHeader(id, h, items);
                    items.Add(item);
                }
                catch (Exception)
                {
                    Log.LogError($"Failed to create item {id}, skipping.");
                }
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
