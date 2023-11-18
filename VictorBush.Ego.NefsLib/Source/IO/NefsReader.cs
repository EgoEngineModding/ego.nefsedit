// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version151;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;
using static VictorBush.Ego.NefsLib.IO.NefsRsaKeys;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VictorBush.Ego.NefsLib.Tests")]

namespace VictorBush.Ego.NefsLib.IO;

/// <summary>
/// Reads NeFS archives.
/// </summary>
public class NefsReader : INefsReader
{
	/// <summary>
	/// The default RSA exponent for encrypted headers.
	/// </summary>
	public static readonly byte[] DefaultRsaExponent = { 01, 00, 01, 00 };

	private static readonly ILogger Log = NefsLog.GetLogger();

	private readonly IReadOnlyList<byte[]> rsaKeys;

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsReader"/> class.
	/// </summary>
	/// <param name="fileSystem">The file system used by the factory.</param>
	public NefsReader(IFileSystem fileSystem)
	{
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		RsaExponent = DefaultRsaExponent;
		this.rsaKeys = new[]
		{
			DefaultPublicKey, Dirt2PublicKey, F12011PublicKey, Dirt3PublicKey, GridAutosportPublicKey, Grid2PublicKey,
			DirtShowdownPublicKey, DirtPublicKey
		};
	}

	private IFileSystem FileSystem { get; }

	private byte[] RsaExponent { get; }

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
	/// Decodes the intro header for file versions 1.5.1.
	/// </summary>
	/// <param name="stream">The stream containing the header.</param>
	/// <param name="offset">The offset to the header from the beginning of the stream.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The decoded header data.</returns>
	internal static async Task<byte[]> DecodeXorIntroAsync(Stream stream, long offset,
		CancellationToken cancellationToken)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		var buf = new byte[NefsHeaderIntro.Size];
		await stream.ReadExactlyAsync(buf, cancellationToken).ConfigureAwait(false);
		static void Decode(byte[] buffer)
		{
			var uintBuf = MemoryMarshal.Cast<byte, uint>(buffer.AsSpan());

			uintBuf[14] ^= uintBuf[5];
			uintBuf[5] ^= uintBuf[2];
			uintBuf[2] ^= uintBuf[4];
			uintBuf[4] ^= uintBuf[7];
			uintBuf[7] ^= uintBuf[3];
			uintBuf[3] ^= uintBuf[9];
			uintBuf[9] ^= uintBuf[10];
			uintBuf[10] ^= uintBuf[1];
			uintBuf[1] ^= uintBuf[13];
			uintBuf[13] ^= uintBuf[11];
			uintBuf[11] ^= uintBuf[0];
			uintBuf[0] ^= uintBuf[12];
			uintBuf[12] ^= uintBuf[6];
			uintBuf[6] ^= uintBuf[8];
			uintBuf[8] ^= uintBuf[14];

			var mod = uintBuf[14];
			for (var i = 15; i < 31; ++i)
			{
				uintBuf[i] ^= mod;
			}

			// Debug: for copying to hex editor
			//var tmp = Convert.ToHexString(buffer);
		}

