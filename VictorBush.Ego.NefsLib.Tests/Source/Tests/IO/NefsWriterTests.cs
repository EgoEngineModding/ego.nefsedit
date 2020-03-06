// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.IO
{
    using System.IO.Abstractions.TestingHelpers;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Progress;
    using VictorBush.Ego.NefsLib.Tests.TestArchives;
    using Xunit;

    public class NefsWriterTests
    {
        private const string TempDir = @"C:\temp";
        private readonly INefsCompressor compressor;
        private readonly MockFileSystem fileSystem = new MockFileSystem();

        public NefsWriterTests()
        {
            this.fileSystem.AddDirectory(TempDir);
            this.compressor = new NefsCompressor(this.fileSystem);
        }

        [Fact]
        public async Task WriteArchiveAsync_ArchiveNotModified_ArchiveWritten()
        {
            var sourceArchive = TestArchiveNotModified.Create(@"C:\archive.nefs");
            var writer = this.CreateWriter();
            var archive = await writer.WriteArchiveAsync(@"C:\dest.nefs", sourceArchive, new NefsProgress());

            Assert.Equal(sourceArchive.Items.Count, archive.Items.Count);
        }

        private NefsWriter CreateWriter()
        {
            return new NefsWriter(TempDir, this.fileSystem, this.compressor);
        }
    }
}
