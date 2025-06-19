﻿// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataSource;

/// <summary>
/// Describes a chunk of data in an archive.
/// </summary>
public class NefsDataChunk
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsDataChunk"/> class.
	/// </summary>
	/// <param name="size">The size of the data chunk.</param>
	/// <param name="cumulativeSize">The cumulative size of the data chunk.</param>
	/// <param name="transform">The transform that has been applied to this chunk.</param>
	public NefsDataChunk(uint size, uint cumulativeSize, NefsDataTransform transform)
	{
		Size = size;
		CumulativeSize = cumulativeSize;
		Transform = transform ?? throw new ArgumentNullException(nameof(transform));
	}

	/// <summary>
	/// The cumulative size of the chunk. This is equal to the size of this chunk + the size of all the previous chunks.
	/// </summary>
	public uint CumulativeSize { get; }

	/// <summary>
	/// The size of the chunk.
	/// </summary>
	public uint Size { get; }

	/// <summary>
	/// The transform that has been applied to this chunk.
	/// </summary>
	public NefsDataTransform Transform { get; }

	/// <summary>
	/// Creats a list of chunks given a list of cumulative chunk sizes.
	/// </summary>
	/// <param name="cumulativeSizes">List of cumulative chunk sizes.</param>
	/// <param name="transform">The transform applied to all chunks.</param>
	/// <returns>A list of chunks.</returns>
	public static List<NefsDataChunk> CreateChunkList(
		IReadOnlyList<uint> cumulativeSizes,
		NefsDataTransform transform)
	{
		var chunks = new List<NefsDataChunk>();

		for (var i = 0; i < cumulativeSizes.Count; ++i)
		{
			var size = cumulativeSizes[i];

			// Get individual size by subtracting previous cumulative size
			if (i > 0)
			{
				size -= cumulativeSizes[i - 1];
			}

			var chunk = new NefsDataChunk(size, cumulativeSizes[i], transform);
			chunks.Add(chunk);
		}

		return chunks;
	}
}
