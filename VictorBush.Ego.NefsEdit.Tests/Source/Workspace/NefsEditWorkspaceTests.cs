// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Tests.Workspace
{
    using System.IO.Abstractions.TestingHelpers;
    using System.Threading.Tasks;
    using Moq;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Tests.Services;
    using VictorBush.Ego.NefsEdit.Workspace;
    using VictorBush.Ego.NefsLib.IO;
    using Xunit;

    public class NefsEditWorkspaceTests
    {
        private readonly MockFileSystem fileSystem = new MockFileSystem();
        private readonly Mock<INefsCompressor> nefsCompressorMock = new Mock<INefsCompressor>();
        private readonly IProgressService progressService = new InvisibleProgressService();
        private readonly Mock<IUiService> uiServiceMock = new Mock<IUiService>();

        [Fact]
        public async Task CloseArchiveAsync_NoArchiveOpen_TrueReturned()
        {
            var w = this.CreateWorkspace();
            var closed = false;
            w.ArchiveClosed += (o, e) => closed = true;

            // Close archive - should return true even though no archive was closed
            Assert.True(await w.CloseArchiveAsync());

            // Verify close event not thrown since no archive was open
            Assert.False(closed);
        }

        [Fact]
        public async Task CloseArchiveAsync_ArchiveNotModified_ArchiveClosed()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task CloseArchiveAsync_ArchiveModifiedAndUserCancels_ArchiveNotClosed()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task CloseArchiveAsync_ArchiveModifiedAndUserSaves_ArchiveSavedAndClosed()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task CloseArchiveAsync_ArchiveModifiedAndUserDoesNotSave_ArchiveClosedWithoutSaving()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task OpenArchiveAsync_FileDoesNotExist_ArchiveNotOpened()
        {
            var w = this.CreateWorkspace();
            var opened = false;
            w.ArchiveOpened += (o, e) => opened = true;

            var result = await w.OpenArchiveAsync(@"C:\archive.nefs");

            Assert.False(result);
            Assert.False(opened);
        }

        [Fact]
        public async Task OpenArchiveAsync_FileExists_ArchiveOpened()
        {
            Assert.True(false);
        }

        private NefsEditWorkspace CreateWorkspace()
        {
            return new NefsEditWorkspace(
                this.fileSystem,
                this.progressService,
                this.uiServiceMock.Object,
                this.nefsCompressorMock.Object);
        }
    }
}
