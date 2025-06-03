// See LICENSE.txt for license information.

using System.Security.Cryptography;

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// A SHA-256 hash value.
/// </summary>
public sealed class Sha256HashBuilder
{
	private readonly List<byte> buffer = new();

	/// <summary>
	/// Adds data to be hashed.
	/// </summary>
	/// <param name="data">Bytes of data to add to the buffer to be hashed.</param>
	public void AddData(byte[] data)
	{
		this.buffer.AddRange(data);
	}

	/// <summary>
	/// Adds data to be hashed.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="absoluteOffset">The offset from the beginning of the stream to read from.</param>
	/// <param name="count">The number of bytes to read.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>An async <see cref="Task"/>.</returns>
	public async Task AddDataAsync(Stream stream, long absoluteOffset, int count, CancellationToken cancellationToken = default)
	{
		stream.Seek(absoluteOffset, SeekOrigin.Begin);
		await AddDataAsync(stream, count, cancellationToken);
	}

	/// <summary>
	/// Adds data to be hashed.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="count">The number of bytes to read.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>An async <see cref="Task"/>.</returns>
	public async Task AddDataAsync(Stream stream, int count, CancellationToken cancellationToken = default)
	{
		var data = new byte[count];
		var bytesRead = await stream.ReadAsync(data, 0, count, cancellationToken);
		if (bytesRead != count)
		{
			throw new ArgumentException($"Expected to add {count} bytes to the hash, but only read {bytesRead}.");
		}

		this.buffer.AddRange(data);
	}

	/// <summary>
	/// Computes the hash of the current data buffer.
	/// </summary>
	/// <returns>The SHA-256 hash value.</returns>
	public Sha256Hash FinishHash()
	{
		using (var hash = SHA256.Create())
		{
			var value = hash.ComputeHash(this.buffer.ToArray());
			return new Sha256Hash(value);
		}
	}
}
