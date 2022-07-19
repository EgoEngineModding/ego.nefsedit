// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VictorBush.Ego.NefsLib.Tests")]

namespace VictorBush.Ego.NefsLib.IO;

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
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsReader"/> class.
	/// </summary>
	/// <param name="fileSystem">The file system used by the factory.</param>
	/// <param name="rsaPublicKey">Specifies a different RSA public key to use for encrypted headers.</param>
	/// <param name="rsaExponent">Specifies a different RSA exponent to use for encrypted headers.</param>
	public NefsReader(IFileSystem fileSystem, byte[]? rsaPublicKey, byte[]? rsaExponent)
	{
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		RsaPublicKey = rsaPublicKey ?? DefaultRsaPublicKey;
		RsaExponent = rsaExponent ?? DefaultRsaExponent;
	}

	private IFileSystem FileSystem { get; }

	private byte[] RsaExponent { get; }

	private byte[] RsaPublicKey { get; }

	/// <inheritdoc/>
	public async Task<NefsArchive> ReadArchiveAsync(string filePath, NefsProgress p)
	{
		if (!FileSystem.File.Exists(filePath))
		{
			throw new FileNotFoundException($"File not found: {filePath}.");
		}

		// Read the header
		INefsHeader header;
		using (var stream = FileSystem.File.OpenRead(filePath))
		{
			header = await ReadHeaderAsync(stream, 0, p);
		}

		// Create items from header
		var items = header.CreateItemList(filePath, p);

		// Create the archive
		return new NefsArchive(header, items);
	}

	/// <inheritdoc/>
	public Task<NefsArchive> ReadArchiveAsync(NefsArchiveSource source, NefsProgress p)
	{
		switch (source)
		{
			case StandardSource standard:
				return ReadArchiveAsync(standard.FilePath, p);

			case HeadlessSource gameDat:
				return ReadGameDatArchiveAsync(gameDat.FilePath, gameDat.HeaderFilePath, gameDat.PrimaryOffset, gameDat.PrimarySize, gameDat.SecondaryOffset, gameDat.SecondarySize, p);

			case NefsInjectSource nefsInject:
				return ReadNefsInjectArchiveAsync(nefsInject.FilePath, nefsInject.NefsInjectFilePath, p);

			default:
				throw new ArgumentException("Unknown source type.");
		}
	}

	/// <inheritdoc/>
	public async Task<NefsArchive> ReadGameDatArchiveAsync(string dataFilePath, string headerFilePath, long primaryOffset, int? primarySize, long secondaryOffset, int? secondarySize, NefsProgress p)
	{
		// Validate header path
		if (!FileSystem.File.Exists(headerFilePath))
		{
			throw new FileNotFoundException($"File not found: {headerFilePath}.");
		}

		// Validate data path
		if (!FileSystem.File.Exists(dataFilePath))
		{
			throw new FileNotFoundException($"File not found: {dataFilePath}.");
		}

		// Read the header
		INefsHeader header;
		using (var stream = FileSystem.File.OpenRead(headerFilePath))
		{
			header = await ReadSplitHeaderAsync(stream, primaryOffset, primarySize, secondaryOffset, secondarySize, p);
		}

		// Create items from header
		var items = header.CreateItemList(dataFilePath, p);

		// Create the archive
		return new NefsArchive(header, items);
	}

	public async Task<NefsArchive> ReadNefsInjectArchiveAsync(string dataFilePath, string nefsInjectFilePath, NefsProgress p)
	{
		if (!FileSystem.File.Exists(nefsInjectFilePath))
		{
			throw new FileNotFoundException($"File not found: {nefsInjectFilePath}.");
		}

		INefsHeader header;
		using (var stream = FileSystem.File.OpenRead(nefsInjectFilePath))
		{
			NefsInjectHeader nefsInject;
			using (p.BeginTask(0.1f, "Reading NefsInject header"))
			{
				nefsInject = await ReadNefsInjectHeaderAsync(stream, 0, p);
			}

			using (p.BeginTask(0.9f, "Reading Nefs header"))
			{
				header = await ReadSplitHeaderAsync(stream, nefsInject.PrimaryOffset, nefsInject.PrimarySize, nefsInject.SecondaryOffset, nefsInject.SecondarySize, p);
			}
		}

		// Create items from header
		var items = header.CreateItemList(dataFilePath, p);

		// Create the archive
		return new NefsArchive(header, items);
	}

	/// <summary>
	/// Decrypts an encrypted Dirt Rally 2 header into a new stream. The caller is responsible for disposing the stream.
	/// </summary>
	/// <param name="stream">The stream containing the encrypted header.</param>
	/// <param name="offset">The offset to the header from the beginning of the stream.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The decrypted stream.</returns>
	internal async Task<(NefsHeaderIntro Intro, Stream Stream)> DecryptDirtRally2Header(
		Stream stream,
		long offset,
		NefsProgress p)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		// The decrypted stream will need to be disposed by the caller
		var decryptedStream = new MemoryStream();

		// Encrypted headers:
		// - Headers are "encrypted" in a two-step process. RSA-1024. No padding is used.
		// - First 0x80 bytes are signed with an RSA private key (data -> decrypt -> scrambled data).
		// - Must use an RSA 1024-bit public key to unscramble the data (scrambled data -> encrypt -> data).
		// - For DiRT Rally 2 this public key is stored in the main executable.
		byte[] encryptedHeader = new byte[NefsHeaderIntro.Size + 1]; // TODO : Why the +1?
		await stream.ReadAsync(encryptedHeader, 0, (int)NefsHeaderIntro.Size, p.CancellationToken);
		encryptedHeader[NefsHeaderIntro.Size] = 0;

		// Use big integers instead of RSA since the c# implementation forces the use of padding.
		var n = new BigInteger(RsaPublicKey);
		var e = new BigInteger(RsaExponent);
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
		var intro = new NefsHeaderIntro(isEncrypted: true);
		await FileData.ReadDataAsync(decryptedStream, 0, intro, p);

		// The rest of the header is encrypted using AES-256, decrypt using the key from the header intro
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
			using (var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
			{
				// Decrypt data from input stream and copy to the decrypted stream
				await cryptoStream.CopyPartialAsync(decryptedStream, headerSize, p.CancellationToken);
			}
		}

		return (intro, decryptedStream);
	}

	internal async Task<INefsHeader> ReadEncryptedDirtRally2Header(Stream stream, long offset, NefsProgress p)
	{
		var (intro, decryptedStream) = await DecryptDirtRally2Header(stream, offset, p);
		if (intro.MagicNumber != NefsHeaderIntro.NefsMagicNumber)
		{
			throw new InvalidOperationException("Failed to decrypt header. Only Dirt Rally 2 encrypted archives are supported.");
		}

		// Only support encrypted headers with no special offsets (we have no examples of encrypted game.dat, etc)
		var header = await ReadHeaderVersion20Async(decryptedStream, 0, 0, intro, p);
		await ValidateEncryptedHeaderAsync(decryptedStream, offset, intro);
		decryptedStream.Dispose();
		return header;
	}

	internal async Task<INefsHeader> ReadHeaderAsync(Stream stream, long offset, NefsProgress p)
	{
		INefsHeader header;
		NefsHeaderIntro intro;

		var validMagicNum = await ValidateMagicNumberAsync(stream, offset, p);
		if (!validMagicNum)
		{
			Log.LogInformation("Header magic number mismatch, assuming a Dirt Rally 2 encrypted archive.");
			header = await ReadEncryptedDirtRally2Header(stream, offset, p);
			return header;
		}

		using (p.BeginTask(0.2f, "Reading header intro"))
		{
			intro = await ReadHeaderIntroAsync(stream, offset, p);
		}

		using (p.BeginTask(0.8f, "Reading header"))
		{
			if (intro.NefsVersion == (uint)NefsVersion.Version200)
			{
				// 2.0.0
				Log.LogInformation("Detected NeFS version 2.0.");
				header = await ReadHeaderVersion20Async(stream, offset, offset, intro, p);
			}
			else if (intro.NefsVersion == (uint)NefsVersion.Version160)
			{
				// 1.6.0
				Log.LogInformation("Detected NeFS version 1.6.");
				header = await ReadHeaderVersion16Async(stream, offset, offset, intro, p);
			}
			else
			{
				Log.LogError($"Detected unkown NeFS version {intro.NefsVersion}. Treating as 2.0.");
				header = await ReadHeaderVersion20Async(stream, offset, offset, intro, p);
			}
		}

		await ValidateHeaderHashAsync(stream, offset, intro);
		return header;
	}

	/// <summary>
	/// Reads the header from an input stream.
	/// </summary>
	/// <summary>
	/// Reads the header intro from an input stream. This is for non-encrypted headers only.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header intro from the beginning of the stream.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header intro.</returns>
	internal async Task<NefsHeaderIntro> ReadHeaderIntroAsync(
		Stream stream,
		long offset,
		NefsProgress p)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		var intro = new NefsHeaderIntro();
		await FileData.ReadDataAsync(stream, offset, intro, p);
		return intro;
	}

	/// <summary>
	/// Reads the header intro table of contents from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header intro table of contents from the beginning of the stream.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header intro offets data.</returns>
	internal async Task<Nefs16HeaderIntroToc> ReadHeaderIntroTocVersion16Async(Stream stream, long offset, NefsProgress p)
	{
		var toc = new Nefs16HeaderIntroToc();
		await FileData.ReadDataAsync(stream, offset, toc, p);
		return toc;
	}

	/// <summary>
	/// Reads the header intro table of contents from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header intro table of contents from the beginning of the stream.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header intro offets data.</returns>
	internal async Task<Nefs20HeaderIntroToc> ReadHeaderIntroTocVersion20Async(Stream stream, long offset, NefsProgress p)
	{
		var toc = new Nefs20HeaderIntroToc();
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
	internal async Task<NefsHeaderPart1> ReadHeaderPart1Async(Stream stream, long offset, int size, NefsProgress p)
	{
		var entries = new List<NefsHeaderPart1Entry>();

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "1"))
		{
			return new NefsHeaderPart1(entries);
		}

		// Get entries in part 1
		var numEntries = size / NefsHeaderPart1.EntrySize;
		var entryOffset = offset;

		for (var i = 0; i < numEntries; ++i)
		{
			using (p.BeginTask(1.0f / numEntries))
			{
				var guid = Guid.NewGuid();
				var entry = new NefsHeaderPart1Entry(guid);
				await FileData.ReadDataAsync(stream, entryOffset, entry, p);
				entryOffset += NefsHeaderPart1.EntrySize;
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
	internal async Task<NefsHeaderPart2> ReadHeaderPart2Async(Stream stream, long offset, int size, NefsProgress p)
	{
		var entries = new List<NefsHeaderPart2Entry>();

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "2"))
		{
			return new NefsHeaderPart2(entries);
		}

		// Get entries in part 2
		var numEntries = size / NefsHeaderPart2.EntrySize;
		var entryOffset = offset;

		for (var i = 0; i < numEntries; ++i)
		{
			using (p.BeginTask(1.0f / numEntries))
			{
				var entry = new NefsHeaderPart2Entry();
				await FileData.ReadDataAsync(stream, entryOffset, entry, p);
				entryOffset += NefsHeaderPart2.EntrySize;

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
	internal async Task<NefsHeaderPart3> ReadHeaderPart3Async(Stream stream, long offset, int size, NefsProgress p)
	{
		var entries = new List<string>();

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "3"))
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
				var nullOffset = size;
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
					// No null terminator found, assume end of part 3. There can be a few garbage bytes at the end of
					// this part.
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
	/// Reads header part 4 from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="part1">Header part 1.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal async Task<Nefs16HeaderPart4> ReadHeaderPart4Version16Async(Stream stream, long offset, int size, NefsHeaderPart1 part1, NefsProgress p)
	{
		var entries = new List<Nefs16HeaderPart4Entry>();
		var indexLookup = new Dictionary<Guid, uint>();

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "4"))
		{
			return new Nefs16HeaderPart4(entries, indexLookup, 0);
		}

		// Get entries in part 4
		var numEntries = size / Nefs16HeaderPart4.EntrySize;
		var entryOffset = offset;

		for (var i = 0; i < numEntries; ++i)
		{
			using (p.BeginTask(1.0f / numEntries))
			{
				var entry = new Nefs16HeaderPart4Entry();
				await FileData.ReadDataAsync(stream, entryOffset, entry, p);
				entryOffset += Nefs16HeaderPart4.EntrySize;

				entries.Add(entry);
			}
		}

		// Create a table to allow looking up a part 4 index by item Guid
		foreach (var p1 in part1.EntriesByIndex)
		{
			indexLookup.Add(p1.Guid, p1.IndexPart4);
		}

		// Get the unkown last value at the end of part 4
		var endValue = new UInt32Type(0);
		await endValue.ReadAsync(stream, stream.Position, p);

		return new Nefs16HeaderPart4(entries, indexLookup, endValue.Value);
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
	internal async Task<Nefs20HeaderPart4> ReadHeaderPart4Version20Async(Stream stream, long offset, int size, NefsHeaderPart1 part1, NefsProgress p)
	{
		var entries = new List<Nefs20HeaderPart4Entry>();
		var indexLookup = new Dictionary<Guid, uint>();

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "4"))
		{
			return new Nefs20HeaderPart4(entries, indexLookup);
		}

		// Get entries in part 4
		var numEntries = size / Nefs20HeaderPart4.EntrySize;
		var entryOffset = offset;

		for (var i = 0; i < numEntries; ++i)
		{
			using (p.BeginTask(1.0f / numEntries))
			{
				var entry = new Nefs20HeaderPart4Entry();
				await FileData.ReadDataAsync(stream, entryOffset, entry, p);
				entryOffset += Nefs20HeaderPart4.EntrySize;

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
	/// Reads header part 5 from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal async Task<NefsHeaderPart5> ReadHeaderPart5Async(Stream stream, long offset, int size, NefsProgress p)
	{
		var part5 = new NefsHeaderPart5();

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "5"))
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
	/// <param name="numItems">The number of entries.</param>
	/// <param name="part1">Header part 1. Used to match part 6 data with an item.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal async Task<Nefs16HeaderPart6> ReadHeaderPart6Version16Async(Stream stream, long offset, int numItems, NefsHeaderPart1 part1, NefsProgress p)
	{
		var entries = new List<Nefs16HeaderPart6Entry>();
		var size = numItems * Nefs16HeaderPart6.EntrySize;

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, (int)size, "6"))
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
				await FileData.ReadDataAsync(stream, entryOffset, entry, p);
				entryOffset += Nefs16HeaderPart6.EntrySize;

				entries.Add(entry);
			}
		}

		return new Nefs16HeaderPart6(entries);
	}

	/// <summary>
	/// Reads header part 6 from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="part1">Header part 1. Used to match part 6 data with an item.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal async Task<Nefs20HeaderPart6> ReadHeaderPart6Version20Async(Stream stream, long offset, NefsHeaderPart1 part1, NefsProgress p)
	{
		var entries = new List<Nefs20HeaderPart6Entry>();
		var numItems = part1.EntriesByIndex.Count;
		var size = numItems * Nefs20HeaderPart6.EntrySize;

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "6"))
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
				await FileData.ReadDataAsync(stream, entryOffset, entry, p);
				entryOffset += Nefs20HeaderPart6.EntrySize;

				entries.Add(entry);
			}
		}

		return new Nefs20HeaderPart6(entries);
	}

	/// <summary>
	/// Reads header part 7 from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="numEntries">Number of entries.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal async Task<NefsHeaderPart7> ReadHeaderPart7Async(Stream stream, long offset, int numEntries, NefsProgress p)
	{
		var entries = new List<NefsHeaderPart7Entry>();
		var size = numEntries * NefsHeaderPart7.EntrySize;

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "7"))
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
				await FileData.ReadDataAsync(stream, entryOffset, entry, p);
				entryOffset += NefsHeaderPart7.EntrySize;

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
	/// <param name="hashBlockSize">The block size specified by the header used to split up the file data for hashing.</param>
	/// <param name="part5">Header part 5.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal async Task<NefsHeaderPart8> ReadHeaderPart8Async(Stream stream, long offset, int hashBlockSize, NefsHeaderPart5 part5, NefsProgress p)
	{
		// Archives can specify a 0 hash block size (use default in this case)
		if (hashBlockSize == 0)
		{
			hashBlockSize = NefsWriter.DefaultHashBlockSize;
		}

		var totalCompressedDataSize = part5.DataSize - part5.FirstDataOffset;
		var numHashes = (int)(totalCompressedDataSize / (uint)hashBlockSize);
		var part8 = new NefsHeaderPart8(numHashes);

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, part8.Size, "8"))
		{
			return part8;
		}

		await FileData.ReadDataAsync(stream, offset, part8, p);
		return part8;
	}

	internal async Task<Nefs16Header> ReadHeaderVersion16Async(
		Stream stream,
		long primaryOffset,
		long secondaryOffset,
		NefsHeaderIntro intro,
		NefsProgress p)
	{
		Nefs16HeaderIntroToc toc;
		NefsHeaderPart1 part1;
		NefsHeaderPart2 part2;
		NefsHeaderPart3 part3;
		Nefs16HeaderPart4 part4;
		NefsHeaderPart5 part5;
		Nefs16HeaderPart6 part6;
		NefsHeaderPart7 part7;
		NefsHeaderPart8 part8;

		// Calc weight of each task (8 parts + table of contents)
		var weight = 1.0f / 9.0f;

		using (p.BeginTask(weight, "Reading header intro table of contents"))
		{
			toc = await ReadHeaderIntroTocVersion16Async(stream, primaryOffset + Nefs16HeaderIntroToc.Offset, p);
		}

		using (p.BeginTask(weight, "Reading header part 1"))
		{
			part1 = await ReadHeaderPart1Async(stream, primaryOffset + toc.OffsetToPart1, (int)toc.Part1Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 2"))
		{
			part2 = await ReadHeaderPart2Async(stream, primaryOffset + toc.OffsetToPart2, (int)toc.Part2Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 3"))
		{
			part3 = await ReadHeaderPart3Async(stream, primaryOffset + toc.OffsetToPart3, (int)toc.Part3Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 4"))
		{
			part4 = await ReadHeaderPart4Version16Async(stream, primaryOffset + toc.OffsetToPart4, (int)toc.Part4Size, part1, p);
		}

		using (p.BeginTask(weight, "Reading header part 5"))
		{
			part5 = await ReadHeaderPart5Async(stream, primaryOffset + toc.OffsetToPart5, NefsHeaderPart5.Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 6"))
		{
			var numEntries = part1.EntriesByIndex.Count;
			part6 = await ReadHeaderPart6Version16Async(stream, secondaryOffset + toc.OffsetToPart6, numEntries, part1, p);
		}

		using (p.BeginTask(weight, "Reading header part 7"))
		{
			var numEntries = part2.EntriesByIndex.Count;
			part7 = await ReadHeaderPart7Async(stream, secondaryOffset + toc.OffsetToPart7, numEntries, p);
		}

		using (p.BeginTask(weight, "Reading header part 8"))
		{
			// Part 8 must use primary offset because, for split headers, it is contained in the primary header section
			// (after part 5)
			part8 = await ReadHeaderPart8Async(stream, primaryOffset + toc.OffsetToPart8, (int)toc.HashBlockSize, part5, p);
		}

		return new Nefs16Header(intro, toc, part1, part2, part3, part4, part5, part6, part7, part8);
	}

	internal async Task<Nefs20Header> ReadHeaderVersion20Async(
		Stream stream,
		long primaryOffset,
		long secondaryOffset,
		NefsHeaderIntro intro,
		NefsProgress p)
	{
		Nefs20HeaderIntroToc toc;
		NefsHeaderPart1 part1;
		NefsHeaderPart2 part2;
		NefsHeaderPart3 part3;
		Nefs20HeaderPart4 part4;
		NefsHeaderPart5 part5;
		Nefs20HeaderPart6 part6;
		NefsHeaderPart7 part7;
		NefsHeaderPart8 part8;

		// Calc weight of each task (8 parts + table of contents)
		var weight = 1.0f / 9.0f;

		using (p.BeginTask(weight, "Reading header intro table of contents"))
		{
			toc = await ReadHeaderIntroTocVersion20Async(stream, primaryOffset + Nefs20HeaderIntroToc.Offset, p);
		}

		using (p.BeginTask(weight, "Reading header part 1"))
		{
			part1 = await ReadHeaderPart1Async(stream, primaryOffset + toc.OffsetToPart1, (int)toc.Part1Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 2"))
		{
			part2 = await ReadHeaderPart2Async(stream, primaryOffset + toc.OffsetToPart2, (int)toc.Part2Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 3"))
		{
			part3 = await ReadHeaderPart3Async(stream, primaryOffset + toc.OffsetToPart3, (int)toc.Part3Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 4"))
		{
			part4 = await ReadHeaderPart4Version20Async(stream, primaryOffset + toc.OffsetToPart4, (int)toc.Part4Size, part1, p);
		}

		using (p.BeginTask(weight, "Reading header part 5"))
		{
			part5 = await ReadHeaderPart5Async(stream, primaryOffset + toc.OffsetToPart5, NefsHeaderPart5.Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 6"))
		{
			part6 = await ReadHeaderPart6Version20Async(stream, secondaryOffset + toc.OffsetToPart6, part1, p);
		}

		using (p.BeginTask(weight, "Reading header part 7"))
		{
			var numEntries = part2.EntriesByIndex.Count;
			part7 = await ReadHeaderPart7Async(stream, secondaryOffset + toc.OffsetToPart7, numEntries, p);
		}

		using (p.BeginTask(weight, "Reading header part 8"))
		{
			var hashBlockSize = NefsWriter.DefaultHashBlockSize;
			part8 = await ReadHeaderPart8Async(stream, primaryOffset + toc.OffsetToPart8, hashBlockSize, part5, p);
		}

		return new Nefs20Header(intro, toc, part1, part2, part3, part4, part5, part6, part7, part8);
	}

	internal async Task<NefsInjectHeader> ReadNefsInjectHeaderAsync(Stream stream, long offset, NefsProgress p)
	{
		var nefsInject = new NefsInjectHeader();
		await FileData.ReadDataAsync(stream, offset, nefsInject, p);
		return nefsInject;
	}

	internal async Task<INefsHeader> ReadSplitHeaderAsync(
		Stream stream,
		long primaryOffset,
		int? primarySize,
		long secondaryOffset,
		int? secondarySize,
		NefsProgress p)
	{
		INefsHeader header;
		NefsHeaderIntro intro;

		var validMagicNum = await ValidateMagicNumberAsync(stream, primaryOffset, p);
		if (!validMagicNum)
		{
			throw new InvalidOperationException("Header magic number mismatch, aborting.");
		}

		using (p.BeginTask(0.2f, "Reading header intro"))
		{
			intro = await ReadHeaderIntroAsync(stream, primaryOffset, p);
		}

		using (p.BeginTask(0.8f, "Reading header"))
		{
			if (intro.NefsVersion == (uint)NefsVersion.Version200)
			{
				// 2.0.0
				Log.LogInformation("Detected NeFS version 2.0.");
				header = await ReadHeaderVersion20Async(stream, primaryOffset, secondaryOffset, intro, p);
			}
			else if (intro.NefsVersion == (uint)NefsVersion.Version160)
			{
				// 1.6.0
				Log.LogInformation("Detected NeFS version 1.6.");
				header = await ReadHeaderVersion16Async(stream, primaryOffset, secondaryOffset, intro, p);
			}
			else
			{
				Log.LogError($"Detected unkown NeFS version {intro.NefsVersion}. Treating as 2.0.");
				header = await ReadHeaderVersion20Async(stream, primaryOffset, secondaryOffset, intro, p);
			}
		}

		await ValidateSplitHeaderHashAsync(stream, primaryOffset, primarySize, secondaryOffset, secondarySize, intro);
		return header;
	}

	internal async Task<bool> ValidateMagicNumberAsync(Stream stream, long offset, NefsProgress p)
	{
		// Read magic number (first four bytes)
		stream.Seek(offset, SeekOrigin.Begin);
		var magicNum = new UInt32Type(0);
		await magicNum.ReadAsync(stream, offset, p);
		return magicNum.Value == NefsHeaderIntro.NefsMagicNumber;
	}

	private async Task ValidateEncryptedHeaderAsync(Stream stream, long offset, NefsHeaderIntro intro)
	{
		var hash = await HashHelper.HashEncryptedHeaderAsync(stream, offset, (int)intro.HeaderSize);
		if (hash != intro.ExpectedHash)
		{
			Log.LogWarning("Header hash does not match expected value.");
		}
	}

	private async Task ValidateHeaderHashAsync(Stream stream, long offset, NefsHeaderIntro intro)
	{
		var hash = await HashHelper.HashStandardHeaderAsync(stream, offset, (int)intro.HeaderSize);
		if (hash != intro.ExpectedHash)
		{
			Log.LogWarning("Header hash does not match expected value.");
		}
	}

	private bool ValidateHeaderPartStream(Stream stream, long offset, int size, string part)
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

	private async Task ValidateSplitHeaderHashAsync(Stream stream, long primaryOffset, int? primarySize, long secondaryOffset, int? secondarySize, NefsHeaderIntro intro)
	{
		if (!primarySize.HasValue)
		{
			Log.LogWarning("Split-header primary size not specified, cannot validate hash.");
			return;
		}

		if (!secondarySize.HasValue)
		{
			Log.LogWarning("Split-header secondary size not specified, cannot validate hash.");
			return;
		}

		var hash = await HashHelper.HashSplitHeaderAsync(stream, primaryOffset, primarySize.Value, secondaryOffset, secondarySize.Value);
		if (hash != intro.ExpectedHash)
		{
			Log.LogWarning("Header hash does not match expected value.");
		}
	}
}
