// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Item;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.TestArchives;

/// <summary>
/// <para>Test archive that has modified items in it.</para>
/// <list>
/// <item>/file1 (Replaced)</item>
/// <item>/dir1 (Not modified)</item>
/// <item>/dir1/file2 (Not modified)</item>
/// <item>/dir1/file3 (Replaced)</item>
/// <item>/dir1/file4 (Removed)</item>
/// </list>
/// </summary>
internal class TestArchiveModified
{
	/*
	Directory 1 - in root of archive.
	Not modified.
	*/

	public static uint Dir1DirectoryId => Dir1ItemId;

	public static Guid Dir1Guid { get; } = Guid.NewGuid();

	public static uint Dir1ItemId => 1;

	public static string Dir1Name => "dir1";

	public static string Dir1PathInArchive => Dir1Name;

	public static uint Dir1SiblingId => Dir1ItemId;

	/*
	File 1 - in root of archive.
	Modified - file replaced.
	*/

	public static IReadOnlyList<uint> File1ChunkSizes { get; } = new List<uint> { 10, 20, 30 };

	public static uint File1DirectoryId => File1ItemId;

	public static uint File1ExtractedSize => 0x27000;

	public static Guid File1Guid { get; } = Guid.NewGuid();

	public static uint File1ItemId => 0;

	public static string File1Name => "file1.txt";

	public static ulong File1Offset => Nefs20Header.DataOffsetDefault;

	public static string File1PathInArchive => File1Name;

	public static string File1ReplacementFile => @"C:\file1replace.txt";

	public static uint File1SiblingId => Dir1ItemId;

	/*
	File 2 - inside directory "dir1".
	Not modified.
	*/

	public static IReadOnlyList<uint> File2ChunkSizes { get; } = new List<uint> { 5, 15, 25 };

	public static uint File2DirectoryId => Dir1ItemId;

	public static uint File2ExtractedSize => 0x26000;

	public static Guid File2Guid { get; } = Guid.NewGuid();

	public static uint File2ItemId => 2;

	public static string File2Name => "file2.txt";

	public static ulong File2Offset => File1Offset + File1ChunkSizes.Last();

	public static string File2PathInArchive => $@"{Dir1PathInArchive}\{File2Name}";

	public static uint File2SiblingId => File3ItemId;

	/*
	File 3 - Not compressed. In directory "dir1".
	Modified - file replaced.
	*/

	public static IReadOnlyList<uint> File3ChunkSizes { get; } = new List<uint> { 31 };

	public static uint File3DirectoryId => Dir1ItemId;

	public static uint File3ExtractedSize => 31;

	public static Guid File3Guid { get; } = Guid.NewGuid();

	public static uint File3ItemId => 3;

	public static string File3Name => "file3.txt";

	public static ulong File3Offset => File2Offset + File2ChunkSizes.Last();

	public static string File3PathInArchive => $@"{Dir1PathInArchive}\{File3Name}";

	public static string File3ReplacementFile => @"C:\file3replace.txt";

	public static uint File3SiblingId => File4ItemId;

	/*
	File 4 - In directory "dir1".
	Modified - file removed.
	*/

	public static IReadOnlyList<uint> File4ChunkSizes { get; } = new List<uint> { 5, 10, 15 };

	public static uint File4DirectoryId => Dir1ItemId;

	public static uint File4ExtractedSize => 0x29000;

	public static Guid File4Guid { get; } = Guid.NewGuid();

	public static uint File4ItemId => 4;

	public static string File4Name => "file4.txt";

	public static ulong File4Offset => File3Offset + File3ChunkSizes.Last();

	public static string File4PathInArchive => $@"{Dir1PathInArchive}\{File4Name}";

	public static uint File4SiblingId => File4ItemId;

	public static uint NumItems => 5;

	/// <summary>
	/// Creates a test archive. Does not write an archive to disk. Just creates a <see cref="NefsArchive"/> object.
	/// </summary>
	/// <param name="filePath">The file path to use for the archive.</param>
	/// <returns>A <see cref="NefsArchive"/>.</returns>
	public static NefsArchive Create(string filePath)
	{
		var items = new NefsItemList(filePath);

		var file1Attributes = new NefsItemAttributes(v20IsZlib: true);
		var file1Chunks = NefsDataChunk.CreateChunkList(File1ChunkSizes, TestHelpers.TestTransform);
		var file1DataSource = new NefsItemListDataSource(items, (long)File1Offset, new NefsItemSize(File1ExtractedSize, file1Chunks));
		var file1 = new NefsItem(File1Guid, new NefsItemId(File1ItemId), File1Name, new NefsItemId(File1DirectoryId), file1DataSource, TestHelpers.TestTransform, file1Attributes);
		items.Add(file1);

		var dir1Attributes = new NefsItemAttributes(isDirectory: true);
		var dir1DataSource = new NefsEmptyDataSource();
		var dir1 = new NefsItem(Dir1Guid, new NefsItemId(Dir1ItemId), Dir1Name, new NefsItemId(Dir1DirectoryId), dir1DataSource, null, dir1Attributes);
		items.Add(dir1);

		var file2Attributes = new NefsItemAttributes(v20IsZlib: true);
		var file2Chunks = NefsDataChunk.CreateChunkList(File2ChunkSizes, TestHelpers.TestTransform);
		var file2DataSource = new NefsItemListDataSource(items, (long)File2Offset, new NefsItemSize(File2ExtractedSize, file2Chunks));
		var file2 = new NefsItem(File2Guid, new NefsItemId(File2ItemId), File2Name, new NefsItemId(File2DirectoryId), file2DataSource, TestHelpers.TestTransform, file2Attributes);
		items.Add(file2);

		var file3Attributes = new NefsItemAttributes(v20IsZlib: true);
		var file3Chunks = NefsDataChunk.CreateChunkList(File3ChunkSizes, TestHelpers.TestTransform);
		var file3DataSource = new NefsItemListDataSource(items, (long)File3Offset, new NefsItemSize(File3ExtractedSize, file3Chunks));
		var file3 = new NefsItem(File3Guid, new NefsItemId(File3ItemId), File3Name, new NefsItemId(File3DirectoryId), file3DataSource, TestHelpers.TestTransform, file3Attributes);
		items.Add(file3);

		Assert.Equal((int)NumItems, items.Count);

		var intro = new NefsHeaderIntro
		{
			NumberOfItems = (uint)items.Count,
		};

		var toc = new Nefs20HeaderIntroToc();
		var part3 = new NefsHeaderPart3(items);
		var part4 = new Nefs20HeaderPart4(items);
		var part1 = new NefsHeaderPart1(items, part4);
		var part2 = new NefsHeaderPart2(items, part3);
		var part5 = new NefsHeaderPart5();
		var part6 = new Nefs20HeaderPart6(items);
		var part7 = new NefsHeaderPart7(items);
		var part8 = new NefsHeaderPart8(0);
		var header = new Nefs20Header(intro, toc, part1, part2, part3, part4, part5, part6, part7, part8);

		return new NefsArchive(header, items);
	}
}
