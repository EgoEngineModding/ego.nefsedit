// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions.TestingHelpers;
    using System.Text;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Item;
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
        public async Task WriteHeaderPart1Async_ValidData_Written()
        {
            var items = new NefsItemList(@"C:\hi.txt");
            var file1 = TestHelpers.CreateItem(0, 0, "file1", 10, 11, new List<UInt32> { 12, 13 }, NefsItemType.File);
            var file2 = TestHelpers.CreateItem(1, 1, "file2", 20, 21, new List<UInt32> { 22, 23 }, NefsItemType.File);
            var dir1 = TestHelpers.CreateItem(2, 2, "dir1", 0, 0, new List<UInt32> { 0 }, NefsItemType.Directory);
            items.Add(file1);
            items.Add(file2);
            items.Add(dir1);

            var part4 = new NefsHeaderPart4(items);
            var part1 = new NefsHeaderPart1(items, part4);

            /*
            Write
            */

            var writer = this.CreateWriter();
            byte[] buffer;
            var offset = 5;

            using (var ms = new MemoryStream())
            {
                await writer.WriteHeaderPart1Async(ms, (uint)offset, part1, new NefsProgress());
                buffer = ms.ToArray();
            }

            /*
            Verify
            */

            /*
            file1
            */

            // Data offset (8 bytes)
            Assert.Equal(10, BitConverter.ToInt64(buffer, offset + 0));

            // Index to part 2
            Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 8));

            // Index to part 4
            Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x0c));

            // Item id
            Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x10));

            /*
            file2
            */

            offset += (int)NefsHeaderPart1Entry.Size;

            // Data offset (8 bytes)
            Assert.Equal(20, BitConverter.ToInt64(buffer, offset + 0));

            // Index to part 2
            Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 8));

            // Index to part 4
            Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0x0c));

            // Item id
            Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 0x10));

            /*
            dir1
            */

            offset += (int)NefsHeaderPart1Entry.Size;

            // Data offset (8 bytes)
            Assert.Equal(0, BitConverter.ToInt64(buffer, offset + 0));

            // Index to part 2
            Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 8));

            // Index to part 4
            Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x0c));

            // Item id
            Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0x10));
        }

        [Fact]
        public async Task WriteHeaderPart2Async_ValidData_Written()
        {
            var items = new NefsItemList(@"C:\hi.txt");
            var file1 = TestHelpers.CreateItem(0, 0, "file1", 10, 11, new List<UInt32> { 12, 13 }, NefsItemType.File);
            var file2 = TestHelpers.CreateItem(1, 1, "file2", 20, 21, new List<UInt32> { 22, 23 }, NefsItemType.File);
            var dir1 = TestHelpers.CreateItem(2, 2, "dir1", 0, 0, new List<UInt32> { 0 }, NefsItemType.Directory);
            var file3 = TestHelpers.CreateItem(3, 2, "file3", 30, 31, new List<UInt32> { 32, 33 }, NefsItemType.File);
            items.Add(file1);
            items.Add(file2);
            items.Add(dir1);
            items.Add(file3);

            var part3 = new NefsHeaderPart3(items);
            var part2 = new NefsHeaderPart2(items, part3);

            /*
            Write
            */

            var writer = this.CreateWriter();
            byte[] buffer;
            var offset = 5;

            using (var ms = new MemoryStream())
            {
                await writer.WriteHeaderPart2Async(ms, (uint)offset, part2, new NefsProgress());
                buffer = ms.ToArray();
            }

            /*
            Verify
            */

            /*
            dir1
            */

            // Dir id
            Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0));

            // First child id
            Assert.Equal(3, BitConverter.ToInt32(buffer, offset + 0x04));

            // Part 3 offset
            Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x08));

            // Extracted size
            Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x0c));

            // Item id
            Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0x10));

            /*
            file3
            */

            offset += (int)NefsHeaderPart2Entry.Size;

            // Dir id
            Assert.Equal(2, BitConverter.ToInt32(buffer, offset + 0));

            // First child id
            Assert.Equal(3, BitConverter.ToInt32(buffer, offset + 0x04));

            // Part 3 offset
            Assert.Equal(17, BitConverter.ToInt32(buffer, offset + 0x08));

            // Extracted size
            Assert.Equal(31, BitConverter.ToInt32(buffer, offset + 0x0c));

            // Item id
            Assert.Equal(3, BitConverter.ToInt32(buffer, offset + 0x10));

            /*
            file1
            */

            offset += (int)NefsHeaderPart2Entry.Size;

            // Dir id
            Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0));

            // First child id
            Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x04));

            // Part 3 offset
            Assert.Equal(5, BitConverter.ToInt32(buffer, offset + 0x08));

            // Extracted size
            Assert.Equal(11, BitConverter.ToInt32(buffer, offset + 0x0c));

            // Item id
            Assert.Equal(0, BitConverter.ToInt32(buffer, offset + 0x10));

            /*
            file2
            */

            offset += (int)NefsHeaderPart2Entry.Size;

            // Dir id
            Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 0));

            // First child id
            Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 0x04));

            // Part 3 offset
            Assert.Equal(11, BitConverter.ToInt32(buffer, offset + 0x08));

            // Extracted size
            Assert.Equal(21, BitConverter.ToInt32(buffer, offset + 0x0c));

            // Item id
            Assert.Equal(1, BitConverter.ToInt32(buffer, offset + 0x10));
        }

        [Fact]
        public async Task WriteHeaderPart3Async_ValidData_Written()
        {
            var items = new NefsItemList(@"C:\hi.txt");
            var file1 = TestHelpers.CreateItem(0, 0, "file1", 10, 11, new List<UInt32> { 12, 13 }, NefsItemType.File);
            var file2 = TestHelpers.CreateItem(1, 1, "file2", 20, 21, new List<UInt32> { 22, 23 }, NefsItemType.File);
            var dir1 = TestHelpers.CreateItem(2, 2, "dir1", 0, 0, new List<UInt32> { 0 }, NefsItemType.Directory);
            var file3 = TestHelpers.CreateItem(3, 2, "file3", 30, 31, new List<UInt32> { 32, 33 }, NefsItemType.File);
            items.Add(file1);
            items.Add(file2);
            items.Add(dir1);
            items.Add(file3);

            var part3 = new NefsHeaderPart3(items);

            /*
            Write
            */

            var writer = this.CreateWriter();
            byte[] buffer;
            var offset = 5;

            using (var ms = new MemoryStream())
            {
                await writer.WriteHeaderPart3Async(ms, (uint)offset, part3, new NefsProgress());
                buffer = ms.ToArray();
            }

            /*
            Verify
            */

            // Null terminated strings
            Assert.Equal("dir1", Encoding.ASCII.GetString(buffer, offset + 0, 4));
            Assert.Equal(0, buffer[offset + 4]);
            Assert.Equal("file1", Encoding.ASCII.GetString(buffer, offset + 5, 5));
            Assert.Equal(0, buffer[offset + 10]);
            Assert.Equal("file2", Encoding.ASCII.GetString(buffer, offset + 11, 5));
            Assert.Equal(0, buffer[offset + 16]);
            Assert.Equal("file3", Encoding.ASCII.GetString(buffer, offset + 17, 5));
            Assert.Equal(0, buffer[offset + 22]);
        }

        [Fact]
        public async Task WriteHeaderPart4Async_ValidData_Written()
        {
            var items = new NefsItemList(@"C:\hi.txt");
            var file1 = TestHelpers.CreateItem(0, 0, "file1", 10, 11, new List<UInt32> { 12, 13 }, NefsItemType.File);
            var file2 = TestHelpers.CreateItem(1, 1, "file2", 20, 21, new List<UInt32> { 22, 23 }, NefsItemType.File);
            var dir1 = TestHelpers.CreateItem(2, 2, "dir1", 0, 0, new List<UInt32> { 0 }, NefsItemType.Directory);
            var file3 = TestHelpers.CreateItem(3, 2, "file3", 30, 31, new List<UInt32> { 32, 33 }, NefsItemType.File);
            items.Add(file1);
            items.Add(file2);
            items.Add(dir1);
            items.Add(file3);

            var part4 = new NefsHeaderPart4(items);

            /*
            Write
            */

            var writer = this.CreateWriter();
            byte[] buffer;
            var offset = 5;

            using (var ms = new MemoryStream())
            {
                await writer.WriteHeaderPart4Async(ms, (uint)offset, part4, new NefsProgress());
                buffer = ms.ToArray();
            }

            /*
            Verify
            */

            Assert.Equal(28 + offset, buffer.Length);
            Assert.Equal(12, BitConverter.ToInt32(buffer, offset + 0));
            Assert.Equal(13, BitConverter.ToInt32(buffer, offset + 4));
            Assert.Equal(22, BitConverter.ToInt32(buffer, offset + 8));
            Assert.Equal(23, BitConverter.ToInt32(buffer, offset + 12));
            Assert.Equal(32, BitConverter.ToInt32(buffer, offset + 16));
            Assert.Equal(33, BitConverter.ToInt32(buffer, offset + 20));

            // Last four bytes - largest compressed size
            Assert.Equal(33, BitConverter.ToInt32(buffer, offset + 24));
        }

        [Fact]
        public async Task WriteHeaderPart5Async_ValidData_Written()
        {
            var part5 = new NefsHeaderPart5();
            part5.ArchiveSize.Value = 1234;
            part5.UnknownData.Value = 5678;

            /*
            Write
            */

            var writer = this.CreateWriter();
            byte[] buffer;
            var offset = 5;

            using (var ms = new MemoryStream())
            {
                await writer.WriteHeaderPart5Async(ms, (uint)offset, part5, new NefsProgress());
                buffer = ms.ToArray();
            }

            /*
            Verify
            */

            Assert.Equal(1234, BitConverter.ToInt64(buffer, offset + 0));
            Assert.Equal(5678, BitConverter.ToInt64(buffer, offset + 8));
        }

        [Fact]
        public async Task WriterHeaderIntroAsync_ValidData_Written()
        {
            var aes = new byte[] { 0xE5, 0x69, 0x65, 0x23, 0xAB, 0xF5, 0x43, 0xFF, 0xC9, 0xDF, 0xB2, 0x2C, 0x64, 0xD1, 0x11, 0x46, 0xE5, 0x9B, 0xAC, 0xC8, 0xAC, 0x8B, 0xA4, 0x15, 0x9E, 0xE0, 0xE2, 0xBB, 0x54, 0x09, 0x0A, 0x6C, 0x99, 0x30, 0xC6, 0xC1, 0x84, 0x3C, 0x90, 0x29, 0x75, 0xB2, 0xB5, 0x5E, 0x3B, 0x7A, 0x06, 0x3D, 0xE1, 0xD2, 0x1F, 0x6F, 0xB7, 0xDC, 0x57, 0x5A, 0xC4, 0x4F, 0x84, 0xCB, 0x13, 0x87, 0xAB, 0xBF };
            var hash = new byte[] { 0xCB, 0x13, 0x87, 0xAB, 0xBF, 0xD5, 0x45, 0x93, 0x34, 0x0A, 0x50, 0xC1, 0xA8, 0x0A, 0x82, 0x53, 0xF9, 0xD5, 0x46, 0xDA, 0x24, 0xDA, 0xA4, 0xDA, 0x82, 0xEA, 0x9A, 0xB5, 0xBC, 0xD8, 0x6B, 0xFC };

            var intro = new NefsHeaderIntro();
            intro.AesKeyHexString.Value = aes;
            intro.ExpectedHash.Value = hash;
            intro.HeaderSize.Value = 12345;
            intro.NumberOfItems.Value = 9876;
            intro.Unknown0x68.Value = 101;
            intro.Unknown0x70zlib.Value = 202;
            intro.Unknown0x78.Value = 303;

            /*
            Write
            */

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
        }

        [Fact]
        public async Task WriterHeaderIntroTocAsync_ValidData_Written()
        {
            var toc = new NefsHeaderIntroToc();
            toc.OffsetToPart1.Value = 111;
            toc.OffsetToPart2.Value = 222;
            toc.OffsetToPart3.Value = 333;
            toc.OffsetToPart4.Value = 444;
            toc.OffsetToPart5.Value = 555;
            toc.OffsetToPart6.Value = 666;
            toc.OffsetToPart7.Value = 777;
            toc.OffsetToPart8.Value = 888;
            toc.Unknown0x00.Value = 404;

            // This chunk of data is unknown, but it must be 0x5C bytes long
            toc.Unknown0x24.Value = new byte[0x5C];
            toc.Unknown0x24.Value[0] = 20;

            /*
            Write
            */

            var writer = this.CreateWriter();
            byte[] buffer;
            var offset = 10;

            using (var ms = new MemoryStream())
            {
                await writer.WriteHeaderIntroTocAsync(ms, (uint)offset, toc, new NefsProgress());
                buffer = ms.ToArray();
            }

            /*
            Verify
            */

            // 0x00 unknwon
            Assert.Equal(404, BitConverter.ToInt32(buffer, offset + 0x00));

            // Offset to part 1
            Assert.Equal(111, BitConverter.ToInt32(buffer, offset + 0x04));

            // Offset to part 6
            Assert.Equal(666, BitConverter.ToInt32(buffer, offset + 0x08));

            // Offset to part 2
            Assert.Equal(222, BitConverter.ToInt32(buffer, offset + 0x0C));

            // Offset to part 7
            Assert.Equal(777, BitConverter.ToInt32(buffer, offset + 0x10));

            // Offset to part 3
            Assert.Equal(333, BitConverter.ToInt32(buffer, offset + 0x14));

            // Offset to part 4
            Assert.Equal(444, BitConverter.ToInt32(buffer, offset + 0x18));

            // Offset to part 5
            Assert.Equal(555, BitConverter.ToInt32(buffer, offset + 0x1C));

            // Offset to part 8
            Assert.Equal(888, BitConverter.ToInt32(buffer, offset + 0x20));

            // 0x24 Unknown
            Assert.Equal(20, buffer[offset + 0x24]);
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
