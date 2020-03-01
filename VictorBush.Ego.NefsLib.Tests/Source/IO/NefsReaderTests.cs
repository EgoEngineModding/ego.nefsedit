// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Threading;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    public class NefsReaderTests
    {
        private readonly MockFileSystem fileSystem = new MockFileSystem();

        private readonly NefsProgress p = new NefsProgress(CancellationToken.None);

        [Fact]
        public async void IntegrationTest_CarArchive()
        {
            var fs = new FileSystem();
            var reader = new NefsReader(fs);
            var path = @"E:\Applications\Steam\steamapps\common\DiRT Rally 2.0\cars\fr5.nefs";
            NefsArchive nefs = null;

            using (var stream = fs.File.OpenRead(path))
            {
                nefs = await reader.ReadArchiveAsync(path, this.p);
            }

            Assert.Equal(98, nefs.Items.Count);
        }

        [Fact]
        public async void IntegrationTest_EncrpytedCarArchive()
        {
            var fs = new FileSystem();
            var reader = new NefsReader(fs);
            var path = @"E:\Applications\Steam\steamapps\common\DiRT Rally 2.0\cars\c4r.nefs";
            NefsArchive nefs = null;

            using (var stream = fs.File.OpenRead(path))
            {
                nefs = await reader.ReadArchiveAsync(path, this.p);
            }

            Assert.Equal(98, nefs.Items.Count);
        }

        [Fact]
        public async void ReadHeaderPart1Async_ExtraBytesAtEnd_ExtraBytesIgnored()
        {
            byte[] bytes =
            {
                // 5 bytes offset
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

                // Entry 1
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x11, 0x12, 0x13, 0x14, 0x21, 0x22, 0x23, 0x24,
                0x31, 0x32, 0x33, 0x34,

                // Extra bytes
                0xFF, 0xFF,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = NefsHeaderPart1Entry.Size + 2;
            var offset = (uint)5;

            // Test
            var part1 = await reader.ReadHeaderPart1Async(stream, offset, size, this.p);

            // Verify
            Assert.Single(part1.EntriesById);

            // Entries are keyed by id
            var e1 = part1.EntriesById[new NefsItemId(0x34333231)];
            Assert.Equal((ulong)0x0807060504030201, e1.OffsetToData.Value);
            Assert.Equal((uint)0x14131211, e1.IndexIntoPart2.Value);
            Assert.Equal((uint)0x24232221, e1.IndexIntoPart4.Value);
            Assert.Equal((uint)0x34333231, e1.Id.Value);
        }

        [Fact]
        public async void ReadHeaderPart1Async_ValidData_DataRead()
        {
            byte[] bytes =
            {
                // 5 bytes offset
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

                // Entry 1
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x11, 0x12, 0x13, 0x14, 0x21, 0x22, 0x23, 0x24,
                0x31, 0x32, 0x33, 0x34,

                // Entry 2
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x51, 0x52, 0x53, 0x54, 0x61, 0x62, 0x63, 0x64,
                0x71, 0x72, 0x73, 0x74,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = 2 * NefsHeaderPart1Entry.Size;
            var offset = (uint)5;

            // Test
            var part1 = await reader.ReadHeaderPart1Async(stream, offset, size, this.p);

            // Verify
            Assert.Equal(2, part1.EntriesById.Count);

            // Entries are keyed by id
            var e1 = part1.EntriesById[new NefsItemId(0x34333231)];
            Assert.Equal((ulong)0x0807060504030201, e1.OffsetToData.Value);
            Assert.Equal((uint)0x14131211, e1.IndexIntoPart2.Value);
            Assert.Equal((uint)0x24232221, e1.IndexIntoPart4.Value);
            Assert.Equal((uint)0x34333231, e1.Id.Value);

            var e2 = part1.EntriesById[new NefsItemId(0x74737271)];
            Assert.Equal((ulong)0x4847464544434241, e2.OffsetToData.Value);
            Assert.Equal((uint)0x54535251, e2.IndexIntoPart2.Value);
            Assert.Equal((uint)0x64636261, e2.IndexIntoPart4.Value);
            Assert.Equal((uint)0x74737271, e2.Id.Value);
        }

        [Fact]
        public async void ReadHeaderPart1Async_OffsetOutOfBounds_NoEntries()
        {
            var bytes = new byte[10];
            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);

            // Test
            var part1 = await reader.ReadHeaderPart1Async(stream, 10, 5, this.p);

            // Verify
            Assert.Empty(part1.EntriesById);
        }

        [Fact]
        public async void ReadHeaderPart1Async_SizeOutOfBounds_NoEntries()
        {
            var bytes = new byte[10];
            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);

            // Test
            var part1 = await reader.ReadHeaderPart1Async(stream, 0, 20, this.p);

            // Verify
            Assert.Empty(part1.EntriesById);
        }

        [Fact]
        public async void ReadHeaderPart1Async_ZeroSize_NoEntries()
        {
            var bytes = new byte[2];
            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);

            // Test
            var part1 = await reader.ReadHeaderPart1Async(stream, 0, 0, this.p);

            // Verify
            Assert.Empty(part1.EntriesById);
        }

        [Fact]
        public async void ReadHeaderPart2Async_ExtraBytesAtEnd_ExtraBytesIgnored()
        {
            byte[] bytes =
            {
                // 5 bytes offset
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

                // Entry 1
                0x01, 0x02, 0x03, 0x04, 0x11, 0x12, 0x13, 0x14,
                0x21, 0x22, 0x23, 0x24, 0x31, 0x32, 0x33, 0x34,
                0x41, 0x42, 0x43, 0x44,

                // Extra bytes
                0xFF, 0xFF,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = NefsHeaderPart2Entry.Size + 2;
            var offset = (uint)5;

            // Test
            var part2 = await reader.ReadHeaderPart2Async(stream, offset, size, this.p);

            // Verify
            Assert.Single(part2.EntriesById);

            // Entries are keyed by id
            var e1 = part2.EntriesById[new NefsItemId(0x44434241)];
            Assert.Equal((uint)0x04030201, e1.DirectoryId.Value);
            Assert.Equal((uint)0x14131211, e1.FirstChildId.Value);
            Assert.Equal((uint)0x24232221, e1.OffsetIntoPart3.Value);
            Assert.Equal((uint)0x34333231, e1.ExtractedSize.Value);
            Assert.Equal((uint)0x44434241, e1.Id.Value);
        }

        [Fact]
        public async void ReadHeaderPart2Async_ValidData_DataRead()
        {
            byte[] bytes =
            {
                // 5 bytes offset
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

                // Entry 1
                0x01, 0x02, 0x03, 0x04, 0x11, 0x12, 0x13, 0x14,
                0x21, 0x22, 0x23, 0x24, 0x31, 0x32, 0x33, 0x34,
                0x41, 0x42, 0x43, 0x44,

                // Entry 2
                0x51, 0x52, 0x53, 0x54, 0x61, 0x62, 0x63, 0x64,
                0x71, 0x72, 0x73, 0x74, 0x81, 0x82, 0x83, 0x84,
                0x91, 0x92, 0x93, 0x94,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = 2 * NefsHeaderPart2Entry.Size;
            var offset = (uint)5;

            // Test
            var part2 = await reader.ReadHeaderPart2Async(stream, offset, size, this.p);

            // Verify
            Assert.Equal(2, part2.EntriesById.Count);

            // Entries are keyed by id
            var e1 = part2.EntriesById[new NefsItemId(0x44434241)];
            Assert.Equal((uint)0x04030201, e1.DirectoryId.Value);
            Assert.Equal((uint)0x14131211, e1.FirstChildId.Value);
            Assert.Equal((uint)0x24232221, e1.OffsetIntoPart3.Value);
            Assert.Equal((uint)0x34333231, e1.ExtractedSize.Value);
            Assert.Equal((uint)0x44434241, e1.Id.Value);

            var e2 = part2.EntriesById[new NefsItemId(0x94939291)];
            Assert.Equal((uint)0x54535251, e2.DirectoryId.Value);
            Assert.Equal((uint)0x64636261, e2.FirstChildId.Value);
            Assert.Equal((uint)0x74737271, e2.OffsetIntoPart3.Value);
            Assert.Equal((uint)0x84838281, e2.ExtractedSize.Value);
            Assert.Equal((uint)0x94939291, e2.Id.Value);
        }

        [Fact]
        public async void ReadHeaderPart3Async_DoubleTerminators_StringsRead()
        {
            byte[] bytes =
            {
                // Offset
                0xFF, 0xFF,

                // Entries
                0x41, 0x42, 0x00,
                0x43, 0x00, 0x00, // Double terminators
                0x45, 0x46, 0x00,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = (uint)9;
            var offset = (uint)2;

            // Test
            var part3 = await reader.ReadHeaderPart3Async(stream, offset, size, this.p);

            // Verify
            Assert.Equal(4, part3.FileNamesByOffset.Count);
            Assert.Equal(4, part3.OffsetsByFileName.Count);
            Assert.Equal("AB", part3.FileNamesByOffset[0]);
            Assert.Equal("C", part3.FileNamesByOffset[3]);
            Assert.Equal("", part3.FileNamesByOffset[5]);
            Assert.Equal("EF", part3.FileNamesByOffset[6]);
        }

        [Fact]
        public async void ReadHeaderPart3Async_NoEndingTerminator_StringsRead()
        {
            byte[] bytes =
            {
                // Offset
                0xFF, 0xFF,

                // Entries
                0x41, 0x42, 0x00,
                0x43, 0x44, 0x00,
                0x45, 0x46, 0x47, // No ending terminator
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = (uint)9;
            var offset = (uint)2;

            // Test
            var part3 = await reader.ReadHeaderPart3Async(stream, offset, size, this.p);

            // Verify
            Assert.Equal(3, part3.FileNamesByOffset.Count);
            Assert.Equal(3, part3.OffsetsByFileName.Count);
            Assert.Equal("AB", part3.FileNamesByOffset[0]);
            Assert.Equal("CD", part3.FileNamesByOffset[3]);
            Assert.Equal("EFG", part3.FileNamesByOffset[6]);
        }

        [Fact]
        public async void ReadHeaderPart3Async_ValidData_StringsRead()
        {
            byte[] bytes =
            {
                // Offset
                0xFF, 0xFF,

                // Entries
                0x41, 0x42, 0x00,
                0x43, 0x44, 0x00,
                0x45, 0x46, 0x00,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = (uint)9;
            var offset = (uint)2;

            // Test
            var part3 = await reader.ReadHeaderPart3Async(stream, offset, size, this.p);

            // Verify
            Assert.Equal(3, part3.FileNamesByOffset.Count);
            Assert.Equal(3, part3.OffsetsByFileName.Count);
            Assert.Equal("AB", part3.FileNamesByOffset[0]);
            Assert.Equal("CD", part3.FileNamesByOffset[3]);
            Assert.Equal("EF", part3.FileNamesByOffset[6]);
        }

        [Fact]
        public async void ReadHeaderPart4Async_ValidData_DataRead()
        {
            // Item 1 has 2 chunk sizes
            var e1p1 = new NefsHeaderPart1Entry();
            e1p1.Id.Value = 0;
            e1p1.IndexIntoPart4.Value = 0;
            var e1p2 = new NefsHeaderPart2Entry();
            e1p2.ExtractedSize.Value = NefsArchive.ChunkSize * 2;

            // Item 2 has 1 chunk size
            var e2p1 = new NefsHeaderPart1Entry();
            e2p1.Id.Value = 1;
            e2p1.IndexIntoPart4.Value = 2;
            var e2p2 = new NefsHeaderPart2Entry();
            e2p2.ExtractedSize.Value = NefsArchive.ChunkSize;

            // Item 3 has no chunks
            var e3p1 = new NefsHeaderPart1Entry();
            e3p1.Id.Value = 2;
            e3p1.IndexIntoPart4.Value = 0xFFFFFFFF;
            var e3p2 = new NefsHeaderPart2Entry();
            e3p2.ExtractedSize.Value = NefsArchive.ChunkSize;

            // Item 4 is a directory (extracted size == 0)
            var e4p1 = new NefsHeaderPart1Entry();
            e4p1.Id.Value = 3;
            e4p1.IndexIntoPart4.Value = 0;
            var e4p2 = new NefsHeaderPart2Entry();
            e4p2.ExtractedSize.Value = 0;

            // Item 5 has 3 chunks
            var e5p1 = new NefsHeaderPart1Entry();
            e5p1.Id.Value = 4;
            e5p1.IndexIntoPart4.Value = 3;
            var e5p2 = new NefsHeaderPart2Entry();
            e5p2.ExtractedSize.Value = (NefsArchive.ChunkSize * 2) + 5;

            var part1Items = new Dictionary<NefsItemId, NefsHeaderPart1Entry>
            {
                { new NefsItemId(0), e1p1 },
                { new NefsItemId(1), e2p1 },
                { new NefsItemId(2), e3p1 },
                { new NefsItemId(3), e4p1 },
                { new NefsItemId(4), e5p1 },
            };

            var part2Items = new Dictionary<NefsItemId, NefsHeaderPart2Entry>
            {
                { new NefsItemId(0), e1p2 },
                { new NefsItemId(1), e2p2 },
                { new NefsItemId(2), e3p2 },
                { new NefsItemId(3), e4p2 },
                { new NefsItemId(4), e5p2 },
            };

            var part1 = new NefsHeaderPart1(part1Items);
            var part2 = new NefsHeaderPart2(part2Items);

            // Setup data
            byte[] bytes =
            {
                // Offset
                0xFF, 0xFF,

                // Item 1
                0x11, 0x12, 0x13, 0x14,
                0x15, 0x16, 0x17, 0x18,

                // Item 2
                0x21, 0x22, 0x23, 0x24,

                // Item 5
                0x31, 0x32, 0x33, 0x34,
                0x35, 0x36, 0x37, 0x38,
                0x39, 0x3A, 0x3B, 0x3C,

                // Last four bytes
                0x01, 0x02, 0x03, 0x04,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = (uint)28;
            var offset = (uint)2;

            // Test
            var part4 = await reader.ReadHeaderPart4Async(stream, offset, size, part1, part2, this.p);

            // Verify
            Assert.Equal(3, part4.EntriesByIndex.Count);

            Assert.Equal((uint)0x14131211, part4.EntriesByIndex[0].ChunkSizes[0]);
            Assert.Equal((uint)0x18171615, part4.EntriesByIndex[0].ChunkSizes[1]);

            Assert.Equal((uint)0x24232221, part4.EntriesByIndex[2].ChunkSizes[0]);

            Assert.Equal((uint)0x34333231, part4.EntriesByIndex[3].ChunkSizes[0]);
            Assert.Equal((uint)0x38373635, part4.EntriesByIndex[3].ChunkSizes[1]);
            Assert.Equal((uint)0x3C3B3A39, part4.EntriesByIndex[3].ChunkSizes[2]);

            Assert.Equal((uint)0x04030201, part4.LastFourBytes);
        }

        [Fact]
        public async void ReadHeaderPart5Async_ValidData_DataRead()
        {
            byte[] bytes =
            {
                // 5 bytes offset
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

                // Archive size
                0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,

                // Unknown data
                0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = (uint)16;
            var offset = (uint)5;

            // Test
            var part5 = await reader.ReadHeaderPart5Async(stream, offset, size, this.p);

            // Verify
            Assert.Equal((UInt64)0x1817161514131211, part5.ArchiveSize.Value);
            Assert.Equal((UInt64)0x2827262524232221, part5.UnknownData.Value);
        }

        [Fact]
        public async void ReadHeaderPart6Async_ValidData_DataRead()
        {
            byte[] bytes =
            {
                // 5 bytes offset
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

                // Entry 1
                0x11, 0x12, 0x13, 0x14,

                // Entry 2
                0x21, 0x22, 0x23, 0x24,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = 2 * NefsHeaderPart6Entry.Size;
            var offset = (uint)5;

            // Test
            var part6 = await reader.ReadHeaderPart6Async(stream, offset, size, this.p);

            // Verify
            Assert.Equal(2, part6.Entries.Count);

            var e1 = part6.Entries[0];
            Assert.Equal(0x11, e1.Byte0.Value[0]);
            Assert.Equal(0x12, e1.Byte1.Value[0]);
            Assert.Equal(0x13, e1.Byte2.Value[0]);
            Assert.Equal(0x14, e1.Byte3.Value[0]);

            var e2 = part6.Entries[1];
            Assert.Equal(0x21, e2.Byte0.Value[0]);
            Assert.Equal(0x22, e2.Byte1.Value[0]);
            Assert.Equal(0x23, e2.Byte2.Value[0]);
            Assert.Equal(0x24, e2.Byte3.Value[0]);
        }

        [Fact]
        public async void ReadHeaderPart7Async_ValidData_DataRead()
        {
            byte[] bytes =
            {
                // 5 bytes offset
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

                // Entry 1
                0x11, 0x12, 0x13, 0x14,
                0x15, 0x16, 0x17, 0x18,

                // Entry 2
                0x21, 0x22, 0x23, 0x24,
                0x25, 0x26, 0x27, 0x28,
            };

            var stream = new MemoryStream(bytes);
            var reader = new NefsReader(this.fileSystem);
            var size = 2 * NefsHeaderPart7Entry.Size;
            var offset = (uint)5;

            // Test
            var part7 = await reader.ReadHeaderPart7Async(stream, offset, size, this.p);

            // Verify
            Assert.Equal(2, part7.Entries.Count);

            var e1 = part7.Entries[0];
            Assert.Equal((uint)0x14131211, e1.Unknown0x00.Value);
            Assert.Equal((uint)0x18171615, e1.Unknown0x04.Value);

            var e2 = part7.Entries[1];
            Assert.Equal((uint)0x24232221, e2.Unknown0x00.Value);
            Assert.Equal((uint)0x28272625, e2.Unknown0x04.Value);
        }
    }
}
