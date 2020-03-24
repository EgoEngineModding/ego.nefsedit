// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Tests.Workspace
{
    using System.IO;
    using System.IO.Abstractions.TestingHelpers;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Moq;
    using VictorBush.Ego.NefsEdit.Commands;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Tests.Services;
    using VictorBush.Ego.NefsEdit.Tests.Source;
    using VictorBush.Ego.NefsEdit.Workspace;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    public class NefsEditWorkspaceTests
    {
        private readonly MockFileSystem fileSystem = new MockFileSystem();
        private readonly Mock<INefsCompressor> nefsCompressorMock = new Mock<INefsCompressor>();
        private readonly Mock<INefsReader> nefsReaderMock = new Mock<INefsReader>();
        private readonly Mock<INefsWriter> nefsWriterMock = new Mock<INefsWriter>();
        private readonly IProgressService progressService = new InvisibleProgressService();
        private readonly Mock<ISettingsService> settingsServiceMcok = new Mock<ISettingsService>();
        private readonly Mock<IUiService> uiServiceMock = new Mock<IUiService>();

        [Fact]
        public async Task CloseArchiveAsync_ArchiveModifiedAndUserCancels_ArchiveNotClosed()
        {
            var w = this.CreateWorkspace();
            var closed = false;
            w.ArchiveClosed += (o, e) => closed = true;

            // Open an archive
            var archivePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);
            this.fileSystem.AddFile(archivePath, new MockFileData("hi"));
            await w.OpenArchiveAsync(archivePath);

            Assert.Same(archive, w.Archive);
            Assert.False(w.ArchiveIsModified);

            // Modify archvie
            var itemId = new NefsItemId(0);
            var item = w.Archive.Items[itemId];
            var cmd = new RemoveFileCommand(item, item.State);
            w.Execute(cmd);

            // Mock user cancelling the request to save before closing
            this.MockMessageBox(DialogResult.Cancel);

            // Close the archive
            var result = await w.CloseArchiveAsync();

            Assert.False(result);
            Assert.False(closed);
            Assert.Same(archive, w.Archive);
            Assert.True(w.ArchiveIsModified);
            Assert.Equal(archivePath, w.ArchiveSource.DataFilePath);
            Assert.Equal(archivePath, w.ArchiveSource.HeaderFilePath);
        }

        [Fact]
        public async Task CloseArchiveAsync_ArchiveModifiedAndUserDoesNotSave_ArchiveClosedWithoutSaving()
        {
            var w = this.CreateWorkspace();
            var closed = false;
            w.ArchiveClosed += (o, e) => closed = true;

            // Open an archive
            var archivePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);
            this.fileSystem.AddFile(archivePath, new MockFileData("hi"));
            await w.OpenArchiveAsync(archivePath);

            Assert.Same(archive, w.Archive);
            Assert.False(w.ArchiveIsModified);

            // Modify archvie
            var itemId = new NefsItemId(0);
            var item = w.Archive.Items[itemId];
            var cmd = new RemoveFileCommand(item, item.State);
            w.Execute(cmd);

            // Mock user choosing NOT to save before closing
            this.MockMessageBox(DialogResult.No);

            // Close the archive
            var result = await w.CloseArchiveAsync();

            Assert.True(result);
            Assert.True(closed);
            Assert.Null(w.Archive);
            Assert.False(w.ArchiveIsModified);
            Assert.Null(w.ArchiveSource);

            // Verify not saved
            this.nefsWriterMock.Verify(
                n => n.WriteArchiveAsync(
                It.IsAny<string>(),
                It.IsAny<NefsArchive>(),
                It.IsAny<NefsProgress>()), Times.Never());
        }

        [Fact]
        public async Task CloseArchiveAsync_ArchiveModifiedAndUserSavesFail_ArchiveSavedAndClosed()
        {
            var w = this.CreateWorkspace();
            var closed = false;
            w.ArchiveClosed += (o, e) => closed = true;

            // Open an archive
            var archivePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);
            this.fileSystem.AddFile(archivePath, new MockFileData("hi"));
            await w.OpenArchiveAsync(archivePath);

            Assert.Same(archive, w.Archive);
            Assert.False(w.ArchiveIsModified);

            // Modify archvie
            var itemId = new NefsItemId(0);
            var item = w.Archive.Items[itemId];
            var cmd = new RemoveFileCommand(item, item.State);
            w.Execute(cmd);

            // Mock user choosing to save before closing
            this.MockMessageBox(DialogResult.Yes);

            // Mock the save to fail
            this.nefsWriterMock.Setup(n => n.WriteArchiveAsync(
                It.IsAny<string>(),
                It.IsAny<NefsArchive>(),
                It.IsAny<NefsProgress>()))
                .ThrowsAsync(new IOException());

            // Close the archive
            var result = await w.CloseArchiveAsync();

            Assert.False(result);
            Assert.False(closed);
            Assert.Same(archive, w.Archive);
            Assert.True(w.ArchiveIsModified);
            Assert.Equal(archivePath, w.ArchiveSource.DataFilePath);
            Assert.Equal(archivePath, w.ArchiveSource.HeaderFilePath);

            // Verify writer was called
            this.nefsWriterMock.Verify(
                n => n.WriteArchiveAsync(
                It.IsAny<string>(),
                It.IsAny<NefsArchive>(),
                It.IsAny<NefsProgress>()), Times.Once());
        }

        [Fact]
        public async Task CloseArchiveAsync_ArchiveModifiedAndUserSavesSuccess_ArchiveSavedAndClosed()
        {
            var w = this.CreateWorkspace();
            var closed = false;
            w.ArchiveClosed += (o, e) => closed = true;

            // Open an archive
            var archivePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);
            this.fileSystem.AddFile(archivePath, new MockFileData("hi"));
            await w.OpenArchiveAsync(archivePath);

            Assert.Same(archive, w.Archive);
            Assert.False(w.ArchiveIsModified);

            // Modify archvie
            var itemId = new NefsItemId(0);
            var item = w.Archive.Items[itemId];
            var cmd = new RemoveFileCommand(item, item.State);
            w.Execute(cmd);

            // Mock user choosing to save before closing
            this.MockMessageBox(DialogResult.Yes);

            // Mock the save to success
            this.nefsWriterMock.Setup(n => n.WriteArchiveAsync(
                It.IsAny<string>(),
                It.IsAny<NefsArchive>(),
                It.IsAny<NefsProgress>())).ReturnsAsync(archive);

            // Close the archive
            var result = await w.CloseArchiveAsync();

            Assert.True(result);
            Assert.True(closed);
            Assert.Null(w.Archive);
            Assert.False(w.ArchiveIsModified);
            Assert.Null(w.ArchiveSource);

            // Verify writer was called
            this.nefsWriterMock.Verify(
                n => n.WriteArchiveAsync(
                It.IsAny<string>(),
                It.IsAny<NefsArchive>(),
                It.IsAny<NefsProgress>()), Times.Once());
        }

        [Fact]
        public async Task CloseArchiveAsync_ArchiveNotModified_ArchiveClosed()
        {
            var w = this.CreateWorkspace();
            var closed = false;
            w.ArchiveClosed += (o, e) => closed = true;

            // Open an archive
            var archivePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);
            this.fileSystem.AddFile(archivePath, new MockFileData("hi"));
            await w.OpenArchiveAsync(archivePath);

            Assert.Same(archive, w.Archive);
            Assert.False(w.ArchiveIsModified);

            // Close the archive
            var result = await w.CloseArchiveAsync();

            Assert.True(result);
            Assert.True(closed);
            Assert.Null(w.Archive);
            Assert.False(w.ArchiveIsModified);
            Assert.Null(w.ArchiveSource);
        }

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
        public async Task OpenArchiveAsync_ArchiveOpenAndModified_OldArchiveSaved()
        {
            var w = this.CreateWorkspace();
            var closed = false;
            w.ArchiveClosed += (o, e) => closed = true;
            var opened = false;
            w.ArchiveOpened += (o, e) => opened = true;

            // Open an archive
            var archivePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);
            this.fileSystem.AddFile(archivePath, new MockFileData("hi"));
            await w.OpenArchiveAsync(archivePath);

            Assert.Same(archive, w.Archive);
            Assert.False(w.ArchiveIsModified);

            // Modify archvie
            var itemId = new NefsItemId(0);
            var item = w.Archive.Items[itemId];
            var cmd = new RemoveFileCommand(item, item.State);
            w.Execute(cmd);

            // Mock user choosing to save before closing
            this.MockMessageBox(DialogResult.Yes);

            // Mock the save to success
            this.nefsWriterMock.Setup(n => n.WriteArchiveAsync(
                It.IsAny<string>(),
                It.IsAny<NefsArchive>(),
                It.IsAny<NefsProgress>())).ReturnsAsync(archive);

            // Open another archive
            var archive2Path = @"C:\archive.nefs";
            var archive2 = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive2);
            var result = await w.OpenArchiveAsync(archive2Path);

            Assert.True(result);
            Assert.True(closed);
            Assert.True(opened);
            Assert.Same(archive2, w.Archive);
            Assert.False(w.ArchiveIsModified);
            Assert.Equal(archive2Path, w.ArchiveSource.DataFilePath);
            Assert.Equal(archive2Path, w.ArchiveSource.HeaderFilePath);

            // Verify writer was called
            this.nefsWriterMock.Verify(
                n => n.WriteArchiveAsync(
                It.IsAny<string>(),
                It.IsAny<NefsArchive>(),
                It.IsAny<NefsProgress>()), Times.Once());
        }

        [Fact]
        public async Task OpenArchiveAsync_ArchiveOpenButNotModified_OldArchiveClosed()
        {
            var w = this.CreateWorkspace();
            var closed = false;
            w.ArchiveClosed += (o, e) => closed = true;
            var opened = false;
            w.ArchiveOpened += (o, e) => opened = true;

            // Open an archive
            var archivePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);
            this.fileSystem.AddFile(archivePath, new MockFileData("hi"));
            await w.OpenArchiveAsync(archivePath);

            Assert.Same(archive, w.Archive);
            Assert.False(w.ArchiveIsModified);

            // Open another archive
            var archive2Path = @"C:\archive.nefs";
            var archive2 = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive2);
            var result = await w.OpenArchiveAsync(archive2Path);

            Assert.True(result);
            Assert.True(closed);
            Assert.True(opened);
            Assert.Same(archive2, w.Archive);
            Assert.False(w.ArchiveIsModified);
            Assert.Equal(archive2Path, w.ArchiveSource.DataFilePath);
            Assert.Equal(archive2Path, w.ArchiveSource.HeaderFilePath);

            // Verify writer was not called
            this.nefsWriterMock.Verify(
                n => n.WriteArchiveAsync(
                It.IsAny<string>(),
                It.IsAny<NefsArchive>(),
                It.IsAny<NefsProgress>()), Times.Never());
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
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);

            // Test
            var result = await w.OpenArchiveAsync(filePath);

            // Verify
            Assert.True(result);
            Assert.Same(archive, w.Archive);
            Assert.True(opened);
            Assert.Equal(filePath, w.ArchiveSource.DataFilePath);
            Assert.Equal(filePath, w.ArchiveSource.HeaderFilePath);
        }

        [Fact]
        public async Task SaveArchiveAsync_NoArchiveOpen_NotSaved()
        {
            var w = this.CreateWorkspace();
            var saved = false;
            w.ArchiveSaved += (o, e) => saved = true;

            // Save archive
            var result = await w.SaveArchiveAsync(@"C:\archive.nefs");

            Assert.False(result);
            Assert.False(saved);
            Assert.Null(w.Archive);
            Assert.False(w.ArchiveIsModified);
            Assert.Null(w.ArchiveSource);

            // Verify writer not called
            this.nefsWriterMock.Verify(
                n =>
                n.WriteArchiveAsync(
                       It.IsAny<string>(),
                       It.IsAny<NefsArchive>(),
                       It.IsAny<NefsProgress>()), Times.Never());
        }

        [Fact]
        public async Task SaveArchiveAsync_SaveFailed_NotSaved()
        {
            var w = this.CreateWorkspace();
            var saved = false;
            w.ArchiveSaved += (o, e) => saved = true;

            // Open an archive
            var archivePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);
            this.fileSystem.AddFile(archivePath, new MockFileData("hi"));
            await w.OpenArchiveAsync(archivePath);

            Assert.Same(archive, w.Archive);
            Assert.False(w.ArchiveIsModified);

            // Modify archvie
            var itemId = new NefsItemId(0);
            var item = w.Archive.Items[itemId];
            var cmd = new RemoveFileCommand(item, item.State);
            w.Execute(cmd);

            // Mock save falied
            this.nefsWriterMock.Setup(n =>
                n.WriteArchiveAsync(
                    It.IsAny<string>(),
                    It.IsAny<NefsArchive>(),
                    It.IsAny<NefsProgress>()))
                .ThrowsAsync(new IOException());

            // Save archive
            var result = await w.SaveArchiveAsync(archivePath);

            Assert.False(result);
            Assert.False(saved);
            Assert.Same(archive, w.Archive);
            Assert.True(w.ArchiveIsModified);
            Assert.Equal(archivePath, w.ArchiveSource.DataFilePath);
            Assert.Equal(archivePath, w.ArchiveSource.HeaderFilePath);
        }

        [Fact]
        public async Task SaveArchiveAsync_SaveSuccess_Saved()
        {
            var w = this.CreateWorkspace();
            var saved = false;
            w.ArchiveSaved += (o, e) => saved = true;

            // Open an archive
            var archivePath = @"C:\archive.nefs";
            var archive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsReaderMock.Setup(r => r.ReadArchiveAsync(It.IsAny<NefsArchiveSource>(), It.IsAny<NefsProgress>()))
                .ReturnsAsync(archive);
            this.fileSystem.AddFile(archivePath, new MockFileData("hi"));
            await w.OpenArchiveAsync(archivePath);

            Assert.Same(archive, w.Archive);
            Assert.False(w.ArchiveIsModified);

            // Mock save success
            var savedArchive = TestHelpers.CreateTestArchive(archivePath);
            this.nefsWriterMock.Setup(n =>
                n.WriteArchiveAsync(
                    It.IsAny<string>(),
                    It.IsAny<NefsArchive>(),
                    It.IsAny<NefsProgress>()))
                .ReturnsAsync(savedArchive);

            // Save archive
            var result = await w.SaveArchiveAsync(archivePath);

            Assert.True(result);
            Assert.True(saved);
            Assert.Same(savedArchive, w.Archive);
            Assert.False(w.ArchiveIsModified);
            Assert.Equal(archivePath, w.ArchiveSource.DataFilePath);
            Assert.Equal(archivePath, w.ArchiveSource.HeaderFilePath);
        }

        private NefsEditWorkspace CreateWorkspace()
        {
            return new NefsEditWorkspace(
                this.fileSystem,
                this.progressService,
                this.uiServiceMock.Object,
                this.settingsServiceMcok.Object,
                this.nefsReaderMock.Object,
                this.nefsWriterMock.Object,
                this.nefsCompressorMock.Object);
        }

        private void MockMessageBox(DialogResult result)
        {
            this.uiServiceMock.Setup(u => u.ShowMessageBox(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MessageBoxButtons>(),
                It.IsAny<MessageBoxIcon>())).Returns(result);
        }
    }
}
