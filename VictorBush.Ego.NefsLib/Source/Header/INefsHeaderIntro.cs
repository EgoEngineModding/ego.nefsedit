// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header introduction. Contains size, encryption, and verification info.
/// </summary>
public interface INefsHeaderIntro
{
	/// <summary>
	/// File magic number; "NeFS" or 0x5346654E.
	/// </summary>
	uint MagicNumber { get; init; }

	/// <summary>
	/// Size of header in bytes.
	/// </summary>
	uint HeaderSize { get; init; }

	/// <summary>
	/// The NeFS format version.
	/// </summary>
	uint NefsVersion { get; init; }

	/// <summary>
	/// The number of items in the archive.
	/// </summary>
	uint NumberOfItems { get; init; }

	/// <summary>
	/// 256-bit AES key stored as a hex string.
	/// </summary>
	byte[] AesKeyHexString { get; init; }

	/// <summary>
	/// Gets the AES-256 key for this header.
	/// </summary>
	/// <returns>A byte array with the AES key.</returns>
	byte[] GetAesKey();
}
