// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Builder;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Tests.TestArchives;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.Item;

public class NefsItemTests
{
	[Fact]
	public void Clone_ItemCloned()
	{
		var nefs = TestArchiveNotModified.Create(@"C:\archive.nefs");
		var builder = new Nefs200ItemListBuilder((Nefs200Header)nefs.Header, NefsLog.GetLogger());
		var item = builder.BuildItem(TestArchiveNotModified.File3ItemId, nefs.Items);
		item.UpdateState(NefsItemState.Replaced);

		var clone = item with {};

		Assert.Equal(item.CompressedSize, clone.CompressedSize);
		Assert.Same(item.DataSource, clone.DataSource);
		Assert.Equal(item.DirectoryId, clone.DirectoryId);
		Assert.Equal(item.ExtractedSize, clone.ExtractedSize);
		Assert.Equal(item.FileName, clone.FileName);
		Assert.Equal(item.Id, clone.Id);
		Assert.Equal(item.State, clone.State);
		Assert.Equal(item.Type, clone.Type);
	}
}
