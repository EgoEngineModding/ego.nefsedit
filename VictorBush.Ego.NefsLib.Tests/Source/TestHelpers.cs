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
        /// <param name="siblingId">The sibling id.</param>
        /// <param name="fileName">The item name.</param>
        /// <param name="filePathInArchive">The item's path in the archive.</param>
        /// <param name="dataOffset">Data offset.</param>
        /// <param name="extractedSize">Extracted size.</param>
        /// <param name="chunkSizes">Compressed chunks sizes.</param>
        /// <param name="type">The item type.</param>
        /// <returns>The new item.</returns>
        internal static NefsItem CreateItem(
            uint id,
            uint dirId,
            uint siblingId,
            string fileName,
            string filePathInArchive,
            UInt64 dataOffset,
            UInt32 extractedSize,
            IReadOnlyList<UInt32> chunkSizes,
            NefsItemType type)
        {
            var size = new NefsItemSize(extractedSize, chunkSizes);
            var dataSource = new NefsFileDataSource(@"C:\source.txt", dataOffset, size, extractedSize != chunkSizes.LastOrDefault());
            return new NefsItem(
                new NefsItemId(id),
                fileName,
                filePathInArchive,
                new NefsItemId(dirId),
                new NefsItemId(siblingId),
                type,
                dataSource,
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
