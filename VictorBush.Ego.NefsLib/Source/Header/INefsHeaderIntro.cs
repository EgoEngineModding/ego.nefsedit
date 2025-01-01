// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header introduction. Contains size, encryption, and verification info.
/// </summary>
public interface INefsHeaderIntro
{
	/// <summary>
	/// Whether the file is in little endian format; otherwise, big endian.
	/// </summary>
	bool IsLittleEndian { get; }

	/// <summary>
	/// Whether the header is encrypted.
	/// </summary>
	bool IsEncrypted { get; }

	/// <summary>
	/// Whether the header is xor encoded.
	/// </summary>
	bool IsXorEncoded { get; }

	/// <summary>
	/// File magic number; "NeFS" or 0x5346654E.
	/// </summary>
	uint MagicNumber { get; }

	/// <summary>
	/// Size of header in bytes.
	/// </summary>
	uint HeaderSize { get; }

	/// <summary>
	/// The NeFS format version.
	/// </summary>
	uint NefsVersion { get; }

	/// <summary>
	/// The number of items in the archive.
	/// </summary>
	uint NumberOfItems { get; }

	/// <summary>
	/// 256-bit AES key stored as a hex string.
	/// </summary>
	ReadOnlySpan<byte> AesKeyHexString { get; }

	/// <summary>
	/// Gets the AES-256 key for this header.
	/// </summary>
	/// <returns>A byte array with the AES key.</returns>
	byte[] GetAesKey();
}
