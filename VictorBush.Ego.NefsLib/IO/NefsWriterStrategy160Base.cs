// See LICENSE.txt for license information.

using System.Security.Cryptography;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.IO;

internal abstract class NefsWriterStrategy160Base<T> : NefsWriterStrategy<T>
	where T : INefsHeader
{
	/// <summary>
	/// Calculates the header hash.
	/// </summary>
	/// <param name="writer">The writer with the stream containing the header.</param>
	/// <param name="primaryOffset">The offset to the header in the stream.</param>
	/// <param name="tocSize">The header size in bytes.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The header hash.</returns>
	protected static async Task<Sha256Hash> ComputeHashAsync(
		EndianBinaryWriter writer,
		long primaryOffset,
		uint tocSize,
		NefsProgress p)
	{
		// The hash is of the entire header except for the expected hash
		var secondOffset = primaryOffset + 0x24;
		var headerSize = Convert.ToInt32(tocSize);

		// Seek to beginning of header
		var stream = writer.BaseStream;
		stream.Seek(primaryOffset, SeekOrigin.Begin);

		// Read magic num
		var dataToHash = new byte[headerSize - 0x20];
		await stream.ReadExactlyAsync(dataToHash, 0, 4).ConfigureAwait(false);

		// Skip expected hash and read rest of header
		stream.Seek(secondOffset, SeekOrigin.Begin);
		await stream.ReadExactlyAsync(dataToHash, 4, headerSize - 0x24).ConfigureAwait(false);

		// Compute the new expected hash
		return new Sha256Hash(SHA256.HashData(dataToHash));
	}
}
