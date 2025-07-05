// See LICENSE.txt for license information.

using System.Text;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

internal abstract class NefsReaderStrategy
{
	private static readonly ILogger Log = NefsLog.GetLogger();
	private static readonly Dictionary<NefsVersion, NefsReaderStrategy> Strategies = new();

	/// <summary>
	/// Gets the version supported by the reader.
	/// </summary>
	protected abstract NefsVersion Version { get; }

	public static NefsReaderStrategy Get(NefsVersion version)
	{
		if (Strategies.TryGetValue(version, out var strategy))
		{
			return strategy;
		}

		strategy = version switch
		{
			NefsVersion.Version010 => new NefsReaderStrategy010(),
			NefsVersion.Version020 => new NefsReaderStrategy020(),
			NefsVersion.Version130 => new NefsReaderStrategy130(),
			NefsVersion.Version140 => new NefsReaderStrategy140(),
			NefsVersion.Version150 => new NefsReaderStrategy150(),
			NefsVersion.Version151 => new NefsReaderStrategy151(),
			NefsVersion.Version160 => new NefsReaderStrategy160(),
			NefsVersion.Version200 => new NefsReaderStrategy200(),
			_ => throw new NotImplementedException(
				$"Support for {version.ToPrettyString()} is not implemented.")
		};

		Strategies.Add(version, strategy);
		return strategy;
	}

	public abstract Task<(AesKeyHexBuffer, uint, uint)> GetAesKeyHeaderSizeAndOffset(EndianBinaryReader reader, long offset,
		CancellationToken token = default);

	public abstract Task<INefsHeader> ReadHeaderAsync(
		EndianBinaryReader reader,
		long primaryOffset,
		NefsWriterSettings detectedSettings,
		NefsProgress p);

	public abstract Task<INefsHeader> ReadHeaderAsync(
		EndianBinaryReader reader,
		long primaryOffset,
		int? primarySize,
		long secondaryOffset,
		int? secondarySize,
		NefsProgress p);

	/// <summary>
	/// Reads header part 3 from an input stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <param name="count">The number of entries, or -1 if unknown.</param>
	/// <returns>The loaded header part.</returns>
	internal static async Task<NefsHeaderNameTable> ReadHeaderPart3Async(Stream stream, long offset, int size,
		NefsProgress p, int count = -1)
	{
		var entries = new List<string>();

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, "3"))
		{
			return new NefsHeaderNameTable(entries);
		}

		// Read in header part 3
		var bytes = new byte[size];
		stream.Seek(offset, SeekOrigin.Begin);
		await stream.ReadExactlyAsync(bytes, p.CancellationToken);

		// Process all strings in the strings table
		var nextOffset = 0;
		while (nextOffset < size)
		{
			var currentOffset = nextOffset;

			// Find the next null terminator
			var nullOffset = size;
			for (var i = nextOffset; i < size; ++i)
			{
				if (bytes[i] == 0)
				{
					nullOffset = i;
					break;
				}
			}

			// Task weight is the size of the string being processed / size of part 3
			var strSize = nextOffset - currentOffset;
			using var _ = p.BeginTask((float)strSize / size);

			if (nullOffset == size)
			{
				// No null terminator found, assume end of part 3. There can be a few garbage bytes at the end of
				// this part.
				break;
			}

			// Get the string
			var str = Encoding.ASCII.GetString(bytes, nextOffset, nullOffset - nextOffset);

			// Record entry
			entries.Add(str);
			if (entries.Count == count)
			{
				break;
			}

			// Find next string
			nextOffset = nullOffset + 1;
		}

		return new NefsHeaderNameTable(entries);
	}

	/// <summary>
	/// Reads the data from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the data from the beginning of the stream.</param>
	/// <param name="token">The cancellation token.</param>
	/// <returns>The loaded data.</returns>
	protected static async Task<T> ReadTocDataAsync<T>(
		EndianBinaryReader reader,
		long offset,
		CancellationToken token = default)
		where T : unmanaged, INefsTocData<T>
	{
		reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		var header = await reader.ReadTocDataAsync<T>(token).ConfigureAwait(false);
		return header;
	}

	/// <summary>
	/// Reads the table from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the table from the beginning of the stream.</param>
	/// <param name="size">The size of the table.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded table.</returns>
	internal static async ValueTask<T> ReadTocTableAsync<T, TData>(EndianBinaryReader reader, long offset,
		int size, NefsProgress p)
		where T : INefsTocTable<T, TData>
		where TData :  unmanaged, INefsTocData<TData>
	{
		var entries = await ReadTocEntriesAsync<TData>(reader, offset, size, p).ConfigureAwait(false);
		return T.Create(entries);
	}

	/// <summary>
	/// Reads the table from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the table from the beginning of the stream.</param>
	/// <param name="size">The size of the table.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded table.</returns>
	internal static async ValueTask<T> ReadTocTableZeroStopAsync<T, TData>(EndianBinaryReader reader, long offset,
		int size, NefsProgress p)
		where T : INefsTocTable<T, TData>
		where TData :  unmanaged, INefsTocData<TData>
	{
		var entries = await ReadTocEntriesZeroStopAsync<TData>(reader, offset, size, p).ConfigureAwait(false);
		return T.Create(entries);
	}

	/// <summary>
	/// Reads ToC entries from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The ToC entries.</returns>
	protected static async ValueTask<T[]> ReadTocEntriesAsync<T>(EndianBinaryReader reader, long offset, int size,
		NefsProgress p)
		where T : unmanaged, INefsTocData<T>
	{
		var stream = reader.BaseStream;

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, typeof(T).Name))
		{
			return [];
		}

		// Get entries
		stream.Seek(offset, SeekOrigin.Begin);
		var numEntries = size / T.ByteCount;
		var entries = new T[numEntries];
		for (var i = 0; i < numEntries; ++i)
		{
			using (p.BeginTask(1.0f / numEntries))
			{
				entries[i] = await reader.ReadTocDataAsync<T>(p.CancellationToken).ConfigureAwait(false);
			}
		}

		return entries;
	}

	/// <summary>
	/// Reads ToC entries from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The ToC entries.</returns>
	protected static async ValueTask<List<T>> ReadTocEntriesZeroStopAsync<T>(EndianBinaryReader reader, long offset,
		int size, NefsProgress p)
		where T : unmanaged, INefsTocData<T>
	{
		var stream = reader.BaseStream;

		// Validate inputs
		if (!ValidateHeaderPartStream(stream, offset, size, typeof(T).Name))
		{
			return [];
		}

		// Get entries
		stream.Seek(offset, SeekOrigin.Begin);
		var zeroEntry = default(T);
		var numEntries = size / T.ByteCount;
		var entries = new List<T>(numEntries);
		while (true)
		{
			using var _ = p.BeginTask(1.0f / numEntries);
			var entry = await reader.ReadTocDataAsync<T>(p.CancellationToken).ConfigureAwait(false);
			if (entry.Equals(zeroEntry))
			{
				break;
			}

			entries.Add(entry);
		}

		return entries;
	}

	private static bool ValidateHeaderPartStream(Stream stream, long offset, int size, string part)
	{
		if (size == 0)
		{
			Log.LogWarning($"Header part {part} has a size of 0.");
			return false;
		}

		if (offset >= stream.Length)
		{
			Log.LogError($"Header part {part} has an offset outside the bounds of the input stream.");
			return false;
		}

		if (offset + size > stream.Length)
		{
			Log.LogError($"Header part {part} has size outside the bounds of the input stream.");
			return false;
		}

		return true;
	}
}
