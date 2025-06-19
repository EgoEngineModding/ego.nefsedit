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
	/// List of cumulative block sizes. If the item does not have blocks this list should be empty.
	/// </param>
	public NefsItemSize(uint extractedSize, IReadOnlyList<NefsDataChunk> chunks)
	{
		ExtractedSize = extractedSize;
		Chunks = chunks;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemSize"/> class. This constructor can be used if the item
	/// does not have blocks.
	/// </summary>
	/// <param name="extractedSize">The size of the item's data when extracted from the archive.</param>
	public NefsItemSize(uint extractedSize)
	{
		ExtractedSize = extractedSize;
		Chunks = [];
	}

	/// <summary>
	/// List of metadata for the item's data blocks (in order).
	/// </summary>
	public IReadOnlyList<NefsDataChunk> Chunks { get; }

	/// <summary>
	/// Gets the size of the data after any transforms (compression, encryption) are undone.
	/// </summary>
	public uint ExtractedSize { get; }

	/// <summary>
	/// Gets the size of the data in bytes after transforms (compression, encrypted) have been applied.
	/// </summary>
	public uint TransformedSize => Chunks.Count == 0 ? ExtractedSize : Chunks[^1].CumulativeSize;
}
