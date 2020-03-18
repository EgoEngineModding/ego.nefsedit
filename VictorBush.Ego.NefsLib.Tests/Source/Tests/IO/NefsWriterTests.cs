// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.IO
{
    using System;
    using System.IO;
    using System.IO.Abstractions.TestingHelpers;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Header;
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
            var sourcePath = @"C:\archive.nefs";
            var destPath = @"C:\dest.nefs";
            this.fileSystem.AddFile(sourcePath, new MockFileData("hi"));
            var sourceArchive = TestArchiveNotModified.Create(sourcePath);
            var writer = this.CreateWriter();
            var archive = await writer.WriteArchiveAsync(@"C:\dest.nefs", sourceArchive, new NefsProgress());

            Assert.Equal(sourceArchive.Items.Count, archive.Items.Count);

            // Try to read archive again
            var reader = new NefsReader(this.fileSystem);
            var readArchive = await reader.ReadArchiveAsync(destPath, new NefsProgress());
            Assert.Equal(sourceArchive.Items.Count, readArchive.Items.Count);
        }

        [Fact]
        public async Task WriterHeaderIntroAsync_ValidData_Written()
        {
            var aes = new byte[] { 0xE5, 0x69, 0x65, 0x23, 0xAB, 0xF5, 0x43, 0xFF, 0xC9, 0xDF, 0xB2, 0x2C, 0x64, 0xD1, 0x11, 0x46, 0xE5, 0x9B, 0xAC, 0xC8, 0xAC, 0x8B, 0xA4, 0x15, 0x9E, 0xE0, 0xE2, 0xBB, 0x54, 0x09, 0x0A, 0x6C, 0x99, 0x30, 0xC6, 0xC1, 0x84, 0x3C, 0x90, 0x29, 0x75, 0xB2, 0xB5, 0x5E, 0x3B, 0x7A, 0x06, 0x3D, 0xE1, 0xD2, 0x1F, 0x6F, 0xB7, 0xDC, 0x57, 0x5A, 0xC4, 0x4F, 0x84, 0xCB, 0x13, 0x87, 0xAB, 0xBF };
            var hash = new byte[] { 0xCB, 0x13, 0x87, 0xAB, 0xBF, 0xD5, 0x45, 0x93, 0x34, 0x0A, 0x50, 0xC1, 0xA8, 0x0A, 0x82, 0x53, 0xF9, 0xD5, 0x46, 0xDA, 0x24, 0xDA, 0xA4, 0xDA, 0x82, 0xEA, 0x9A, 0xB5, 0xBC, 0xD8, 0x6B, 0xFC };

            var intro = new NefsHeaderIntro();
            intro.AesKey.Value = aes;
            intro.ExpectedHash.Value = hash;
            intro.HeaderSize.Value = 12345;
            intro.NumberOfItems.Value = 9876;
            intro.OffsetToPart1.Value = 111;
            intro.OffsetToPart2.Value = 222;
            intro.OffsetToPart3.Value = 333;
            intro.OffsetToPart4.Value = 444;
            intro.OffsetToPart5.Value = 555;
            intro.OffsetToPart6.Value = 666;
            intro.OffsetToPart7.Value = 777;
            intro.OffsetToPart8.Value = 888;
            intro.Unknown0x68.Value = 101;
            intro.Unknown0x70zlib.Value = 202;
            intro.Unknown0x78.Value = 303;
            intro.Unknown0x80.Value = 404;

            // This chunk of data is unknown, but it must be 0x5C bytes long
            intro.Unknown0xa4.Value = new byte[0x5C];
            intro.Unknown0xa4.Value[0] = 20;

            var writer = this.CreateWriter();
            byte[] buffer;

            using (var ms = new MemoryStream())
            {
                await writer.WriteHeaderIntroAsync(ms, 0U, intro, new NefsProgress());
                buffer = ms.ToArray();
            }

            /*
            Verify
            */

            // Magic #
            Assert.Equal(0x5346654EU, BitConverter.ToUInt32(buffer, 0));

            // Expected hash
            this.VerifyArrraySlice(hash, 0, buffer, 0x04, 0x20);

            // AES key
            this.VerifyArrraySlice(aes, 0, buffer, 0x24, 0x40);

            // Header size
            Assert.Equal(12345, BitConverter.ToInt32(buffer, 0x64));

            // 0x68 unknwon
            Assert.Equal(101, BitConverter.ToInt32(buffer, 0x68));

            // Number of items
            Assert.Equal(9876, BitConverter.ToInt32(buffer, 0x6C));

            // 0x70 unknown (8 bytes)
            Assert.Equal(202, BitConverter.ToInt64(buffer, 0x70));

            // 0x78 unknwon (8 bytes)
            Assert.Equal(303, BitConverter.ToInt64(buffer, 0x78));

            // 0x80 unknwon
            Assert.Equal(404, BitConverter.ToInt32(buffer, 0x80));

            // Offset to part 1
            Assert.Equal(111, BitConverter.ToInt32(buffer, 0x84));

            // Offset to part 6
            Assert.Equal(666, BitConverter.ToInt32(buffer, 0x88));

            // Offset to part 2
            Assert.Equal(222, BitConverter.ToInt32(buffer, 0x8C));

            // Offset to part 7
            Assert.Equal(777, BitConverter.ToInt32(buffer, 0x90));

            // Offset to part 3
            Assert.Equal(333, BitConverter.ToInt32(buffer, 0x94));

            // Offset to part 4
            Assert.Equal(444, BitConverter.ToInt32(buffer, 0x98));

            // Offset to part 5
            Assert.Equal(555, BitConverter.ToInt32(buffer, 0x9C));

            // Offset to part 8
            Assert.Equal(888, BitConverter.ToInt32(buffer, 0xA0));

            // 0xa4 Unknown
            Assert.Equal(20, buffer[0xa4]);
        }

        private NefsWriter CreateWriter()
        {
            return new NefsWriter(TempDir, this.fileSystem, this.compressor);
        }

        private void VerifyArrraySlice(
            byte[] expectedBuffer,
            int expectedOffset,
            byte[] actualBuffer,
            int actualOffset,
            int length)
        {
            var expected = expectedOffset;
            var actual = actualOffset;

            for (; actual < length; ++actual, ++expected)
            {
                Assert.Equal(expectedBuffer[expected], actualBuffer[actual]);
            }
        }
    }
}
