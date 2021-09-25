// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using Microsoft.Extensions.Logging;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// A NeFS archive header.
    /// </summary>
    public class Nefs16Header : INefsHeader
    {
        /// <summary>
        /// Offset to the first data item used in most archives.
        /// </summary>
        public const UInt32 DataOffsetDefault = 0x10000U;

        /// <summary>
        /// Offset to the first data item used in large archives where header needs more room.
        /// </summary>
        public const UInt32 DataOffsetLarge = 0x50000U;

        /// <summary>
        /// Offset to the header intro.
        /// </summary>
        public const uint IntroOffset = 0x0;

        private static readonly ILogger Log = NefsLog.GetLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="Nefs16Header"/> class.
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
        public Nefs16Header(
            NefsHeaderIntro intro,
            Nefs16HeaderIntroToc toc,
            NefsHeaderPart1 part1,
            NefsHeaderPart2 part2,
            NefsHeaderPart3 part3,
            Nefs16HeaderPart4 part4,
            NefsHeaderPart5 part5,
            Nefs16HeaderPart6 part6,
            NefsHeaderPart7 part7,
            NefsHeaderPart8 part8)
        {
            this.Intro = intro ?? throw new ArgumentNullException(nameof(intro));
            this.TableOfContents = toc ?? throw new ArgumentNullException(nameof(toc));
            this.Part1 = part1 ?? throw new ArgumentNullException(nameof(part1));
            this.Part2 = part2 ?? throw new ArgumentNullException(nameof(part2));
            this.Part3 = part3 ?? throw new ArgumentNullException(nameof(part3));
            this.Part4 = part4 ?? throw new ArgumentNullException(nameof(part4));
            this.Part5 = part5 ?? throw new ArgumentNullException(nameof(part5));
            this.Part6 = part6 ?? throw new ArgumentNullException(nameof(part6));
            this.Part7 = part7 ?? throw new ArgumentNullException(nameof(part7));
            this.Part8 = part8 ?? throw new ArgumentNullException(nameof(part8));
        }

        /// <summary>
        /// The header intro.
        /// </summary>
        public NefsHeaderIntro Intro { get; }

        /// <inheritdoc/>
        public Boolean IsEncrypted => this.Intro.IsEncrypted;

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
        public Nefs16HeaderPart4 Part4 { get; }

        /// <summary>
        /// Header part 5.
        /// </summary>
        public NefsHeaderPart5 Part5 { get; }

        /// <summary>
        /// Header part 6.
        /// </summary>
        public Nefs16HeaderPart6 Part6 { get; }

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
        public Nefs16HeaderIntroToc TableOfContents { get; }

        /// <inheritdoc/>
        public NefsItem CreateItemInfo(uint part1Index, NefsItemList dataSourceList)
        {
            return this.CreateItemInfo(this.Part1.EntriesByIndex[(int)part1Index].Guid, dataSourceList);
        }

        /// <inheritdoc/>
        public NefsItem CreateItemInfo(Guid guid, NefsItemList dataSourceList)
        {
            var p1 = this.Part1.EntriesByGuid[guid];
            var p2 = this.Part2.EntriesByIndex[(int)p1.IndexPart2];
            var p6 = this.Part6.EntriesByGuid[guid];
            var id = p1.Id;

            // Gather attributes
            var attributes = p6.CreateAttributes();

            // Find parent
            var parentId = this.GetItemDirectoryId(p1.IndexPart2);

            // Offset and size
            var dataOffset = (long)p1.Data0x00_OffsetToData.Value;
            var extractedSize = p2.Data0x0c_ExtractedSize.Value;

            // Transform
            var transform = new NefsDataTransform(this.TableOfContents.BlockSize, true, this.Intro.IsEncrypted ? this.Intro.GetAesKey() : null);

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
                var numChunks = this.TableOfContents.ComputeNumChunks(p2.ExtractedSize);
                var chunkSize = this.TableOfContents.BlockSize;
                var chunks = this.Part4.CreateChunksList(p1.IndexPart4, numChunks, chunkSize, this.Intro.GetAesKey());
                var size = new NefsItemSize(extractedSize, chunks);
                dataSource = new NefsItemListDataSource(dataSourceList, dataOffset, size);
            }

            // File name and path
            var fileName = this.GetItemFileName(p1.IndexPart2);

            // Create item
            return new NefsItem(p1.Guid, id, fileName, parentId, dataSource, transform, attributes);
        }

        /// <inheritdoc/>
        public NefsItemList CreateItemList(String dataFilePath, NefsProgress p)
        {
            var items = new NefsItemList(dataFilePath);

            for (var i = 0; i < this.Part1.EntriesByIndex.Count; ++i)
            {
                p.CancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var item = this.CreateItemInfo((uint)i, items);
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
            return new NefsItemId(this.Part2.EntriesByIndex[(int)indexPart2].Data0x00_DirectoryId.Value);
        }

        /// <inheritdoc/>
        public string GetItemFileName(uint indexPart2)
        {
            var offsetIntoPart3 = this.Part2.EntriesByIndex[(int)indexPart2].Data0x08_OffsetIntoPart3.Value;
            return this.Part3.FileNamesByOffset[offsetIntoPart3];
        }
    }
}
