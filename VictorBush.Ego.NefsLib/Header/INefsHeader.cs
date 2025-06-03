// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// A NeFS archive header.
/// </summary>
public interface INefsHeader
{
	/// <summary>
	/// Gets the header version.
	/// </summary>
	NefsVersion Version { get; }

	/// <summary>
	/// Gets whether the header is from a little-endian system.
	/// </summary>
	bool IsLittleEndian { get; }

	/// <summary>
	/// Gets whether the header is encrypted.
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
	/// Gets the volumes described by the header.
	/// </summary>
	IReadOnlyList<VolumeInfo> Volumes { get; }

	/// <summary>
	/// Gets the file name at the given offset.
	/// </summary>
	/// <param name="nameOffset">The name offset.</param>
	/// <returns>The file name.</returns>
	string GetFileName(uint nameOffset);
}
