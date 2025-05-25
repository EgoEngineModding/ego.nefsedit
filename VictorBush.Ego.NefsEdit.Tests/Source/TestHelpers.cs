// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsEdit.Tests;

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
		var file1 = new NefsItem(new NefsItemId(0), "file1", new NefsItemId(0), file1DataSource, transform, file1Attributes);
		items.Add(file1);

		var dir1Attributes = new NefsItemAttributes(isDirectory: true);
		var dir1DataSource = new NefsEmptyDataSource();
		var dir1 = new NefsItem(new NefsItemId(1), "dir1", new NefsItemId(1), dir1DataSource, null, dir1Attributes);
		items.Add(dir1);

		var file2Attributes = new NefsItemAttributes(v20IsZlib: true);
		var file2Chunks = NefsDataChunk.CreateChunkList(new List<UInt32> { 5, 6, 7 }, transform);
		var file2DataSource = new NefsItemListDataSource(items, 104, new NefsItemSize(15, file2Chunks));
		var file2 = new NefsItem(new NefsItemId(2), "file2", dir1.Id, file2DataSource, transform, file2Attributes);
		items.Add(file2);

		var intro = new Nefs160TocHeaderA();
		var toc = new Nefs200TocHeaderB();
		var part3 = new NefsHeaderPart3(items);
		var part4 = new Nefs200HeaderBlockTable(items);
		var part1 = new Nefs160HeaderEntryTable(items, part4);
		var part2 = new Nefs160HeaderSharedEntryInfoTable(items, part3);
		var part5 = new NefsHeaderPart5();
		var part6 = new Nefs160HeaderWriteableEntryTable(items);
		var part7 = new Nefs160HeaderWriteableSharedEntryInfo(items);
		var part8 = new Nefs160HeaderHashDigestTable([]);
		var header = new Nefs200Header(new NefsWriterSettings(), intro, toc, part1, part2, part3, part4, part5, part6, part7, part8);

		return new NefsArchive(header, items);
	}
}
