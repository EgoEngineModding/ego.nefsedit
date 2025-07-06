// See LICENSE.txt for license information.

using System.IO.Abstractions.TestingHelpers;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Tests;

internal static class TestHelpers
{
	/// <summary>
	/// The path to the test data file for data type tests. The file is created with the <see cref="DataTypesTestData"/>
	/// and put on the mock file system.
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
		var attributes = new NefsItemAttributes(isDirectory: true);

		return new NefsItem(
			new NefsItemId(id),
			fileName,
			new NefsItemId(dirId),
			new NefsEmptyDataSource(),
			null,
			attributes);
	}

	/// <summary>
	/// Creates an item for testing.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <param name="dirId">The directory id.</param>
	/// <param name="fileName">The item name.</param>
	/// <param name="dataSource">The data source.</param>
	/// <param name="firstDuplicateId">The first duplicate id.</param>
	/// <returns>The new item.</returns>
	internal static NefsItem CreateFile(
		uint id,
		uint dirId,
		string fileName,
		INefsDataSource dataSource,
		uint? firstDuplicateId = null)
	{
		var attributes = new NefsItemAttributes(v20IsZlib: true);

		var transform = TestTransform;
		return new NefsItem(
			new NefsItemId(id),
			new NefsItemId(firstDuplicateId ?? id)
,			fileName,
			new NefsItemId(dirId),
			dataSource,
			transform,
			attributes);
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
		ulong dataOffset,
		uint extractedSize,
		IReadOnlyList<uint> chunkSizes,
		NefsItemType type)
	{
		var attributes = new NefsItemAttributes(
			isDirectory: type == NefsItemType.Directory)
			{ IsTransformed = true };

		var transform = TestTransform;
		var chunks = NefsDataChunk.CreateChunkList(chunkSizes, transform);
		var size = new NefsItemSize(extractedSize, chunks);
		var dataSource = new NefsFileDataSource(@"C:\source.txt", (long)dataOffset, size, extractedSize != chunkSizes.LastOrDefault());
		return new NefsItem(
			new NefsItemId(id),
			fileName,
			new NefsItemId(dirId),
			dataSource,
			transform,
			attributes);
	}

	internal static NefsVolumeSource[] Source(this NefsArchive nefs)
	{
		var dataOffset = nefs.Header.Volumes.Count > 0 ? nefs.Header.Volumes[0].DataOffset : 0;
		return [new NefsVolumeSource(nefs.Items.DataFilePath, dataOffset, nefs.Header.SplitSize)];
	}
}
