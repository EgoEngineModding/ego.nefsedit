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

    /// <summary>
    /// <para>Test archive that has modified items in it.</para>
    /// <list>
    /// <item>/file1</item>
    /// <item>/dir1</item>
    /// <item>/dir1/file2</item>
    /// <item>/dir1/file3</item>
    /// </list>
    /// </summary>
    internal class TestArchiveNotModified
    {
        /*
        Directory 1 - in root of archive.
        */

        public static UInt32 Dir1DirectoryId => Dir1ItemId;

        public static UInt32 Dir1ItemId => 1;

        public static string Dir1Name => "dir1";

        public static string Dir1PathInArchive => Dir1Name;

        public static UInt32 Dir1SiblingId => Dir1ItemId;

        /*
        File 1 - in root of archive.
        */

        public static IReadOnlyList<UInt32> File1ChunkSizes { get; } = new List<UInt32> { 10, 20, 30 };

        public static UInt32 File1DirectoryId => File1ItemId;

        public static UInt32 File1ExtractedSize => 100;

        public static UInt32 File1ItemId => 0;

        public static string File1Name => "file1.txt";

        public static UInt64 File1Offset => NefsHeader.DataOffsetDefault;

        public static string File1PathInArchive => File1Name;

        public static UInt32 File1SiblingId => Dir1ItemId;

        /*
        File 2 - inside directory "dir1".
        */

        public static IReadOnlyList<UInt32> File2ChunkSizes { get; } = new List<UInt32> { 5, 15, 25 };

        public static UInt32 File2DirectoryId => Dir1ItemId;

        public static UInt32 File2ExtractedSize => 50;

        public static UInt32 File2ItemId => 2;

        public static string File2Name => "file2.txt";

        public static UInt64 File2Offset => File1Offset + File1ChunkSizes.Last();

        public static string File2PathInArchive => $@"{Dir1PathInArchive}\{File2Name}";

        public static UInt32 File2SiblingId => File3ItemId;

        /*
        File 3 - Not compressed. In directory "dir1".
        */

        public static IReadOnlyList<UInt32> File3ChunkSizes { get; } = new List<UInt32> { 31 };

        public static UInt32 File3DirectoryId => Dir1ItemId;

        public static UInt32 File3ExtractedSize => 31;

        public static UInt32 File3ItemId => 3;

        public static string File3Name => "file3.txt";

        public static UInt64 File3Offset => File2Offset + File2ChunkSizes.Last();

        public static string File3PathInArchive => $@"{Dir1PathInArchive}\{File3Name}";

        public static UInt32 File3SiblingId => File3ItemId;

        public static UInt32 NumItems => 4;

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
            var file1 = new NefsItem(new NefsItemId(File1ItemId), File1Name, new NefsItemId(File1DirectoryId), NefsItemType.File, file1DataSource, TestHelpers.CreateUnknownData());
            items.Add(file1);

            var dir1DataSource = new NefsEmptyDataSource();
            var dir1 = new NefsItem(new NefsItemId(Dir1ItemId), Dir1Name, new NefsItemId(Dir1DirectoryId), NefsItemType.Directory, dir1DataSource, TestHelpers.CreateUnknownData());
            items.Add(dir1);

            var file2DataSource = new NefsItemListDataSource(items, File2Offset, new NefsItemSize(File2ExtractedSize, File2ChunkSizes));
            var file2 = new NefsItem(new NefsItemId(File2ItemId), File2Name, new NefsItemId(File2DirectoryId), NefsItemType.File, file2DataSource, TestHelpers.CreateUnknownData());
            items.Add(file2);

            var file3DataSource = new NefsItemListDataSource(items, File3Offset, new NefsItemSize(File3ExtractedSize, File3ChunkSizes));
            var file3 = new NefsItem(new NefsItemId(File3ItemId), File3Name, new NefsItemId(File3DirectoryId), NefsItemType.File, file3DataSource, TestHelpers.CreateUnknownData());
            items.Add(file3);

            Assert.Equal((int)NumItems, items.Count);

            var intro = new NefsHeaderIntro();
            intro.Data0x6c_NumberOfItems.Value = (uint)items.Count;

            var toc = new NefsHeaderIntroToc();

            var header = new NefsHeader(intro, toc, items);

            return new NefsArchive(header, items);
        }
    }
}
