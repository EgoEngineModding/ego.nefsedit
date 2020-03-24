// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.Header
{
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Tests.TestArchives;
    using Xunit;

    public class NefsHeaderTests
    {
        [Fact]
        public void GetItemFilePath_DirectoryInRoot_PathReturned()
        {
            var archive = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var path = archive.Header.GetItemFilePath(new NefsItemId(TestArchiveNotModified.Dir1ItemId));
            Assert.Equal("dir1", path);
        }

        [Fact]
        public void GetItemFilePath_FileInDir_PathReturned()
        {
            var archive = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var path = archive.Header.GetItemFilePath(new NefsItemId(TestArchiveNotModified.File2ItemId));
            Assert.Equal(@"dir1\file2.txt", path);
        }

        [Fact]
        public void GetItemFilePath_FileInRoot_PathReturned()
        {
            var archive = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var path = archive.Header.GetItemFilePath(new NefsItemId(TestArchiveNotModified.File1ItemId));
            Assert.Equal("file1.txt", path);
        }
    }
}
