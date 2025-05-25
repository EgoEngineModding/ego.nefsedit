// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Item;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.TestArchives;

internal class TestArchiveNoItems
{
	/// <summary>
	/// Creates a test archive. Does not write an archive to disk. Just creates a <see cref="NefsArchive"/> object.
	/// </summary>
	/// <param name="filePath">The file path to use for the archive.</param>
	/// <returns>A <see cref="NefsArchive"/>.</returns>
	public static NefsArchive Create(string filePath)
	{
		var items = new NefsItemList(filePath);

		Assert.Empty(items.EnumerateById());
		Assert.Empty(items.EnumerateDepthFirstByName());

		var intro = new Nefs160HeaderIntro(new Nefs160TocHeaderA()) { NumberOfItems = (uint)items.Count };
		var toc = new Nefs20HeaderIntroToc();
		var part3 = new NefsHeaderPart3(items);
		var part4 = new Nefs200HeaderBlockTable(items);
		var part1 = new Nefs160HeaderEntryTable(items, part4);
		var part2 = new Nefs160HeaderSharedEntryInfoTable(items, part3);
		var part5 = new NefsHeaderPart5();
		var part6 = new Nefs20HeaderPart6(items);
		var part7 = new Nefs160HeaderWriteableSharedEntryInfo(items);
		var part8 = new Nefs160HeaderHashDigestTable([]);
		var header = new Nefs200Header(intro, toc, part1, part2, part3, part4, part5, part6, part7, part8);

		return new NefsArchive(header, items);
	}
}
