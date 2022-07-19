// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;

namespace VictorBush.Ego.NefsLib.Item;

/// <summary>
/// Stores information about a item's data size.
/// </summary>
public sealed class NefsItemSize
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemSize"/> class.
	/// </summary>
	/// <param name="extractedSize">The size of the item's data when extracted from the archive.</param>
	/// <param name="chunks">
	/// List of cumulative chunk sizes. If the item is not compressed, this list should only have one entry equal to the
	/// extracted size.)
	/// </param>
	public NefsItemSize(uint extractedSize, IReadOnlyList<NefsDataChunk> chunks)
	{
		ExtractedSize = extractedSize;
		Chunks = chunks ?? new List<NefsDataChunk>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemSize"/> class. This constructor can be used if the item is
	/// not compressed (i.e., extracted size == compressed size).
	/// </summary>
	/// <param name="extractedSize">The size of the item's data when extracted from the archive.</param>
	public NefsItemSize(uint extractedSize)
	{
		ExtractedSize = extractedSize;
		var transform = new NefsDataTransform(extractedSize);
		var chunk = new NefsDataChunk(extractedSize, extractedSize, transform);
		Chunks = new List<NefsDataChunk> { chunk };
	}

	/// <summary>
	/// List of metadata for the item's data chunks (in order).
	/// </summary>
	public IReadOnlyList<NefsDataChunk> Chunks { get; }

	/// <summary>
	/// Gets the size of the data after any transforms (compression, encryption) are undone.
	/// </summary>
	public uint ExtractedSize { get; }

	/// <summary>
	/// Gets the size of the data in bytes after transforms (compression, encrypted) have been applied.
	/// </summary>
	public uint TransformedSize => Chunks.LastOrDefault()?.CumulativeSize ?? 0;
}
