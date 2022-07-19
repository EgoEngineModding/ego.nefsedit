// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// A NeFS archive header.
/// </summary>
public sealed class Nefs20Header : INefsHeader
{
	/// <summary>
	/// Offset to the first data item used in most archives.
	/// </summary>
	public const uint DataOffsetDefault = 0x10000U;

	/// <summary>
	/// Offset to the first data item used in large archives where header needs more room.
	/// </summary>
	public const uint DataOffsetLarge = 0x50000U;

	/// <summary>
	/// Offset to the header intro.
	/// </summary>
	public const uint IntroOffset = 0x0;

	private static readonly ILogger Log = NefsLog.GetLogger();

	/// <summary>
	/// Initializes a new instance of the <see cref="Nefs20Header"/> class.
	/// </summary>
	/// <param name="intro">Header intro.</param>
	/// <param name="toc">Header intro table of contents.</param>
	/// <param name="part1">Header part 1.</param>
	/// <param name="part2">Header part 2.</param>
	/// <param name="part3">Header part 3.</param>
	/// <param name="part4">Header part 4.</param>
	/// <param name="part5">Header part 5.</param>
	/// <param name="part6">Header part 6.</param>
	/// <param name="part7">Header part 7.</param>
	/// <param name="part8">Header part 8.</param>
	public Nefs20Header(
		NefsHeaderIntro intro,
		Nefs20HeaderIntroToc toc,
		NefsHeaderPart1 part1,
		NefsHeaderPart2 part2,
		NefsHeaderPart3 part3,
		Nefs20HeaderPart4 part4,
		NefsHeaderPart5 part5,
		Nefs20HeaderPart6 part6,
		NefsHeaderPart7 part7,
		NefsHeaderPart8 part8)
	{
		Intro = intro ?? throw new ArgumentNullException(nameof(intro));
		TableOfContents = toc ?? throw new ArgumentNullException(nameof(toc));
		Part1 = part1 ?? throw new ArgumentNullException(nameof(part1));
		Part2 = part2 ?? throw new ArgumentNullException(nameof(part2));
		Part3 = part3 ?? throw new ArgumentNullException(nameof(part3));
		Part4 = part4 ?? throw new ArgumentNullException(nameof(part4));
		Part5 = part5 ?? throw new ArgumentNullException(nameof(part5));
		Part6 = part6 ?? throw new ArgumentNullException(nameof(part6));
		Part7 = part7 ?? throw new ArgumentNullException(nameof(part7));
		Part8 = part8 ?? throw new ArgumentNullException(nameof(part8));
	}

	/// <summary>
	/// The header intro.
	/// </summary>
	public NefsHeaderIntro Intro { get; }

	/// <inheritdoc/>
	public bool IsEncrypted => Intro.IsEncrypted;

	/// <summary>
	/// Header part 1.
	/// </summary>
	public NefsHeaderPart1 Part1 { get; }

	/// <summary>
	/// Header part 2.
	/// </summary>
	public NefsHeaderPart2 Part2 { get; }

	/// <summary>
	/// Header part 3.
	/// </summary>
	public NefsHeaderPart3 Part3 { get; }

	/// <summary>
	/// Header part 4.
	/// </summary>
	public Nefs20HeaderPart4 Part4 { get; }

	/// <summary>
	/// Header part 5.
	/// </summary>
	public NefsHeaderPart5 Part5 { get; }

	/// <summary>
	/// Header part 6.
	/// </summary>
	public Nefs20HeaderPart6 Part6 { get; }

	/// <summary>
	/// Header part 7.
	/// </summary>
	public NefsHeaderPart7 Part7 { get; }

	/// <summary>
	/// Header part 8.
	/// </summary>
	public NefsHeaderPart8 Part8 { get; }

	/// <summary>
	/// The header intro table of contents.
	/// </summary>
	public Nefs20HeaderIntroToc TableOfContents { get; }

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
		var p6 = Part6.EntriesByGuid[guid];
		var id = p1.Id;

		// Gather attributes
		var attributes = p6.CreateAttributes();

		// Find parent
		var parentId = GetItemDirectoryId(p1.IndexPart2);

		// Offset and size
		var dataOffset = (long)p1.OffsetToData;
		var extractedSize = p2.ExtractedSize;

		// Transform
		var transform = new NefsDataTransform(Nefs20HeaderIntroToc.ChunkSize, attributes.V20IsZlib, attributes.V20IsAes ? Intro.GetAesKey() : null);

		// Data source
		INefsDataSource dataSource;
		if (attributes.IsDirectory)
		{
			// Item is a directory
			dataSource = new NefsEmptyDataSource();
			transform = null;
		}
		else if (p1.IndexPart4 == 0xFFFFFFFFU)
		{
			// Item is not compressed
			var size = new NefsItemSize(extractedSize);
			dataSource = new NefsItemListDataSource(dataSourceList, dataOffset, size);
		}
		else
		{
			// Item is compressed
			var numChunks = TableOfContents.ComputeNumChunks(p2.ExtractedSize);
			var chunks = Part4.CreateChunksList(p1.IndexPart4, numChunks, transform);
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
