// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.Header.Version020;
using VictorBush.Ego.NefsLib.Header.Version130;
using VictorBush.Ego.NefsLib.Header.Version140;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal abstract class NefsItemListBuilder(ILogger logger)
{
	protected ILogger Logger { get; } = logger;

	public static NefsItemListBuilder Create(INefsHeader header)
	{
		var logger = NefsLog.GetLogger();
		return header switch
		{
			NefsHeader010 h => new NefsItemListBuilder010(h, logger),
			NefsHeader020 h => new NefsItemListBuilder020(h, logger),
			NefsHeader130 h => new NefsItemListBuilder130(h, logger),
			NefsHeader140 h => new NefsItemListBuilder140(h, logger),
			NefsHeader150 h => new NefsItemListBuilder150(h, logger),
			NefsHeader151 h => new NefsItemListBuilder151(h, logger),
			NefsHeader160 h => new NefsItemListBuilder160(h, logger),
			NefsHeader200 h => new NefsItemListBuilder200(h, logger),
			_ => throw new ArgumentException($"Header of type {header.GetType().Name} is not supported.")
		};
	}

	public abstract NefsItemList Build(string dataFilePath, NefsProgress p);

	/// <summary>
	/// Builds an item from the header.
	/// </summary>
	/// <param name="entryIndex">The entry index.</param>
	/// <param name="itemList">The item list being built.</param>
	/// <returns>A new <see cref="NefsItem"/>.</returns>
	internal abstract NefsItem BuildItem(uint entryIndex, NefsItemList itemList);

	protected abstract NefsDataTransformType GetTransformType(uint blockTransformation);
}

internal abstract class NefsItemListBuilder<T>(T header, ILogger logger) : NefsItemListBuilder(logger)
	where T : INefsHeader
{
	protected T Header { get; } = header;

	protected virtual bool SupportsBlockChecksum => false;

	public override NefsItemList Build(string dataFilePath, NefsProgress p)
	{
		var weight = 1f / Header.NumEntries;
		using var _ = p.BeginTask(1.0f, "Creating items");
		var volumes = BuildVolumeSources(dataFilePath);
		var items = new NefsItemList(volumes);
		for (var i = 0; i < Header.NumEntries; ++i)
		{
			p.CancellationToken.ThrowIfCancellationRequested();

			try
			{
				using var __ = p.BeginSubTask(weight);
				var item =  BuildItem((uint)i, items);
				items.Add(item);
			}
			catch (Exception)
			{
				Logger.LogError($"Failed to create item for entry index {i}, skipping.");
			}
		}

		return items;
	}

	private NefsVolumeSource[] BuildVolumeSources(string dataFilePath)
	{
		var volumes = new NefsVolumeSource[Header.Volumes.Count];
		for (var i = 0; i < volumes.Length; ++i)
		{
			var headerVolume = Header.Volumes[i];
			string filePath;
			if (i == 0)
			{
				filePath = dataFilePath;
			}
			else
			{
				if (Header.Version is NefsVersion.Version010 or NefsVersion.Version020)
				{
					// Version 0.2.0 and earlier don't store file name
					filePath = dataFilePath[..^7] + i.ToString("D3") + Path.GetExtension(dataFilePath);
				}
				else
				{
					var volumeDirectory = Path.GetDirectoryName(dataFilePath) ?? string.Empty;
					filePath = Path.Combine(volumeDirectory, headerVolume.Name);
				}
			}

			var volume = new NefsVolumeSource(filePath, headerVolume.DataOffset, Header.SplitSize);
			volumes[i] = volume;
		}

		return volumes;
	}

	/// <summary>
	/// Creates a list of block metadata for an item.
	/// </summary>
	/// <param name="firstBlock">The block table start index for the item.</param>
	/// <param name="numBlocks">The number of chunks.</param>
	/// <param name="transform">The transform to use, or null to determine from block.</param>
	/// <returns>A list of blocks.</returns>
	protected List<NefsDataChunk> BuildBlockList(uint firstBlock, uint numBlocks, NefsDataTransform? transform)
	{
		var chunks = new List<NefsDataChunk>();
		for (var i = firstBlock; i < firstBlock + numBlocks; ++i)
		{
			var block = GetBlock(i);
			var cumulativeSize = block.End;
			var size = cumulativeSize;

			if (i > firstBlock)
			{
				var prevBlock = GetBlock(i - 1);
				size -= prevBlock.End;
			}

			// Determine transform
			transform ??= GetTransform(block.Transformation);
			if (transform is null)
			{
				Logger.LogError("Found data chunk with unknown transform {BlockTransformation}; aborting.",
					block.Transformation);
				// Assume one big untransformed block
				var lastBlock = GetBlock(firstBlock + numBlocks - 1);
				return [new NefsDataChunk(lastBlock.End, lastBlock.End, GetTransform(NefsDataTransformType.None)!)];
			}

			// Create data chunk info
			var chunk = new NefsDataChunk(size, cumulativeSize, transform) { Checksum = block.Checksum };
			chunks.Add(chunk);
		}

		return chunks;
	}

	protected abstract (uint End, uint Transformation, ushort Checksum) GetBlock(uint blockIndex);

	protected NefsDataTransform? GetTransform(uint blockTransformation)
	{
		var type = GetTransformType(blockTransformation);
		return GetTransform(type);
	}

	private NefsDataTransform? GetTransform(NefsDataTransformType type)
	{
		return type switch
		{
			NefsDataTransformType.Zlib => new NefsDataTransform(Header.BlockSize, true)
				{ ComputeChecksum = SupportsBlockChecksum },
			NefsDataTransformType.Aes => new NefsDataTransform(Header.BlockSize, false, Header.AesKey)
				{ ComputeChecksum = SupportsBlockChecksum },
			NefsDataTransformType.Lzss => new NefsDataTransform(Header.BlockSize, false)
				{ ComputeChecksum = SupportsBlockChecksum, IsLzssCompressed = true },
			NefsDataTransformType.None => new NefsDataTransform(Header.BlockSize, false)
				{ ComputeChecksum = SupportsBlockChecksum },
			_ => null,
		};
	}

	/// <summary>
	/// Gets the number of blocks a file will be split up into based on extracted size.
	/// </summary>
	/// <param name="extractedSize">The extracted size of the file.</param>
	/// <returns>The number of data blocks to expect.</returns>
	protected uint GetNumBlocks(uint extractedSize) =>
		(extractedSize + (Header.BlockSize - 1)) / Header.BlockSize;
}
