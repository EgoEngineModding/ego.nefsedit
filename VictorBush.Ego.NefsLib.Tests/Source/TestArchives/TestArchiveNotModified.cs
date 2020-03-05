// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.TestArchives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;
    using Xunit;

    internal class TestArchiveNotModified
    {
        public static UInt32 Dir1DirectoryId => Dir1ItemId;

        public static UInt32 Dir1ItemId => File1ItemId + 1;

        public static string Dir1Name => "dir1";

        public static string Dir1PathInArchive => "dir";

        public static IReadOnlyList<UInt32> File1ChunkSizes { get; } = new List<UInt32> { 10, 20, 30 };

        public static UInt32 File1DirectoryId => File1ItemId;

        public static UInt32 File1ExtractedSize => 100;

        public static UInt32 File1ItemId => 0;

        public static string File1Name => "file1.txt";

        public static UInt64 File1Offset => NefsHeader.DataOffsetDefault;

        public static string File1PathInArchive => "file1.txt";

        public static IReadOnlyList<UInt32> File2ChunkSizes { get; } = new List<UInt32> { 5, 15, 25 };

        public static UInt32 File2DirectoryId => Dir1ItemId;

        public static UInt32 File2ExtractedSize => 50;

        public static UInt32 File2ItemId => Dir1ItemId + 1;

        public static string File2Name => "file2.txt";

        public static UInt64 File2Offset => File1Offset + File1ChunkSizes.Last();

        public static string File2PathInArchive => "dir1/file2.txt";

        public static UInt32 NumItems => 3;

        /// <summary>
        /// Creates a test archive. Does not write an archive to disk. Just creates a <see
        /// cref="NefsArchive"/> object.
        /// </summary>
        /// <param name="filePath">The file path to use for the archive.</param>
        /// <returns>A <see cref="NefsArchive"/>.</returns>
        public static NefsArchive Create(string filePath)
        {
            var items = new NefsItemList(filePath);

            var file1DataSource = new NefsItemListDataSource(items, File1Offset, new NefsItemSize(File1ExtractedSize, File1ChunkSizes));
            var file1 = new NefsItem(new NefsItemId(File1ItemId), File1Name, File1PathInArchive, new NefsItemId(File1DirectoryId), NefsItemType.File, file1DataSource, TestHelpers.CreateUnknownData());
            items.Add(file1);

            var dir1DataSource = new NefsEmptyDataSource();
            var dir1 = new NefsItem(new NefsItemId(Dir1ItemId), Dir1Name, Dir1PathInArchive, new NefsItemId(Dir1DirectoryId), NefsItemType.Directory, dir1DataSource, TestHelpers.CreateUnknownData());
            items.Add(dir1);

            var file2DataSource = new NefsItemListDataSource(items, File2Offset, new NefsItemSize(File2ExtractedSize, File2ChunkSizes));
            var file2 = new NefsItem(new NefsItemId(File2ItemId), File2Name, File2PathInArchive, new NefsItemId(File2DirectoryId), NefsItemType.File, file2DataSource, TestHelpers.CreateUnknownData());
            items.Add(file2);

            Assert.Equal((int)NumItems, items.Count);

            var intro = new NefsHeaderIntro();
            var header = new NefsHeader(intro, items);

            return new NefsArchive(header, items);
        }
    }
}
