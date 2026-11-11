// See LICENSE.txt for license information.

using System.Buffers.Binary;

namespace VictorBush.Ego.NefsLib.IO;

/// <summary>
/// Finds NeFS headers inside files that do not start with one.
/// Some games (e.g. DiRT Showdown on Xbox 360) store several NeFS archives concatenated in a single file.
/// The first archive may start at a non-zero offset, or the beginning of the file may be truncated/corrupted,
/// in which case the "normal" reader fails with a magic number mismatch.
/// </summary>
public static class NefsHeaderScanner
{
	/// <summary>
	/// The NeFS four character code: 53 46 65 4E ("SFeN").
	/// </summary>
	public static readonly byte[] Magic = [0x53, 0x46, 0x65, 0x4E];

	private const int ScanBufferSize = 4 * 1024 * 1024;

	/// <summary>
	/// Enumerates every offset in the stream where a plausible NeFS header starts.
	/// </summary>
	/// <param name="stream">Stream to scan. Must be seekable.</param>
	/// <param name="startOffset">Offset to start scanning at.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Offsets, in ascending order.</returns>
	public static IEnumerable<long> FindHeaderOffsets(Stream stream, long startOffset = 0,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(stream);
		if (!stream.CanSeek)
		{
			throw new ArgumentException("Stream must be seekable.", nameof(stream));
		}

		var buffer = new byte[ScanBufferSize + Magic.Length];
		var carry = 0;
		var position = startOffset;

		stream.Seek(startOffset, SeekOrigin.Begin);
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var read = stream.Read(buffer, carry, ScanBufferSize);
			if (read <= 0)
			{
				yield break;
			}

			var total = carry + read;
			var baseOffset = position - carry;
			var index = 0;

			while (true)
			{
				var hit = buffer.AsSpan(index, total - index).IndexOf(Magic);
				if (hit < 0)
				{
					break;
				}

				var candidate = baseOffset + index + hit;
				if (IsValidHeader(stream, candidate))
				{
					yield return candidate;
				}

				index += hit + 1;
			}

			carry = Math.Min(Magic.Length - 1, total);
			buffer.AsSpan(total - carry, carry).CopyTo(buffer);
			position += read;
		}
	}

	/// <summary>
	/// Returns the first offset at or after <paramref name="startOffset"/> that contains a plausible header.
	/// </summary>
	/// <param name="stream">Stream to scan.</param>
	/// <param name="startOffset">Offset to start scanning at.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The offset, or -1 when no header was found.</returns>
	public static long FindFirstHeaderOffset(Stream stream, long startOffset = 0,
		CancellationToken cancellationToken = default)
	{
		foreach (var offset in FindHeaderOffsets(stream, startOffset, cancellationToken))
		{
			return offset;
		}

		return -1;
	}

	/// <summary>
	/// Checks whether a candidate offset holds a NeFS header. The check looks at the version fields only,
	/// so it stays cheap enough to run over hundreds of candidates.
	/// </summary>
	/// <param name="stream">Stream to inspect.</param>
	/// <param name="offset">Candidate offset.</param>
	/// <returns>True when the offset looks like a valid header.</returns>
	public static bool IsValidHeader(Stream stream, long offset)
	{
		var buffer = new byte[128];
		var restore = stream.Position;

		try
		{
			if (offset < 0 || offset + buffer.Length > stream.Length)
			{
				return false;
			}

			stream.Seek(offset, SeekOrigin.Begin);
			if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
			{
				return false;
			}
		}
		finally
		{
			stream.Position = restore;
		}

		if (!buffer.AsSpan(0, 4).SequenceEqual(Magic))
		{
			return false;
		}

		// Version is stored at +8 (older layouts) or +104 (1.6.0+), little or big endian.
		foreach (var versionOffset in new[] { 8, 104 })
		{
			var raw = buffer.AsSpan(versionOffset, 4);
			var little = BinaryPrimitives.ReadUInt32LittleEndian(raw);
			var big = BinaryPrimitives.ReadUInt32BigEndian(raw);

			foreach (var value in new[] { little, big })
			{
				if (Enum.IsDefined(typeof(NefsVersion), (int)value))
				{
					return true;
				}
			}
		}

		return false;
	}
}
