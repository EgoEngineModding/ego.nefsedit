// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.Header.Version151;

/// <summary>
/// A NeFS archive header.
/// </summary>
public sealed class Nefs150Header : INefsHeader
{
	private static readonly ILogger Log = NefsLog.GetLogger();

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs151Header"/> class.
	/// </summary>
	/// <param name="intro">Header intro.</param>
	/// <param name="part1">Header part 1.</param>
	/// <param name="part2">Header part 2.</param>
	/// <param name="part3">Header part 3.</param>
	/// <param name="part4">Header part 4.</param>
	/// <param name="part5">Header part 5.</param>
	public Nefs150Header(
		Nefs150HeaderIntro intro,
		Nefs150HeaderPart1 part1,
		Nefs150HeaderPart2 part2,
		NefsHeaderPart3 part3,
		Nefs150HeaderPart4 part4,
		NefsHeaderPart5 part5)
	{
		Intro = intro ?? throw new ArgumentNullException(nameof(intro));
		Part1 = part1 ?? throw new ArgumentNullException(nameof(part1));
		Part2 = part2 ?? throw new ArgumentNullException(nameof(part2));
		Part3 = part3 ?? throw new ArgumentNullException(nameof(part3));
		Part4 = part4 ?? throw new ArgumentNullException(nameof(part4));
		Part5 = part5 ?? throw new ArgumentNullException(nameof(part5));
	}

	public Nefs150HeaderIntro Intro { get; }

	/// <inheritdoc/>
	public bool IsEncrypted => Intro.IsEncrypted;

	public Nefs150HeaderPart1 Part1 { get; }
	public Nefs150HeaderPart2 Part2 { get; }
	public NefsHeaderPart3 Part3 { get; }
	public Nefs150HeaderPart4 Part4 { get; }
	public NefsHeaderPart5 Part5 { get; }

	/// <inheritdoc/>
	public NefsItem CreateItemInfo(uint part1Index, NefsItemList dataSourceList)
	{
		return CreateItemInfo(Part1.EntriesByIndex[(int)part1Index].Guid, dataSourceList);
	}

	/// <inheritdoc/>
	public NefsItem CreateItemInfo(Guid guid, NefsItemList dataSourceList)
	{
		var p1 = Part1.EntriesByGuid[guid];
		var p2 = Part2.EntriesByIndex[(int)p1.IndexPart2];
		var id = p2.Id;

		// Gather attributes
		var attributes = p1.CreateAttributes();

		// Find parent
		var parentId = GetItemDirectoryId(p1.IndexPart2);

		// Offset and size
		var dataOffset = (long)p1.OffsetToData;
		var extractedSize = p2.ExtractedSize;

		// Transform
		var transform = new NefsDataTransform(Intro.BlockSize, attributes.V20IsZlib, Intro.IsEncrypted ? Intro.GetAesKey() : null);

		// Data source
		INefsDataSource dataSource;
		if (attributes.IsDirectory)
		{
			// Item is a directory
			dataSource = new NefsEmptyDataSource();
			transform = null;
		}
		else
		{
			var numChunks = Intro.ComputeNumChunks(p2.ExtractedSize);
			var chunkSize = Intro.BlockSize;
			var chunks = Part4.CreateChunksList(p1.IndexPart4, numChunks, chunkSize, Intro.GetAesKey());
			var size = new NefsItemSize(extractedSize, chunks);
			dataSource = new NefsItemListDataSource(dataSourceList, dataOffset, size);
		}

		// File name and path
		var fileName = GetItemFileName(p1.IndexPart2);

		// Create item
		return new NefsItem(p1.Guid, id, fileName, parentId, dataSource, transform, attributes);
	}

	/// <inheritdoc/>
	public NefsItemList CreateItemList(string dataFilePath, NefsProgress p)
	{
		var items = new NefsItemList(dataFilePath);

		for (var i = 0; i < Part1.EntriesByIndex.Count; ++i)
		{
			p.CancellationToken.ThrowIfCancellationRequested();

			try
			{
				var item = CreateItemInfo((uint)i, items);
				items.Add(item);
			}
			catch (Exception)
			{
				Log.LogError($"Failed to create item with part 1 index {i}, skipping.");
			}
		}

		return items;
	}

	/// <inheritdoc/>
	public NefsItemId GetItemDirectoryId(uint indexPart2)
	{
		return Part2.EntriesByIndex[(int)indexPart2].DirectoryId;
	}

	/// <inheritdoc/>
	public string GetItemFileName(uint indexPart2)
	{
		var offsetIntoPart3 = Part2.EntriesByIndex[(int)indexPart2].OffsetIntoPart3;
		return Part3.FileNamesByOffset[offsetIntoPart3];
	}
}
