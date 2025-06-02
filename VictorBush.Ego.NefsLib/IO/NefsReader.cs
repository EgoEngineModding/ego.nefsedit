// See LICENSE.txt for license information.

using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;
using static VictorBush.Ego.NefsLib.IO.NefsRsaKeys;

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
			DirtShowdownPublicKey, DirtPublicKey, OperationFlashpointPublicKey
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
			using var _ = p.BeginTask(0.9f);
			header = await ReadHeaderAsync(stream, 0, p).ConfigureAwait(false);
		}

		// Create items from header
		using var __ = p.BeginTask(0.1f);
		var itemsBuilder = NefsItemListBuilder.Create(header);
		var items = itemsBuilder.Build(filePath, p);

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
		var itemsBuilder = NefsItemListBuilder.Create(header);
		var items = itemsBuilder.Build(dataFilePath, p);

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
		var itemsBuilder = NefsItemListBuilder.Create(header);
		var items = itemsBuilder.Build(dataFilePath, p);

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

		var buf = new byte[NefsConstants.IntroSize];
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
	/// <param name="littleEndian">Whether to use little-endian mode.</param>
	/// <param name="p">Progress info.</param>
	internal async Task DecryptHeaderIntroAsync(
		Stream stream,
		long offset,
		Stream outStream,
		byte[] rsaPublicKey,
		bool littleEndian,
		NefsProgress p)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		// Encrypted headers:
		// - Headers are "encrypted" in a two-step process. RSA-1024. No padding is used.
		// - First 0x80 bytes are signed with an RSA private key (data -> decrypt -> scrambled data).
		// - Must use an RSA 1024-bit public key to unscramble the data (scrambled data -> encrypt -> data).
		// - For DiRT Rally 2 this public key is stored in the main executable.
		var encryptedHeader = new byte[NefsConstants.IntroSize];
		await stream.ReadExactlyAsync(encryptedHeader, p.CancellationToken);
		if (!littleEndian)
		{
			// The game stores big integers as uint16 blocks.
			// It properly swaps the RSA prime and exponent on big-endian systems but not the header
			// We'll emulate how the header is seen by the game here
			SwapEndian(encryptedHeader);
		}

		// Use big integers instead of RSA since the c# implementation forces the use of padding.
		var mod = new BigInteger(rsaPublicKey, true);
		var exp = new BigInteger(RsaExponent, true);
		var m = new BigInteger(encryptedHeader, true);
		var result = BigInteger.ModPow(m, exp, mod);

		// Decrypt the header intro
		var decrypted = result.ToByteArray(true);
		if (!littleEndian)
		{
			// Undo emulation of game logic to get correct data
			SwapEndian(decrypted);
		}

		// Write out the data and fill leftover space with zeros
		await outStream.WriteAsync(decrypted);
		if (decrypted.Length != NefsConstants.IntroSize)
		{
			for (var i = 0; i < (NefsConstants.IntroSize - decrypted.Length); i++)
			{
				outStream.WriteByte(0);
			}
		}

		return;

		static void SwapEndian(Span<byte> data)
		{
			var dataU16 = MemoryMarshal.Cast<byte, ushort>(data);
			for (var i = 0; i < dataU16.Length; i++)
			{
				dataU16[i] = BinaryPrimitives.ReverseEndianness(dataU16[i]);
			}
		}
	}

	internal readonly record struct DecryptHeaderIntroResult(bool Succeeded, bool IsEncrypted = false,
		bool IsXorEncoded = false, bool IsLittleEndian = false);

	internal async Task<DecryptHeaderIntroResult> ReadHeaderIntroAsync(Stream stream, long offset,
		Stream outDecryptStream,
		bool encrypted, NefsProgress p)
	{
		var isXorEncoded = false;
		byte[]? decodedData = null;

		var (validMagicNum, isLittleEndian) = await ValidateMagicNumberAsync(stream, offset, p);
		if (!validMagicNum)
		{
			// Check for v1.5.1 xor encoding
			(validMagicNum, isLittleEndian) = await ValidateXorMagicNumberAsync(stream, offset, p);
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
					var key = this.rsaKeys[i];

					decryptStream.SetLength(0);
					await DecryptHeaderIntroAsync(stream, offset, decryptStream, key, true, p);
					var result = await ReadHeaderIntroAsync(decryptStream, 0, outDecryptStream, true, p);
					if (!result.Succeeded)
					{
						// Try again in big-endian decryption mode
						decryptStream.SetLength(0);
						await DecryptHeaderIntroAsync(stream, offset, decryptStream, key, false, p);
						result = await ReadHeaderIntroAsync(decryptStream, 0, outDecryptStream, true, p);
						if (!result.Succeeded)
						{
							continue;
						}
					}

					return result;
				}

				Log.LogError("Failed to decrypt header.");
				return new DecryptHeaderIntroResult(false, encrypted, IsLittleEndian:isLittleEndian);
			}
			else
			{
				return new DecryptHeaderIntroResult(false, encrypted, IsLittleEndian:isLittleEndian);
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

		return new DecryptHeaderIntroResult(true, encrypted, isXorEncoded, isLittleEndian);
	}

	internal async Task<INefsHeader> ReadHeaderAsync(Stream stream, long offset, NefsProgress p)
	{
		DecryptHeaderIntroResult readResult;
		using var decryptStream = new MemoryStream();

		using (p.BeginTask(0.2f, "Reading header intro"))
		{
			readResult = await ReadHeaderIntroAsync(stream, offset, decryptStream, false, p);
			if (!readResult.Succeeded)
			{
				throw new InvalidDataException("Header magic number mismatch.");
			}
		}

		using (p.BeginTask(0.8f, "Reading header"))
		{
			var introStream = readResult.IsEncrypted || readResult.IsXorEncoded ? decryptStream : stream;
			var introOffset = readResult.IsEncrypted || readResult.IsXorEncoded ? 0 : offset;
			using var reader = new EndianBinaryReader(introStream, readResult.IsLittleEndian);
			var version = await ReadVersionAsync(reader, introOffset, p).ConfigureAwait(false);
			var strategy = NefsReaderStrategy.Get(version);

			if (readResult.IsEncrypted)
			{
				// The rest of the header is encrypted using AES-256, decrypt using the key from the header intro
				var (key, headerSize, aesOffset) =
					await strategy.GetAesKeyHeaderSizeAndOffset(reader, introOffset).ConfigureAwait(false);

				// Decrypt the rest of the header
				using var aes = Aes.Create();

				aes.KeySize = 256;
				aes.Key = Convert.FromHexString(Encoding.ASCII.GetString(key));
				aes.Mode = CipherMode.ECB;
				aes.BlockSize = 128;
				aes.Padding = PaddingMode.Zeros;

				// Decrypt the data - make sure to leave open the base stream
				var decryptor = aes.CreateDecryptor();
				await using var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read, true);

				// Decrypt data from input stream and copy to the decrypted stream
				var headerLeftoverSize = headerSize - NefsConstants.IntroSize;
				introStream.Seek(aesOffset, SeekOrigin.Begin);
				await cryptoStream.CopyPartialAsync(introStream, headerLeftoverSize, p.CancellationToken);

				// Debug: for copying to hex editor
				//var tmp = Convert.ToHexString(((MemoryStream)introStream).ToArray());
			}
			else if (readResult.IsXorEncoded)
			{
				// Copy the rest of the header
				var (_, headerSize, _) =
					await strategy.GetAesKeyHeaderSizeAndOffset(reader, introOffset).ConfigureAwait(false);
				var headerLeftoverSize = headerSize - NefsConstants.IntroSize;
				await stream.CopyPartialAsync(introStream, headerLeftoverSize, p.CancellationToken);
			}

			var writerSettings = new NefsWriterSettings(readResult.IsEncrypted, readResult.IsXorEncoded,
				readResult.IsLittleEndian);
			return await strategy.ReadHeaderAsync(reader, offset, writerSettings, p).ConfigureAwait(false);
		}
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
		var (validMagicNum, isLittleEndian) = await ValidateMagicNumberAsync(stream, primaryOffset, p);
		if (!validMagicNum)
		{
			throw new InvalidOperationException("Header magic number mismatch, aborting.");
		}

		using var reader = new EndianBinaryReader(stream, isLittleEndian);
		var version = await ReadVersionAsync(reader, primaryOffset, p).ConfigureAwait(false);
		var strategy = NefsReaderStrategy.Get(version);

		var header = await strategy.ReadHeaderAsync(reader, primaryOffset, primarySize, secondaryOffset, secondarySize, p)
			.ConfigureAwait(false);
		return header;
	}

	internal async ValueTask<NefsVersion> ReadVersionAsync(EndianBinaryReader reader, long offset, NefsProgress p)
	{
		// v 0.1.0+
		reader.BaseStream.Seek(offset + 8, SeekOrigin.Begin);
		var version = (NefsVersion)await reader.ReadUInt32Async(p.CancellationToken).ConfigureAwait(false);
		if (Enum.IsDefined(version))
		{
			return version;
		}

		// v 1.6.0+
		reader.BaseStream.Seek(offset + 104, SeekOrigin.Begin);
		version = (NefsVersion)await reader.ReadUInt32Async(p.CancellationToken).ConfigureAwait(false);
		return version;
	}

	private static (bool Valid, bool LittleEndian) IsMagicNumber(uint value, bool valueLittleEndian)
	{
		if (value == NefsConstants.FourCc)
		{
			return (true, valueLittleEndian);
		}

		if (BinaryPrimitives.ReverseEndianness(value) == NefsConstants.FourCc)
		{
			return (true, !valueLittleEndian);
		}

		return (false, true);
	}

	internal async Task<(bool Valid, bool LittleEndian)> ValidateMagicNumberAsync(Stream stream, long offset,
		NefsProgress p)
	{
		// Read magic number (first four bytes)
		stream.Seek(offset, SeekOrigin.Begin);
		using var reader = new EndianBinaryReader(stream);
		var magicNum = await reader.ReadUInt32Async(p.CancellationToken).ConfigureAwait(false);
		return IsMagicNumber(magicNum, reader.IsLittleEndian);
	}

	internal async Task<(bool Valid, bool LittleEndian)> ValidateXorMagicNumberAsync(Stream stream, long offset,
		NefsProgress p)
	{
		// Read magic number (first four bytes)
		stream.Seek(offset, SeekOrigin.Begin);
		using var reader = new EndianBinaryReader(stream);
		var magicNum = await reader.ReadUInt32Async(p.CancellationToken).ConfigureAwait(false);
		stream.Seek(offset + 48, SeekOrigin.Begin);
		var modNum = await reader.ReadUInt32Async(p.CancellationToken).ConfigureAwait(false);
		var magic = magicNum ^ modNum;
		return IsMagicNumber(magic, reader.IsLittleEndian);
	}
}
