// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Source.Utility;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// A NeFS archive header.
/// </summary>
public interface INefsHeader
{
	/// <summary>
	/// Gets a value indicating whether the header is encrypted.
	/// </summary>
	bool IsEncrypted { get; }

	/// <summary>
	/// Gets the AES key.
	/// </summary>
	byte[] AesKey { get; }

	/// <summary>
	/// Gets the header hash.
	/// </summary>
	Sha256Hash Hash { get; }

	/// <summary>
	/// Gets the header size in bytes.
	/// </summary>
	uint Size { get; }

	/// <summary>
	/// Gets the data block size.
	/// </summary>
	uint BlockSize { get; }

	/// <summary>
	/// Gets the number of entries.
	/// </summary>
	uint NumEntries { get; }

	/// <summary>
	/// Gets the file name at the given offset.
	/// </summary>
	/// <param name="nameOffset">The name offset.</param>
	/// <returns>The file name.</returns>
	string GetFileName(uint nameOffset);
}
