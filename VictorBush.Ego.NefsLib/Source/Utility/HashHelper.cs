// See LICENSE.txt for license information.

using System.Security.Cryptography;
using System.Text;
using VictorBush.Ego.NefsLib.Source.Utility;

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// Hashing utilities.
/// </summary>
internal static class HashHelper
{
	/// <summary>
	/// Generate a list of hashes from a stream.
	/// </summary>
	/// <param name="stream">The stream to hash data from.</param>
	/// <param name="offset">The offset from the beginning of the stream to start hashing at.</param>
	/// <param name="hashBlockSize">The size of each block of data to hash.</param>
	/// <param name="numHashes">The number of blocks to hash.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A list of hashes.</returns>
	public static async Task<IReadOnlyList<Sha256Hash>> HashDataFileBlocksAsync(Stream stream, long offset, int hashBlockSize, int numHashes, CancellationToken cancellationToken = default)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		var hashes = new List<Sha256Hash>();
		for (var i = 0; i < numHashes; ++i)
		{
			var hashBuilder = new Sha256HashBuilder();
			await hashBuilder.AddDataAsync(stream, hashBlockSize, cancellationToken);
			hashes.Add(hashBuilder.FinishHash());
		}

		return hashes;
	}

	/// <summary>
	/// Computes the hash of an encrpyted header.
	/// </summary>
	/// <param name="stream">The stream containing the header data.</param>
	/// <param name="headerOffset">The offset to the header from the beginning of the stream.</param>
	/// <param name="headerSize">This size of the header.</param>
	/// <returns>The computed hash.</returns>
	public static async Task<Sha256Hash> HashEncryptedHeaderAsync(Stream stream, long headerOffset, int headerSize)
	{
		var hashBuilder = new Sha256HashBuilder();
		await hashBuilder.AddDataAsync(stream, headerOffset, 4);
		await hashBuilder.AddDataAsync(stream, headerOffset + 0x24, 0x5A);
		await hashBuilder.AddDataAsync(stream, headerOffset + 0x80, headerSize - 0x80);
		return hashBuilder.FinishHash();
	}

	/// <summary>
	/// Computes the hash of a split header.
	/// </summary>
	/// <param name="stream">The stream containing the header data.</param>
	/// <param name="primaryOffset">
	/// The offset to the beginning of the primary header data. This contains the intro, toc, parts 1-5, and part 8.
	/// </param>
	/// <param name="primarySize">The size of primary header data block.</param>
	/// <param name="secondaryOffset">
	/// The offset to the beginning of the secondary header data. This contains parts 6 and 7. This is different from
	/// the secondary base offset (although their values may be the same).
	/// </param>
	/// <param name="secondarySize">The size of the secondary header data block.</param>
	/// <returns>The computed hash.</returns>
	public static async Task<Sha256Hash> HashSplitHeaderAsync(Stream stream, long primaryOffset, int primarySize, long secondaryOffset, int secondarySize)
	{
		var hashBuilder = new Sha256HashBuilder();
		await hashBuilder.AddDataAsync(stream, primaryOffset, 4);
		await hashBuilder.AddDataAsync(stream, primaryOffset + 0x24, primarySize - 0x24);
		await hashBuilder.AddDataAsync(stream, secondaryOffset, secondarySize);
		return hashBuilder.FinishHash();
	}

	/// <summary>
	/// Computes the hash of a standard header.
	/// </summary>
	/// <param name="stream">The stream containing the header data.</param>
	/// <param name="headerOffset">The offset to the header from the beginning of the stream.</param>
	/// <param name="headerSize">This size of the header.</param>
	/// <returns>The computed hash.</returns>
	public static async Task<Sha256Hash> HashStandardHeaderAsync(Stream stream, long headerOffset, int headerSize)
	{
		var hashBuilder = new Sha256HashBuilder();
		await hashBuilder.AddDataAsync(stream, headerOffset, 4);
		await hashBuilder.AddDataAsync(stream, headerOffset + 0x24, headerSize - 0x24);
		return hashBuilder.FinishHash();
	}

	/// <summary>
	/// Hashes a string and returns the resulting hash as a string.
	/// </summary>
	/// <param name="stringToHash">The string to hash.</param>
	/// <returns>The hashed string.</returns>
	public static string HashStringMD5(string stringToHash)
	{
		var hash = MD5.Create();
		var bytesToHash = Encoding.ASCII.GetBytes(stringToHash);

		var hashBytes = hash.ComputeHash(bytesToHash);
		var hashStringBuilder = new StringBuilder();

		foreach (var b in hashBytes)
		{
			hashStringBuilder.Append(b.ToString("x2"));
		}

		return hashStringBuilder.ToString();
	}
}
