// See LICENSE.txt for license information.

using System.Buffers;

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// Extension methods for <see cref="Stream"/>.
/// </summary>
public static class StreamExtensions
{
	/// <summary>
	/// The default value used Stream.CopyToAsync().
	/// </summary>
	public const int CopyBufferSize = 81920;

	/// <summary>
	/// Copies part of a stream to a destination stream.
	/// </summary>
	/// <param name="stream">The input stream to copy from.</param>
	/// <param name="destination">The destination stream to write to.</param>
	/// <param name="length">The number of bytes to copy.</param>
	/// <param name="cancelToken">A cancellation token.</param>
	/// <returns>An async task.</returns>
	public static async Task CopyPartialAsync(
		this Stream stream,
		Stream destination,
		long length,
		CancellationToken cancelToken)
	{
		// Use a temporary buffer to transfer chunks of data at a time
		var buffer = ArrayPool<byte>.Shared.Rent(CopyBufferSize);
		var bytesRemaining = length;

		try
		{
			while (bytesRemaining > 0)
			{
				// Read from input stream
				var bytesToRead = Math.Min(bytesRemaining, CopyBufferSize);
				var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, (int)bytesToRead), cancelToken);
				if (bytesRead == 0)
				{
					throw new EndOfStreamException();
				}

				// Copy to destination
				await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancelToken);

				bytesRemaining -= bytesRead;
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	/// <summary>
	/// Copies data from a stream to another stream.
	/// </summary>
	/// <param name="stream">The input stream to copy from.</param>
	/// <param name="destination">The destination stream to write to.</param>
	/// <param name="cancelToken">A cancellation token.</param>
	/// <returns>An async task.</returns>
	public static async Task CopyToAsync(
		this Stream stream,
		Stream destination,
		CancellationToken cancelToken)
	{
		await stream.CopyToAsync(destination, CopyBufferSize, cancelToken);
	}

	/// <summary>
	/// Asynchronously reads bytes from the current stream, advances the position within the stream until the <paramref name="buffer"/> is filled,
	/// and monitors cancellation requests.
	/// </summary>
	/// <param name="stream">The input stream to copy from.</param>
	/// <param name="buffer">The buffer to write the data into.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <returns>A task that represents the asynchronous read operation.</returns>
	public static async ValueTask ReadExactlyAsync(this Stream stream, Memory<byte> buffer,
		CancellationToken cancellationToken = default)
	{
		var totalRead = 0;
		while (totalRead < buffer.Length)
		{
			var read = await stream.ReadAsync(buffer[totalRead..], cancellationToken).ConfigureAwait(false);
			if (read == 0)
			{
				throw new EndOfStreamException();
			}

			totalRead += read;
		}
	}
}