		Decode(buf);
		return buf;
	}

	/// <summary>
	/// Decrypts an encrypted header into the given stream.
	/// </summary>
	/// <param name="stream">The stream containing the encrypted header.</param>
	/// <param name="offset">The offset to the header from the beginning of the stream.</param>
	/// <param name="outStream">The decrypted output stream.</param>
	/// <param name="rsaPublicKey">The RSA public key to use for decryption</param>
	/// <param name="p">Progress info.</param>
	internal async Task DecryptHeaderIntroAsync(
		Stream stream,
		long offset,
		Stream outStream,
		byte[] rsaPublicKey,
		NefsProgress p)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		// Encrypted headers:
		// - Headers are "encrypted" in a two-step process. RSA-1024. No padding is used.
		// - First 0x80 bytes are signed with an RSA private key (data -> decrypt -> scrambled data).
		// - Must use an RSA 1024-bit public key to unscramble the data (scrambled data -> encrypt -> data).
		// - For DiRT Rally 2 this public key is stored in the main executable.
		// Add extra 0 byte at the end of encrypted header for the sake of making the MSB 0 and therefore non-negative
		// when converting to BigInteger
		var encryptedHeader = new byte[NefsHeaderIntro.Size + 1];
		await stream.ReadExactlyAsync(encryptedHeader.AsMemory(0, NefsHeaderIntro.Size), p.CancellationToken);

		// Use big integers instead of RSA since the c# implementation forces the use of padding.
		var n = new BigInteger(rsaPublicKey);
		var e = new BigInteger(RsaExponent);
		var m = new BigInteger(encryptedHeader);

		// Decrypt the header intro
		var decrypted = BigInteger.ModPow(m, e, n).ToByteArray();
		outStream.Write(decrypted, 0, decrypted.Length);

		// Fill any leftover space with zeros
		if (decrypted.Length != NefsHeaderIntro.Size)
		{
			for (var i = 0; i < (NefsHeaderIntro.Size - decrypted.Length); i++)
			{
				outStream.WriteByte(0);
			}
		}
	}

	internal readonly record struct DecryptHeaderIntroResult(bool Succeeded, bool IsEncrypted = false,
		bool IsXorEncoded = false);

	internal async Task<DecryptHeaderIntroResult> ReadHeaderIntroAsync(Stream stream, long offset,
		Stream outDecryptStream,
		bool encrypted, NefsProgress p)
	{
		var isXorEncoded = false;
		byte[]? decodedData = null;

		var validMagicNum = await ValidateMagicNumberAsync(stream, offset, p);
		if (!validMagicNum)
		{
			// Check for v1.5.1 xor encoding
			validMagicNum = await ValidateXorMagicNumberAsync(stream, offset, p);
			if (validMagicNum)
			{
				isXorEncoded = true;
				decodedData = await DecodeXorIntroAsync(stream, offset, p.CancellationToken).ConfigureAwait(false);
			}
			else if (!encrypted)
			{
				Log.LogInformation("Header magic number mismatch, assuming it's an encrypted archive.");
				using var decryptStream = new MemoryStream();
				for (var i = 0; i < this.rsaKeys.Count; ++i)
				{
					decryptStream.SetLength(0);
					var key = this.rsaKeys[i];
					await DecryptHeaderIntroAsync(stream, offset, decryptStream, key, p);

					var result = await ReadHeaderIntroAsync(decryptStream, 0, outDecryptStream, true, p);
					if (!result.Succeeded)
					{
						continue;
					}

					return result;
				}

				Log.LogError("Failed to decrypt header.");
				return new DecryptHeaderIntroResult(false, encrypted);
			}
			else
			{
				return new DecryptHeaderIntroResult(false, encrypted);
			}
		}

		if (isXorEncoded)
		{
			await outDecryptStream.WriteAsync(decodedData, p.CancellationToken);
		}
		else if (encrypted)
		{
			stream.Seek(offset, SeekOrigin.Begin);
			await stream.CopyToAsync(outDecryptStream, p.CancellationToken);
		}

		return new DecryptHeaderIntroResult(true, encrypted, isXorEncoded);
	}

	internal async Task<INefsHeaderIntro> ReadHeaderIntroAsync(Stream stream, long offset, Stream outDecryptStream,
		NefsProgress p)
	{
		DecryptHeaderIntroResult readResult;
		var outStreamOffset = outDecryptStream.Position;

		using (p.BeginTask(0.2f, "Reading header intro"))
		{
			readResult = await ReadHeaderIntroAsync(stream, offset, outDecryptStream, false, p);
			if (!readResult.Succeeded)
			{
				throw new InvalidDataException("Header magic number mismatch.");
			}
		}

		INefsHeaderIntro intro;
		var introStream = readResult.IsEncrypted || readResult.IsXorEncoded ? outDecryptStream : stream;
		var introOffset = readResult.IsEncrypted || readResult.IsXorEncoded ? outStreamOffset : offset;

		using (p.BeginTask(0.8f, "Reading header intro"))
		{
			var oldVersion = await ReadVersionV151Async(introStream, introOffset, p);
			if (oldVersion is (uint)NefsVersion.Version140)
			{
				throw new NotImplementedException("Support for version 1.4.0 is not implemented.");
			}

			if (oldVersion is (uint)NefsVersion.Version151)
			{
				// this must be version < 1.6.0 (so far known to be 1.5.1)
				intro = await ReadHeaderIntroV151Async(introStream, introOffset, readResult, p);
			}
			else
			{
				intro = await ReadHeaderIntroV16Async(introStream, introOffset, readResult, p);
			}
		}

		if (readResult.IsEncrypted)
		{
			// The rest of the header is encrypted using AES-256, decrypt using the key from the header intro
			var key = intro.GetAesKey();

			// Decrypt the rest of the header
			using var aes = Aes.Create();

			aes.KeySize = 256;
			aes.Key = key;
			aes.Mode = CipherMode.ECB;
			aes.BlockSize = 128;
			aes.Padding = PaddingMode.Zeros;

			if (intro.NefsVersion == (uint)NefsVersion.Version151)
			{
				// This version may have a header of 126 bytes, and 2 were added for the sake of RSA encryption
				// we'll back up here to overwrite those last 2 bytes with the bytes contained in next AES section
				// Side-note: last two bytes could be padding and not real data
				introStream.Seek(-2, SeekOrigin.End);
			}
			else
			{
				introStream.Seek(0, SeekOrigin.End);
			}

			// Decrypt the data - make sure to leave open the base stream
			var decryptor = aes.CreateDecryptor();
			using (var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read, true))
			{
				// Decrypt data from input stream and copy to the decrypted stream
				var headerLeftoverSize = intro.HeaderSize - NefsHeaderIntro.Size;
				await cryptoStream.CopyPartialAsync(introStream, headerLeftoverSize, p.CancellationToken);
			}

			if (intro.NefsVersion is (uint)NefsVersion.Version151)
			{
				// Fix last two bytes being part of AES data
				var hiPart = new UInt16Type(NefsHeaderIntro.Size - 2);
				await hiPart.ReadAsync(introStream, introOffset, p);
				var intro151 = (Nefs151HeaderIntro)intro;
				intro = intro151 with { Unknown0x7C = intro151.Unknown0x7C | ((uint)hiPart.Value << 16) };
			}

			// Debug: for copying to hex editor
			var tmp = Convert.ToHexString(((MemoryStream)introStream).ToArray());
		}

		return intro;
	}

	internal async Task<INefsHeader> ReadHeaderAsync(Stream stream, long offset, NefsProgress p)
	{
		INefsHeader header;
		INefsHeaderIntro intro;
		using var decryptStream = new MemoryStream();

		using (p.BeginTask(0.2f, "Reading header intro"))
		{
			intro = await ReadHeaderIntroAsync(stream, offset, decryptStream, p);
		}

		var headerStream = intro.IsEncrypted ? decryptStream : stream;
		offset = intro.IsEncrypted ? 0 : offset;
		using (p.BeginTask(0.8f, "Reading header"))
		{
			if (intro.NefsVersion == (uint)NefsVersion.Version200)
			{
				// 2.0.0
				Log.LogInformation("Detected NeFS version 2.0.");
				header = await ReadHeaderV20Async(headerStream, offset, offset, (NefsHeaderIntro)intro, p);
				if (intro.IsEncrypted)
				{
					await ValidateEncryptedHeaderAsync(headerStream, offset, (NefsHeaderIntro)intro);
				}
				else
				{
					await ValidateHeaderHashAsync(headerStream, offset, (NefsHeaderIntro)intro);
				}
			}
			else if (intro.NefsVersion == (uint)NefsVersion.Version160)
			{
				// 1.6.0
				Log.LogInformation("Detected NeFS version 1.6.");
				header = await ReadHeaderV16Async(headerStream, offset, offset, (NefsHeaderIntro)intro, p);
				if (intro.IsEncrypted)
				{
					await ValidateEncryptedHeaderAsync(headerStream, offset, (NefsHeaderIntro)intro);
				}
				else
				{
					await ValidateHeaderHashAsync(headerStream, offset, (NefsHeaderIntro)intro);
				}
			}
			else if (intro.NefsVersion is (uint)NefsVersion.Version151)
			{
				// 1.5.1
				Log.LogInformation("Detected NeFS version 1.5.1.");
				header = await ReadHeaderV151Async(headerStream, offset, (Nefs151HeaderIntro)intro, p);
			}
			else
			{
				Log.LogError($"Detected unknown NeFS version {intro.NefsVersion}. Treating as 2.0.");
				header = await ReadHeaderV20Async(headerStream, offset, offset, (NefsHeaderIntro)intro, p);
				if (intro.IsEncrypted)
				{
					await ValidateEncryptedHeaderAsync(headerStream, offset, (NefsHeaderIntro)intro);
				}
				else
				{
					await ValidateHeaderHashAsync(headerStream, offset, (NefsHeaderIntro)intro);
				}
			}
		}

		return header;
	}

	/// <summary>
	/// Reads the 1.5.1 header from an input stream.
	/// </summary>
	/// <summary>
	/// Reads the header intro from an input stream. This is for non-encrypted headers only.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header intro from the beginning of the stream.</param>
	/// <param name="decryptResult">Decryption state.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header intro.</returns>
	internal static async Task<Nefs151HeaderIntro> ReadHeaderIntroV151Async(
		Stream stream,
		long offset,
		DecryptHeaderIntroResult decryptResult,
		NefsProgress p)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		var intro = new Nefs151HeaderIntro
			{ IsEncrypted = decryptResult.IsEncrypted, IsXorEncoded = decryptResult.IsXorEncoded };
		await FileData.ReadDataAsync(stream, offset, intro, p);
		return intro;
	}

	/// <summary>
	/// Reads the header from an input stream.
	/// </summary>
	/// <summary>
	/// Reads the header intro from an input stream. This is for non-encrypted headers only.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header intro from the beginning of the stream.</param>
	/// <param name="decryptResult">Decryption state.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header intro.</returns>
	internal static async Task<NefsHeaderIntro> ReadHeaderIntroV16Async(
		Stream stream,
		long offset,
		DecryptHeaderIntroResult decryptResult,
		NefsProgress p)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		var intro = new NefsHeaderIntro
			{ IsEncrypted = decryptResult.IsEncrypted, IsXorEncoded = decryptResult.IsXorEncoded };
		await FileData.ReadDataAsync(stream, offset, intro, p);
		return intro;
	}

	/// <summary>
	/// Reads the header intro table of contents from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header intro table of contents from the beginning of the stream.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header intro offsets data.</returns>
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
	/// <returns>The loaded header intro offsets data.</returns>
	internal async Task<Nefs20HeaderIntroToc> ReadHeaderIntroTocVersion20Async(Stream stream, long offset, NefsProgress p)
	{
		var toc = new Nefs20HeaderIntroToc();
		await FileData.ReadDataAsync(stream, offset, toc, p);
		return toc;
	}

	/// <summary>
	/// Reads 1.5.1 header part 1 from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal async Task<Nefs151HeaderPart1> Read151HeaderPart1Async(Stream stream, long offset, int size,
		NefsProgress p)
	{
		var entries = new List<Nefs151HeaderPart1Entry>();

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "1"))
		{
			return new Nefs151HeaderPart1(entries);
		}

		// Get entries in part 1
		var numEntries = size / Nefs151HeaderPart1.EntrySize;
		var entryOffset = offset;

		for (var i = 0; i < numEntries; ++i)
		{
			using (p.BeginTask(1.0f / numEntries))
			{
				var guid = Guid.NewGuid();
				var entry = new Nefs151HeaderPart1Entry(guid);
				await FileData.ReadDataAsync(stream, entryOffset, entry, p);
				entryOffset += Nefs151HeaderPart1.EntrySize;
				entries.Add(entry);
			}
		}

		return new Nefs151HeaderPart1(entries);
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
	/// Reads 1.5.1 header part 2 from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal async Task<Nefs151HeaderPart2> Read151HeaderPart2Async(Stream stream, long offset, int size,
		NefsProgress p)
	{
		var entries = new List<Nefs151HeaderPart2Entry>();

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "2"))
		{
			return new Nefs151HeaderPart2(entries);
		}

		// Get entries in part 2
		var numEntries = size / Nefs151HeaderPart2.EntrySize;
		var entryOffset = offset;

		for (var i = 0; i < numEntries; ++i)
		{
			using (p.BeginTask(1.0f / numEntries))
			{
				var entry = new Nefs151HeaderPart2Entry();
				await FileData.ReadDataAsync(stream, entryOffset, entry, p);
				entryOffset += Nefs151HeaderPart2.EntrySize;

				entries.Add(entry);

				// TODO: figure out id vs id2, for now throw
				if (entry.Id != entry.Id2)
				{
					throw new NotImplementedException("Proper understanding of part 2 ids not implemented.");
				}
			}
		}

		return new Nefs151HeaderPart2(entries);
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
		await stream.ReadExactlyAsync(bytes, p.CancellationToken);

		// Process all strings in the strings table
		var nextOffset = 0;
		while (nextOffset < size)
		{
			var currentOffset = nextOffset;

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

			// Task weight is the size of the string being processed / size of part 3
			var strSize = nextOffset - currentOffset;
			using var _ = p.BeginTask((float)strSize / size);

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

		return new NefsHeaderPart3(entries);
	}

	/// <summary>
	/// Reads 1.5.1 header part 4 from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="part1">Header part 1.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal async Task<Nefs16HeaderPart4> Read151HeaderPart4Async(Stream stream, long offset, int size,
		Nefs151HeaderPart1 part1, NefsProgress p)
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

		// TODO: I believe this is padding to reach multiple of EntrySize boundary
		// Get the unknown last value at the end of part 4
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

		// TODO: I believe this is padding to reach multiple of EntrySize boundary
		// Get the unknown last value at the end of part 4
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
		if (!ValidateHeaderPartStream(stream, offset, size, "6"))
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

	internal async Task<Nefs151Header> ReadHeaderV151Async(
		Stream stream,
		long primaryOffset,
		Nefs151HeaderIntro intro,
		NefsProgress p)
	{
		Nefs151HeaderPart1 part1;
		Nefs151HeaderPart2 part2;
		NefsHeaderPart3 part3;
		Nefs16HeaderPart4 part4;
		NefsHeaderPart5 part5;

		// Calc weight of each task (5 parts)
		var weight = 1.0f / 5.0f;

		using (p.BeginTask(weight, "Reading header part 1"))
		{
			part1 = await Read151HeaderPart1Async(stream, primaryOffset + intro.OffsetToPart1, (int)intro.Part1Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 2"))
		{
			part2 = await Read151HeaderPart2Async(stream, primaryOffset + intro.OffsetToPart2, (int)intro.Part2Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 3"))
		{
			part3 = await ReadHeaderPart3Async(stream, primaryOffset + intro.OffsetToPart3, (int)intro.Part3Size, p);
		}

		using (p.BeginTask(weight, "Reading header part 4"))
		{
			part4 = await Read151HeaderPart4Async(stream, primaryOffset + intro.OffsetToPart4,
				(int)intro.Part4Size, part1, p);
		}

		using (p.BeginTask(weight, "Reading header part 5"))
		{
			part5 = await ReadHeaderPart5Async(stream, primaryOffset + intro.OffsetToPart5, NefsHeaderPart5.Size, p);
		}

		return new Nefs151Header(intro, part1, part2, part3, part4, part5);
	}

	internal async Task<Nefs16Header> ReadHeaderV16Async(
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

	internal async Task<Nefs20Header> ReadHeaderV20Async(
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
			intro = await ReadHeaderIntroV16Async(stream, primaryOffset, new DecryptHeaderIntroResult(), p);
		}

		using (p.BeginTask(0.8f, "Reading header"))
		{
			if (intro.NefsVersion == (uint)NefsVersion.Version200)
			{
				// 2.0.0
				Log.LogInformation("Detected NeFS version 2.0.");
				header = await ReadHeaderV20Async(stream, primaryOffset, secondaryOffset, intro, p);
			}
			else if (intro.NefsVersion == (uint)NefsVersion.Version160)
			{
				// 1.6.0
				Log.LogInformation("Detected NeFS version 1.6.");
				header = await ReadHeaderV16Async(stream, primaryOffset, secondaryOffset, intro, p);
			}
			else
			{
				Log.LogError($"Detected unkown NeFS version {intro.NefsVersion}. Treating as 2.0.");
				header = await ReadHeaderV20Async(stream, primaryOffset, secondaryOffset, intro, p);
			}
		}

		await ValidateSplitHeaderHashAsync(stream, primaryOffset, primarySize, secondaryOffset, secondarySize, intro);
		return header;
	}

	internal async Task<uint> ReadVersionV151Async(Stream stream, long offset, NefsProgress p)
	{
		stream.Seek(offset, SeekOrigin.Begin);
		var verNum = new UInt32Type(8);
		await verNum.ReadAsync(stream, offset, p);
		return verNum.Value;
	}

	internal async Task<bool> ValidateMagicNumberAsync(Stream stream, long offset, NefsProgress p)
	{
		// Read magic number (first four bytes)
		stream.Seek(offset, SeekOrigin.Begin);
		var magicNum = new UInt32Type(0);
		await magicNum.ReadAsync(stream, offset, p);
		return magicNum.Value == NefsHeaderIntro.NefsMagicNumber;
	}

	internal async Task<bool> ValidateXorMagicNumberAsync(Stream stream, long offset, NefsProgress p)
	{
		// Read magic number (first four bytes)
		stream.Seek(offset, SeekOrigin.Begin);
		var magicNum = new UInt32Type(0);
		await magicNum.ReadAsync(stream, offset, p);
		var modNum = new UInt32Type(48);
		await modNum.ReadAsync(stream, offset, p);
		var magic = magicNum.Value ^ modNum.Value;
		return magic == NefsHeaderIntro.NefsMagicNumber;
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
