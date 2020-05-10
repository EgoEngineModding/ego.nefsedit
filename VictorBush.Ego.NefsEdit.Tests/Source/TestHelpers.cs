// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Tests
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;

    internal class TestHelpers
    {
        /// <summary>
        /// Creates a <see cref="NefsArchive"/> to be used for testing.
        /// </summary>
        /// <param name="filePath">The file path to associate with the archive.</param>
        /// <returns>An archive object.</returns>
        /// <remarks><![CDATA[ Test archive items: /file1 /dir1 /dir1/file2 ]]></remarks>
        internal static NefsArchive CreateTestArchive(string filePath)
        {
            var items = new NefsItemList(filePath);

            var transform = new NefsDataTransform(50, true);

            var file1Attributes = new NefsItemAttributes(v20IsZlib: true);
            var file1Chunks = NefsDataChunk.CreateChunkList(new List<UInt32> { 2, 3, 4 }, transform);
            var file1DataSource = new NefsItemListDataSource(items, 100, new NefsItemSize(20, file1Chunks));
            var file1 = new NefsItem(Guid.NewGuid(), new NefsItemId(0), "file1", new NefsItemId(0), file1DataSource, transform, file1Attributes);
            items.Add(file1);

            var dir1Attributes = new NefsItemAttributes(isDirectory: true);
            var dir1DataSource = new NefsEmptyDataSource();
            var dir1 = new NefsItem(Guid.NewGuid(), new NefsItemId(1), "dir1", new NefsItemId(1), dir1DataSource, null, dir1Attributes);
            items.Add(dir1);

            var file2Attributes = new NefsItemAttributes(v20IsZlib: true);
            var file2Chunks = NefsDataChunk.CreateChunkList(new List<UInt32> { 5, 6, 7 }, transform);
            var file2DataSource = new NefsItemListDataSource(items, 104, new NefsItemSize(15, file2Chunks));
            var file2 = new NefsItem(Guid.NewGuid(), new NefsItemId(2), "file2", dir1.Id, file2DataSource, transform, file2Attributes);
            items.Add(file2);

            var intro = new NefsHeaderIntro();
            var toc = new Nefs20HeaderIntroToc();
            var header = new Nefs20Header(intro, toc, items);

            return new NefsArchive(header, items);
        }
    }
}
