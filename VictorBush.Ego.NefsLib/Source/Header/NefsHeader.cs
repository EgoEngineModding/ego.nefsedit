// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.IO;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// A NeFS archive header.
    /// </summary>
    public class NefsHeader
    {
        /// <summary>
        /// Offset to the header intro.
        /// </summary>
        public const uint IntroOffset = 0x0;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeader"/> class.
        /// </summary>
        /// <param name="intro">Header intro.</param>
        /// <param name="part1">Header part 1.</param>
        /// <param name="part2">Header part 2.</param>
        /// <param name="part3">Header part 3.</param>
        /// <param name="part4">Header part 4.</param>
        /// <param name="part5">Header part 5.</param>
        /// <param name="part6">Header part 6.</param>
        /// <param name="part7">Header part 7.</param>
        /// <param name="part8">Header part 8.</param>
        public NefsHeader(
            NefsHeaderIntro intro,
            NefsHeaderPart1 part1,
            NefsHeaderPart2 part2,
            NefsHeaderPart3 part3,
            NefsHeaderPart4 part4,
            NefsHeaderPart5 part5,
            NefsHeaderPart6 part6,
            NefsHeaderPart7 part7,
            NefsHeaderPart8 part8)
        {
            this.Intro = intro ?? throw new ArgumentNullException(nameof(intro));
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
        /// Initializes a new instance of the <see cref="NefsHeader"/> class.
        /// </summary>
        /// <param name="intro">Header intro.</param>
        /// <param name="items">List of items.</param>
        public NefsHeader(NefsHeaderIntro intro, NefsItemList items)
        {
            this.Intro = intro ?? throw new ArgumentNullException(nameof(intro));

            this.Part3 = new NefsHeaderPart3(items);
            this.Part4 = new NefsHeaderPart4(items);

            this.Part1 = new NefsHeaderPart1(items, this.Part4);
            this.Part2 = new NefsHeaderPart2(items, this.Part3);
            this.Part4 = new NefsHeaderPart4(items);
            this.Part5 = new NefsHeaderPart5();
            this.Part6 = new NefsHeaderPart6(items);
            this.Part7 = new NefsHeaderPart7(items);
            this.Part8 = new NefsHeaderPart8();
        }

        /// <summary>
        /// The header intro.
        /// </summary>
        public NefsHeaderIntro Intro { get; }

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
        public NefsHeaderPart4 Part4 { get; }

        /// <summary>
        /// Header part 5.
        /// </summary>
        public NefsHeaderPart5 Part5 { get; }

        /// <summary>
        /// Header part 6.
        /// </summary>
        public NefsHeaderPart6 Part6 { get; }

        /// <summary>
        /// Header part 7.
        /// </summary>
        public NefsHeaderPart7 Part7 { get; }

        /// <summary>
        /// Header part 8.
        /// </summary>
        public NefsHeaderPart8 Part8 { get; }

        /// <summary>
        /// Gets the directory id for an item. If the item is in the root directory, the directory
        /// id will equal the item's id.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The directory id.</returns>
        public NefsItemId GetItemDirectoryId(NefsItemId id)
        {
            return new NefsItemId(this.Part2.EntriesById[id].DirectoryId.Value);
        }

        /// <summary>
        /// Gets the file name of an item.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The item's file name.</returns>
        public string GetItemFileName(NefsItemId id)
        {
            var offsetIntoPart3 = this.Part2.EntriesById[id].OffsetIntoPart3.Value;
            return this.Part3.FileNamesByOffset[offsetIntoPart3];
        }

        /// <summary>
        /// Gets the file path of the item within the archive.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The item's file path within the archive.</returns>
        public string GetItemFilePath(NefsItemId id)
        {
            var path = this.GetItemFileName(id);

            var dirId = this.GetItemDirectoryId(id);
            var prevDirId = dirId;

            while (dirId != prevDirId)
            {
                var dirName = this.GetItemFileName(dirId);
                path = Path.Combine(dirName, path);

                prevDirId = dirId;
                dirId = this.GetItemDirectoryId(dirId);
            }

            return path;
        }
    }
}
