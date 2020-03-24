// See LICENSE.txt for license information.

// #define ENABLE_INTEGRATION_TESTS
#if ENABLE_INTEGRATION_TESTS

namespace VictorBush.Ego.NefsLib.Tests.Integration
{
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    /// <summary>
    /// Integration tests for testing real archives.
    /// </summary>
    public class DirtRally2Tests
    {
        private const string DirtRally2Path = @"E:\Applications\Steam\steamapps\common\DiRT Rally 2.0\";

        private readonly NefsProgress progress = new NefsProgress();

        [Fact]
        public async Task ReadArchiveAsync_GameDat_Loaded()
        {
            var fs = new FileSystem();
            var reader = new NefsReader(fs);
            var headerPath = Path.Combine(DirtRally2Path, @"dirtrally2.exe");
            var dataPath = Path.Combine(DirtRally2Path, @"game\game.dat");

            var offset = 0x107B9B0;
            var nefs = await reader.ReadArchiveAsync(headerPath, (uint)offset, dataPath, this.progress);

            // Not really verifying anything here, but useful to set breakpoint and inspect objects
            Assert.NotNull(nefs);
        }

        [Fact]
        public async Task ReadArchiveAsync_CarArchive()
        {
            var fs = new FileSystem();
            var reader = new NefsReader(fs);
            var path = Path.Combine(DirtRally2Path, @"cars\fr5.nefs");
            NefsArchive nefs = null;

            using (var stream = fs.File.OpenRead(path))
            {
                nefs = await reader.ReadArchiveAsync(path, this.progress);
            }

            Assert.Equal(98, nefs.Items.Count);
        }

        [Fact]
        public async Task ReadArchiveAsync_EncrpytedCarArchive()
        {
            var fs = new FileSystem();
            var reader = new NefsReader(fs);
            var path = Path.Combine(DirtRally2Path, @"cars\c4r.nefs");
            NefsArchive nefs = null;

            using (var stream = fs.File.OpenRead(path))
            {
                nefs = await reader.ReadArchiveAsync(path, this.progress);
            }

            Assert.Equal(98, nefs.Items.Count);
        }
    }
}

#endif
