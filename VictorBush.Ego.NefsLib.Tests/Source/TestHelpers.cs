// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions.TestingHelpers;
    using System.Linq;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;

    internal static class TestHelpers
    {
        /// <summary>
        /// The path to the test data file for data type tests. The file is created with the <see
        /// cref="DataTypesTestData"/> and put on the mock file system.
        /// </summary>
        internal const string DataTypesTestFilePath = "DataTypesTest.dat";

        /// <summary>
        /// Test data used for data type tests.
        /// </summary>
        internal static readonly byte[] DataTypesTestData = new byte[]
        {
            0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01,
            0x18, 0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11,
            0x28, 0x27, 0x26, 0x25, 0x24, 0x23, 0x22, 0x21,
            0x38, 0x37, 0x36, 0x35, 0x34, 0x33, 0x32, 0x31,
        };

        /// <summary>
        /// Transform used for common test items.
        /// </summary>
        internal static NefsDataTransform TestTransform { get; } = new NefsDataTransform(100, true);

        /// <summary>
        /// Creates a mock file system for data type tests that includes a test file.
        /// </summary>
        /// <returns>A mock file system.</returns>
        internal static MockFileSystem CreateDataTypesTestFileSystem()
        {
            var fs = new MockFileSystem();
            fs.AddFile(DataTypesTestFilePath, new MockFileData(DataTypesTestData));
            return fs;
        }

        /// <summary>
        /// Creates an item for testing.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="dirId">The directory id.</param>
        /// <param name="fileName">The item name.</param>
        /// <returns>The new item.</returns>
        internal static NefsItem CreateDirectory(
            uint id,
            uint dirId,
            string fileName)
        {
            return new NefsItem(
                Guid.NewGuid(),
                new NefsItemId(id),
                fileName,
                new NefsItemId(dirId),
                NefsItemType.Directory,
                new NefsEmptyDataSource(),
                null,
                CreateUnknownData());
        }

        /// <summary>
        /// Creates an item for testing.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="dirId">The directory id.</param>
        /// <param name="fileName">The item name.</param>
        /// <param name="dataSource">The data source.</param>
        /// <returns>The new item.</returns>
        internal static NefsItem CreateFile(
            uint id,
            uint dirId,
            string fileName,
            INefsDataSource dataSource)
        {
            var transform = TestTransform;
            return new NefsItem(
                Guid.NewGuid(),
                new NefsItemId(id),
                fileName,
                new NefsItemId(dirId),
                NefsItemType.File,
                dataSource,
                transform,
                CreateUnknownData());
        }

        /// <summary>
        /// Creates an item for testing.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="dirId">The directory id.</param>
        /// <param name="fileName">The item name.</param>
        /// <param name="dataOffset">Data offset.</param>
        /// <param name="extractedSize">Extracted size.</param>
        /// <param name="chunkSizes">Compressed chunks sizes.</param>
        /// <param name="type">The item type.</param>
        /// <returns>The new item.</returns>
        internal static NefsItem CreateItem(
            uint id,
            uint dirId,
            string fileName,
            UInt64 dataOffset,
            UInt32 extractedSize,
            IReadOnlyList<UInt32> chunkSizes,
            NefsItemType type)
        {
            var transform = TestTransform;
            var chunks = NefsDataChunk.CreateChunkList(chunkSizes, transform);
            var size = new NefsItemSize(extractedSize, chunks);
            var dataSource = new NefsFileDataSource(@"C:\source.txt", dataOffset, size, extractedSize != chunkSizes.LastOrDefault());
            return new NefsItem(
                Guid.NewGuid(),
                new NefsItemId(id),
                fileName,
                new NefsItemId(dirId),
                type,
                dataSource,
                transform,
                CreateUnknownData());
        }

        /// <summary>
        /// Creates empty unknown header data.
        /// </summary>
        /// <returns>An empty <see cref="NefsItemUnknownData"/>.</returns>
        internal static NefsItemUnknownData CreateUnknownData()
        {
            return new NefsItemUnknownData
            {
                Part6Unknown0x00 = 0,
                Part6Unknown0x01 = 0,
                Part6Unknown0x02 = 0,
                Part6Unknown0x03 = 0,
            };
        }
    }
}
