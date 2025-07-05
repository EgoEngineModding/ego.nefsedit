// See LICENSE.txt for license information.

using System.Text;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Tests.DataSource;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.TestArchives;

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

	public static uint Dir1DirectoryId => Dir1ItemId;

	public static uint Dir1ItemId => 1;

	public static string Dir1Name => "dir1";

	public static string Dir1PathInArchive => Dir1Name;

	public static uint Dir1SiblingId => Dir1ItemId;

	/*
	File 1 - in root of archive.
	*/

	public static IReadOnlyList<uint> File1ChunkSizes { get; } = new List<uint> { 10, 20, 30 };

	public static uint File1DirectoryId => File1ItemId;

	public static uint File1ExtractedSize => 0x21000;

	public static uint File1ItemId => 0;

	public static string File1Name => "file1.txt";

	public static ulong File1Offset => NefsHeader200.DefaultBlockSize;

	public static string File1PathInArchive => File1Name;

	public static uint File1SiblingId => Dir1ItemId;

	/*
	File 2 - inside directory "dir1".
	*/

	public static IReadOnlyList<uint> File2ChunkSizes { get; } = new List<uint> { 5, 15, 25 };

	public static uint File2DirectoryId => Dir1ItemId;

	public static uint File2ExtractedSize => 0x24000;

	public static uint File2ItemId => 2;

	public static string File2Name => "file2.txt";

	public static ulong File2Offset => File1Offset + File1ChunkSizes.Last();

	public static string File2PathInArchive => $@"{Dir1PathInArchive}\{File2Name}";

	public static uint File2SiblingId => File3ItemId;

	/*
	File 3 - Not compressed. In directory "dir1".
	*/

	public static IReadOnlyList<uint> File3ChunkSizes { get; } = new List<uint> { 31 };

	public static uint File3DirectoryId => Dir1ItemId;

	public static uint File3ExtractedSize => 31;

	public static uint File3ItemId => 3;

	public static string File3Name => "file3.txt";

	public static ulong File3Offset => File2Offset + File2ChunkSizes.Last();

	public static string File3PathInArchive => $@"{Dir1PathInArchive}\{File3Name}";

	public static uint File3SiblingId => File3ItemId;

	/*
	File 4 - No blocks. In directory "dir1".
	*/

	public static uint File4DirectoryId => Dir1ItemId;

	public static uint File4ExtractedSize => 41;

	public static uint File4ItemId => 4;

	public static string File4Name => "file4.txt";

	public static ulong File4Offset => File3Offset + File3ChunkSizes.Last();

	public static string File4PathInArchive => $@"{Dir1PathInArchive}\{File3Name}";

	public static uint NumItems => 5;

	/// <summary>
	/// Creates a test archive. Does not write an archive to disk. Just creates a <see cref="NefsArchive"/> object.
	/// </summary>
	/// <param name="filePath">The file path to use for the archive.</param>
	/// <returns>A <see cref="NefsArchive"/>.</returns>
	public static NefsArchive Create(string filePath)
	{
		var items = new NefsItemList(filePath);
		var aesString = "44927647059D3D73CDCC8D4C6E808538CAD7622D076A507E16C43A8DD8E3B5AB";

		var file1Attributes = new NefsItemAttributes(v20IsZlib: true);
		var file1Chunks = NefsDataChunk.CreateChunkList(File1ChunkSizes, TestHelpers.TestTransform);
		var file1DataSource = new NefsItemListDataSource(items, (long)File1Offset, new NefsItemSize(File1ExtractedSize, file1Chunks));
		var file1 = new NefsItem(new NefsItemId(File1ItemId), File1Name, new NefsItemId(File1DirectoryId), file1DataSource, TestHelpers.TestTransform, file1Attributes);
		items.Add(file1);

		var dir1Attributes = new NefsItemAttributes(isDirectory: true);
		var dir1DataSource = new NefsEmptyDataSource();
		var dir1 = new NefsItem(new NefsItemId(Dir1ItemId), Dir1Name, new NefsItemId(Dir1DirectoryId), dir1DataSource, null, dir1Attributes);
		items.Add(dir1);

		var file2Attributes = new NefsItemAttributes(v20IsZlib: true);
		var file2Chunks = NefsDataChunk.CreateChunkList(File2ChunkSizes, TestHelpers.TestTransform);
		var file2DataSource = new NefsItemListDataSource(items, (long)File2Offset, new NefsItemSize(File2ExtractedSize, file2Chunks));
		var file2 = new NefsItem(new NefsItemId(File2ItemId), File2Name, new NefsItemId(File2DirectoryId), file2DataSource, TestHelpers.TestTransform, file2Attributes);
		items.Add(file2);

		var file3Attributes = new NefsItemAttributes();
		var file3Transform = new NefsDataTransform(File3ExtractedSize);
		var file3Chunks = NefsDataChunk.CreateChunkList(File3ChunkSizes, file3Transform);
		var file3DataSource = new NefsItemListDataSource(items, (long)File3Offset, new NefsItemSize(File3ExtractedSize, file3Chunks));
		var file3 = new NefsItem(new NefsItemId(File3ItemId), File3Name, new NefsItemId(File3DirectoryId), file3DataSource, file3Transform, file3Attributes);
		items.Add(file3);

		var file4Attributes = new NefsItemAttributes(v20IsZlib: true);
		var file4DataSource = new NefsItemListDataSource(items, (long)File4Offset, new NefsItemSize(File4ExtractedSize));
		var file4 = new NefsItem(new NefsItemId(File4ItemId), File4Name, new NefsItemId(File4DirectoryId), file4DataSource, null, file4Attributes);
		items.Add(file4);

		Assert.Equal((int)NumItems, items.Count);

		var aesKeyBuffer = new AesKeyHexBuffer();
		Encoding.ASCII.GetBytes(aesString).CopyTo(aesKeyBuffer);
		var intro = new NefsTocHeaderA160
		{
			AesKey = aesKeyBuffer
		};

		var builder = new NefsHeaderBuilder200();
		var header = builder.Build(new NefsHeader200 { Intro = intro }, items, new NefsProgress());

		return new NefsArchive(header, items);
	}
}
