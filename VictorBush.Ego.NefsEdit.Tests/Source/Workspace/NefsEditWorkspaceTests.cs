// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Tests.Workspace
{
    using System.IO.Abstractions.TestingHelpers;
    using System.Threading.Tasks;
    using Moq;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Tests.Services;
    using VictorBush.Ego.NefsEdit.Tests.Source;
    using VictorBush.Ego.NefsEdit.Workspace;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    public class NefsEditWorkspaceTests
    {
        private readonly MockFileSystem fileSystem = new MockFileSystem();
        private readonly IProgressService progressService = new InvisibleProgressService();
        private readonly Mock<IUiService> uiServiceMock = new Mock<IUiService>();
        private readonly Mock<INefsReader> nefsReaderMock = new Mock<INefsReader>();
        private readonly Mock<INefsWriter> nefsWriterMock = new Mock<INefsWriter>();

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
            const string filePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(filePath);

            var w = this.CreateWorkspace();
            var opened = false;
            w.ArchiveOpened += (o, e) => opened = true;

            // File exists
            this.fileSystem.AddFile(filePath, new MockFileData("hi"));

            // Mock read archive
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(filePath, It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);

            // Test
            var result = await w.OpenArchiveAsync(filePath);

            // Verify
            Assert.True(result);
            Assert.Same(archive, w.Archive);
            Assert.True(opened);
            Assert.Equal(filePath, w.ArchiveFilePath);
        }

        private NefsEditWorkspace CreateWorkspace()
        {
            return new NefsEditWorkspace(
                this.fileSystem,
                this.progressService,
                this.uiServiceMock.Object,
                this.nefsReaderMock.Object,
                this.nefsWriterMock.Object);
        }
    }
}
