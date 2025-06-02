// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
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

		var builder = new NefsHeaderBuilder200();
		var header = builder.Build(new Nefs200Header(), items, new NefsProgress());

		return new NefsArchive(header, items);
	}
}
