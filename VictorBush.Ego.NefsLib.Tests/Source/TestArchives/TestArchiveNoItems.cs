// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header;
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

		var intro = new NefsHeaderIntro { NumberOfItems = (uint)items.Count };
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
