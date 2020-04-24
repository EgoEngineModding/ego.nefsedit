// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions.TestingHelpers;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    public class NefsTransformerTests
    {
        /// <summary>
        /// Input data string. 100 characters long. Gets encoded to ASCII for test.
        /// </summary>
        private const string InputDataString = @"One two three four five six seven eight nine ten! One two three four five six seven eight nine ten!#";

        private static readonly CompressAsyncTestData CompressAsyncTest1 = new CompressAsyncTestData
        {
            Offset = 0,
            Length = 100,
            ChunkSize = 100,
            ExpectedData = new byte[] { 243, 207, 75, 85, 40, 41, 207, 87, 40, 201, 40, 74, 77, 85, 72, 203, 47, 45, 82, 72, 203, 44, 75, 85, 40, 206, 172, 80, 40, 78, 45, 75, 205, 83, 72, 205, 76, 207, 40, 81, 200, 203, 4, 41, 76, 205, 83, 84, 240, 39, 85, 135, 50, 0, },
            ExpectedChunks = new List<uint> { 54 },
        };

        private static readonly CompressAsyncTestData CompressAsyncTest2 = new CompressAsyncTestData
        {
            Offset = 0,
            Length = 100,
            ChunkSize = 50,
            ExpectedData = new byte[] { 21, 202, 203, 17, 0, 16, 16, 68, 193, 84, 158, 136, 68, 49, 216, 203, 170, 98, 125, 194, 87, 250, 220, 217, 69, 156, 78, 180, 33, 81, 250, 26, 20, 219, 98, 218, 101, 106, 203, 145, 213, 22, 184, 253, 40, 79, 60, 21, 202, 193, 13, 0, 32, 8, 4, 176, 85, 206, 56, 144, 83, 156, 202, 7, 19, 64, 116, 124, 99, 223, 109, 74, 196, 89, 136, 105, 36, 250, 218, 134, 46, 73, 184, 92, 56, 147, 10, 202, 152, 1, 149, 31, 169, 165, 62, },
            ExpectedChunks = new List<uint> { 47, 95 },
        };

        private static readonly CompressAsyncTestData CompressAsyncTest3 = new CompressAsyncTestData
        {
            Offset = 0,
            Length = 100,
            ChunkSize = 40,
            ExpectedData = new byte[] { 5, 193, 193, 17, 0, 32, 8, 3, 176, 85, 186, 145, 83, 20, 225, 3, 119, 128, 232, 248, 38, 203, 137, 190, 129, 214, 36, 33, 113, 18, 98, 67, 148, 61, 20, 135, 14, 218, 214, 198, 7, 203, 203, 204, 75, 85, 40, 73, 205, 83, 84, 240, 7, 49, 202, 243, 21, 74, 50, 138, 82, 83, 21, 210, 242, 75, 139, 20, 210, 50, 203, 82, 21, 138, 51, 43, 20, 138, 83, 1, 43, 75, 205, 83, 72, 205, 76, 207, 40, 81, 200, 203, 204, 75, 85, 40, 73, 205, 83, 84, 6, 0, },
            ExpectedChunks = new List<uint> { 41, 80, 102 },
        };

        private static readonly CompressAsyncTestData CompressAsyncTest4 = new CompressAsyncTestData
        {
            Offset = 0,
            Length = 100,
            ChunkSize = 150,
            ExpectedData = new byte[] { 243, 207, 75, 85, 40, 41, 207, 87, 40, 201, 40, 74, 77, 85, 72, 203, 47, 45, 82, 72, 203, 44, 75, 85, 40, 206, 172, 80, 40, 78, 45, 75, 205, 83, 72, 205, 76, 207, 40, 81, 200, 203, 4, 41, 76, 205, 83, 84, 240, 39, 85, 135, 50, 0, },
            ExpectedChunks = new List<uint> { 54 },
        };

        private static readonly CompressAsyncTestData CompressAsyncTest5 = new CompressAsyncTestData
        {
            Offset = 0,
            Length = 50,
            ChunkSize = 100,
            ExpectedData = new byte[] { 21, 202, 203, 17, 0, 16, 16, 68, 193, 84, 158, 136, 68, 49, 216, 203, 170, 98, 125, 194, 87, 250, 220, 217, 69, 156, 78, 180, 33, 81, 250, 26, 20, 219, 98, 218, 101, 106, 203, 145, 213, 22, 184, 253, 40, 79, 60, },
            ExpectedChunks = new List<uint> { 47 },
        };

        private static readonly CompressAsyncTestData CompressAsyncTest6 = new CompressAsyncTestData
        {
            Offset = 50,
            Length = 100,
            ChunkSize = 100,
            ExpectedData = new byte[] { 21, 202, 193, 13, 0, 32, 8, 4, 176, 85, 206, 56, 144, 83, 156, 202, 7, 19, 64, 116, 124, 99, 223, 109, 74, 196, 89, 136, 105, 36, 250, 218, 134, 46, 73, 184, 92, 56, 147, 10, 202, 152, 1, 149, 31, 169, 165, 62, },
            ExpectedChunks = new List<uint> { 48 },
        };

        private static readonly CompressAsyncTestData CompressAsyncTest7 = new CompressAsyncTestData
        {
            Offset = 50,
            Length = 50,
            ChunkSize = 50,
            ExpectedData = new byte[] { 21, 202, 193, 13, 0, 32, 8, 4, 176, 85, 206, 56, 144, 83, 156, 202, 7, 19, 64, 116, 124, 99, 223, 109, 74, 196, 89, 136, 105, 36, 250, 218, 134, 46, 73, 184, 92, 56, 147, 10, 202, 152, 1, 149, 31, 169, 165, 62, },
            ExpectedChunks = new List<uint> { 48 },
        };

        private static readonly CompressAsyncTestData CompressAsyncTest8 = new CompressAsyncTestData
        {
            Offset = 50,
            Length = 45,
            ChunkSize = 20,
            ExpectedData = new byte[] { 243, 207, 75, 85, 40, 41, 207, 87, 40, 201, 40, 74, 77, 85, 72, 203, 47, 45, 82, 72, 3, 0, 203, 44, 75, 85, 40, 206, 172, 80, 40, 78, 45, 75, 205, 83, 72, 205, 76, 207, 40, 81, 0, 0, 203, 203, 204, 75, 85, 0, 0, },
            ExpectedChunks = new List<uint> { 22, 44, 51 },
        };

        private readonly MockFileSystem fileSystem = new MockFileSystem();

        /// <summary>
        /// Test data.
        /// </summary>
        public static IEnumerable<object[]> CompressAsyncTests =>
            new List<object[]>
            {
                   new object[] { CompressAsyncTest1 },
                   new object[] { CompressAsyncTest2 },
                   new object[] { CompressAsyncTest3 },
                   new object[] { CompressAsyncTest4 },
                   new object[] { CompressAsyncTest5 },
                   new object[] { CompressAsyncTest6 },
                   new object[] { CompressAsyncTest7 },
                   new object[] { CompressAsyncTest8 },
            };

        [Fact]
        public async Task DetransformFileAsync_NotEncrypted_DataDecompressed()
        {
            const string Data = @"Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.
Hello. This is the input data. It is not encrypted.";

            var sourceFilePath = @"C:\source.txt";
            var compressedFilePath = @"C:\compressed.dat";
            var destFilePath = @"C:\dest.txt";
            var chunkSize = 0x10000U;
            var transform = new NefsDataTransform(chunkSize, true);

            this.fileSystem.AddFile(sourceFilePath, new MockFileData(Data));

            // Compress the source data
            var transformer = new NefsTransformer(this.fileSystem);
            var size = await transformer.TransformFileAsync(sourceFilePath, compressedFilePath, transform, new NefsProgress());

            // Decompress the data
            await transformer.DetransformFileAsync(compressedFilePath, 0, destFilePath, 0, size.Chunks, new NefsProgress());

            // Verify
            var decompressedText = this.fileSystem.File.ReadAllText(destFilePath);
            Assert.Equal(Data, decompressedText);
        }

        [MemberData(nameof(CompressAsyncTests))]
        [Theory]
        public async Task TransformAsync_VariousData_DataCompressed(CompressAsyncTestData test)
        {
            var input = Encoding.ASCII.GetBytes(InputDataString);

            using (var inputStream = new MemoryStream(input))
            using (var outputStream = new MemoryStream())
            {
                var transformer = new NefsTransformer(this.fileSystem);
                var transform = new NefsDataTransform(test.ChunkSize, true);
                var size = await transformer.TransformAsync(inputStream, test.Offset, test.Length, outputStream, 0, transform, new NefsProgress());

                // Read data from output stream
                var resultData = new byte[outputStream.Length];
                outputStream.Seek(0, SeekOrigin.Begin);
                await outputStream.ReadAsync(resultData, 0, (int)outputStream.Length);

                // Verify
                Assert.Equal(test.Length, size.ExtractedSize);
                Assert.Equal(test.ExpectedChunks.Count, size.Chunks.Count);
                Assert.True(test.ExpectedChunks.SequenceEqual(size.Chunks.Select(c => c.CumulativeSize)));
                Assert.Equal(test.ExpectedData.Length, resultData.Length);
                Assert.True(test.ExpectedData.SequenceEqual(resultData));
            }
        }

        private string PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }

            sb.Append("}");
            return sb.ToString();
        }

        public class CompressAsyncTestData
        {
            public UInt32 ChunkSize { get; set; }

            public List<UInt32> ExpectedChunks { get; set; } = new List<uint>();

            public byte[] ExpectedData { get; set; }

            public UInt32 Length { get; set; }

            public Int64 Offset { get; set; }
        }
    }
}
