// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// Header part 4.
/// </summary>
public sealed class Nefs151HeaderPart4 : INefsHeaderPart4
{
	public const int LastValueSize = 0x4;
	private static readonly ILogger Log = NefsLog.GetLogger();
	private readonly List<Nefs151HeaderPart4Entry> entriesByIndex;
	private readonly Dictionary<Guid, uint> indexLookup;

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151HeaderPart4"/> class.
	/// </summary>
	/// <param name="entries">A collection of entries to initialize this object with.</param>
	/// <param name="indexLookup">
	/// A dictionary that matches an item Guid to a part 4 index. This is used to find the correct index part 4 value
	/// for an item.
	/// </param>
	/// <param name="unkownEndValue">Last four bytes of part 4.</param>
	internal Nefs151HeaderPart4(IEnumerable<Nefs151HeaderPart4Entry> entries, Dictionary<Guid, uint> indexLookup, uint unkownEndValue)
	{
		this.entriesByIndex = new List<Nefs151HeaderPart4Entry>(entries);
		this.indexLookup = new Dictionary<Guid, uint>(indexLookup);
		UnkownEndValue = unkownEndValue;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs16HeaderPart4"/> class from a list of items.
	/// </summary>
	/// <param name="items">The items to initialize from.</param>
	/// <param name="unkownEndValue">Last four bytes of part 4.</param>
	internal Nefs151HeaderPart4(NefsItemList items, uint unkownEndValue)
	{
		this.entriesByIndex = new List<Nefs151HeaderPart4Entry>();
		this.indexLookup = new Dictionary<Guid, uint>();
		UnkownEndValue = unkownEndValue;

		var nextStartIdx = 0U;

		foreach (var item in items.EnumerateById())
		{
			if (item.Type == NefsItemType.Directory || item.DataSource.Size.Chunks.Count == 0)
			{
				// Item does not have a part 4 entry
				continue;
			}

			// Log this start index to item's Guid to allow lookup later
			this.indexLookup.Add(item.Guid, nextStartIdx);

			// Create entry for each data chunk
			foreach (var chunk in item.DataSource.Size.Chunks)
			{
				// Create entry
				var entry = new Nefs151HeaderPart4Entry
				{
					Checksum = 0x848, // TODO - How to compute this value is unknown. Writing bogus data for now.
					CumulativeBlockSize = chunk.CumulativeSize,
					TransformType = GetTransformType(chunk.Transform),
				};
				this.entriesByIndex.Add(entry);

				nextStartIdx++;
			}
		}
	}

	/// <summary>
	/// List of data chunk info in order as they appear in the header.
	/// </summary>
	public IReadOnlyList<Nefs151HeaderPart4Entry> EntriesByIndex => this.entriesByIndex;

	/// <inheritdoc/>
	IReadOnlyList<INefsHeaderPartEntry> INefsHeaderPart4.EntriesByIndex => this.entriesByIndex;

	/// <summary>
	/// Gets the current size of header part 4.
	/// </summary>
	public int Size => (this.entriesByIndex.Count * Nefs151HeaderPart4Entry.EntrySize) + LastValueSize;

	/// <summary>
	/// There is a 4-byte value at the end of header part 4. Purpose unknown.
	/// </summary>
	public uint UnkownEndValue { get; }

	/// <summary>
	/// Creates a list of chunk metadata for an item.
	/// </summary>
	/// <param name="index">The part 4 index where the chunk list starts at.</param>
	/// <param name="numChunks">The number of chunks.</param>
	/// <param name="chunkSize">The raw chunk size used in the transform.</param>
	/// <param name="aes256key">The AES 256 key to use if chunk is encrypted.</param>
	/// <returns>A list of chunk data.</returns>
	public List<NefsDataChunk> CreateChunksList(uint index, uint numChunks, uint chunkSize, byte[]? aes256key)
	{
		var chunks = new List<NefsDataChunk>();

		for (var i = index; i < index + numChunks; ++i)
		{
			var entry = this.entriesByIndex[(int)i];
			var cumulativeSize = entry.CumulativeBlockSize;
			var size = cumulativeSize;

			if (i > index)
			{
				size -= this.entriesByIndex[(int)i - 1].CumulativeBlockSize;
			}

			// Determine transform
			var transform = GetTransform(entry.TransformType, chunkSize, aes256key);
			if (transform is null)
			{
				Log.LogError($"Found v1.5 data chunk with unknown transform ({entry.TransformType}); aborting.");
				return new List<NefsDataChunk>();
			}

			// Create data chunk info
			var chunk = new NefsDataChunk(size, cumulativeSize, transform);
			chunks.Add(chunk);
		}

		return chunks;
	}

	/// <inheritdoc/>
	public uint GetIndexForItem(NefsItem item)
	{
		// Get index to part 4
		if (item.Type == NefsItemType.Directory)
		{
			// Item is a directory; the index 0
			return 0;
		}
		else
		{
			// Get index into part 4
			return this.indexLookup[item.Guid];
		}
	}

	private NefsDataTransform? GetTransform(Nefs16HeaderPart4TransformType type, uint chunkSize, byte[]? aes256key) => type switch
	{
		Nefs16HeaderPart4TransformType.Zlib => new NefsDataTransform(chunkSize, true),
		Nefs16HeaderPart4TransformType.Aes => new NefsDataTransform(chunkSize, false, aes256key),
		Nefs16HeaderPart4TransformType.None => new NefsDataTransform(chunkSize, false),
		_ => null,
	};

	private Nefs16HeaderPart4TransformType GetTransformType(NefsDataTransform transform)
	{
		// Can have both aes and zlib simulatneously?
		if (transform.IsAesEncrypted && transform.IsZlibCompressed)
		{
			Log.LogWarning("Found multiple data transforms for header part 4 entry.");
		}

		if (transform.IsAesEncrypted)
		{
			return Nefs16HeaderPart4TransformType.Aes;
		}
		else if (transform.IsZlibCompressed)
		{
			return Nefs16HeaderPart4TransformType.Zlib;
		}

		return Nefs16HeaderPart4TransformType.None;
	}
}
