// See LICENSE.txt for license information.

using System.IO.Abstractions.TestingHelpers;
using System.Text;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

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
	public async Task DetransformAndTransform_SingleChunk_Valid()
	{
		byte[] input =
		{
				0xB3, 0xB1, 0xAF, 0xC8, 0xCD, 0x51, 0x28, 0x4B, 0x2D, 0x2A, 0xCE, 0xCC,
				0xCF, 0xB3, 0x55, 0x32, 0xD4, 0x33, 0x50, 0x52, 0x48, 0xCD, 0x4B, 0xCE,
				0x4F, 0xC9, 0xCC, 0x4B, 0xB7, 0x55, 0x0A, 0x0D, 0x71, 0xD3, 0xB5, 0x50,
				0x52, 0xB0, 0xB7, 0xE3, 0xE5, 0xB2, 0x49, 0x2F, 0xCA, 0x2C, 0x88, 0x2F,
				0xC9, 0xCC, 0x05, 0x4A, 0x14, 0x2B, 0x80, 0x38, 0xC1, 0x25, 0xA9, 0xA9,
				0x45, 0x40, 0x9E, 0x4B, 0x69, 0x51, 0x62, 0x09, 0x58, 0xB7, 0x81, 0x9E,
				0xA1, 0xA9, 0x01, 0x10, 0x28, 0x29, 0x14, 0xA5, 0xE6, 0xA4, 0x26, 0x16,
				0xA7, 0xE2, 0x53, 0x02, 0x32, 0xC2, 0x3D, 0x35, 0xB1, 0x08, 0x45, 0xCE,
				0x00, 0x45, 0x3B, 0x86, 0xB4, 0x11, 0x42, 0xAB, 0x47, 0x62, 0x5E, 0x4A,
				0x52, 0x51, 0x62, 0x76, 0x2A, 0x1E, 0xFD, 0xD8, 0xD5, 0xC0, 0x0C, 0x01,
				0x1A, 0x1E, 0x92, 0x99, 0x9B, 0x0A, 0x12, 0x33, 0x36, 0x40, 0x88, 0x85,
				0x16, 0x60, 0x13, 0x75, 0xC9, 0x2F, 0xCF, 0x43, 0x17, 0xCF, 0x80, 0x19,
				0x8F, 0x53, 0x22, 0xA0, 0x34, 0x27, 0x07, 0x26, 0x69, 0x86, 0x64, 0x5A,
				0x48, 0xBE, 0x13, 0xB2, 0x3E, 0x23, 0x68, 0x90, 0xE8, 0xDB, 0x01, 0x00,
			};

		var expectedOutput = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<grip_timings gripSteeringDuration=""0.150000"" releaseSteeringDuration=""0.150000"" gripGearDuration=""0.100000"" releaseGearDuration=""0.120000"" gripHandbrakeDuration=""0.100000"" releaseHandbrakeDuration=""0.120000"" gearTime=""0.300000"" gearUpTime=""0.300000"" gearDownTime=""0.300000"" handbrakeTime=""0.300000"" handbrakePullTime=""0.600000"" gearToBrakeTime=""0.250000"" />";

		const int chunkSize = 0x10000;
		const int transformedSize = 0xA8;
		const int extractedSize = 0x18F;

		// Detransform
		using (var inputStream = new MemoryStream(input))
		using (var outputStream = new MemoryStream())
		{
			var transformer = new NefsTransformer(this.fileSystem);
			var transform = new NefsDataTransform(chunkSize, true);

			var chunk = new NefsDataChunk(transformedSize, transformedSize, transform);
			var size = await transformer.DetransformChunkAsync(inputStream, outputStream, chunk, extractedSize, new NefsProgress());

			// Read data from output stream
			var resultData = new byte[outputStream.Length];
			outputStream.Seek(0, SeekOrigin.Begin);
			await outputStream.ReadAsync(resultData, 0, (int)outputStream.Length);

			var xmlString = Encoding.ASCII.GetString(resultData);

			// Verify
			Assert.Equal(expectedOutput, xmlString);
			Assert.Equal(extractedSize, resultData.Length);
		}

		// Transform
		using (var inputStream = new MemoryStream(Encoding.ASCII.GetBytes(expectedOutput)))
		using (var outputStream = new MemoryStream())
		{
			var transformer = new NefsTransformer(this.fileSystem);
			var transform = new NefsDataTransform(chunkSize, true);

			var chunk = new NefsDataChunk(transformedSize, transformedSize, transform);
			var size = await transformer.TransformChunkAsync(inputStream, extractedSize, outputStream, transform, new NefsProgress());

			// Read data from output stream
			var resultData = new byte[outputStream.Length];
			outputStream.Seek(0, SeekOrigin.Begin);
			await outputStream.ReadAsync(resultData, 0, (int)outputStream.Length);

			// Verify
			Assert.True(input.SequenceEqual(resultData));
			Assert.Equal(transformedSize, resultData.Length);
		}
	}

	[Fact]
	public async Task DetransformAsync_ExtractedSizeSmallerThanTransformed_DataExtracted()
	{
		// There are situations in version 1.6 headers where the extracted size is smaller than the compressed size,
		// resulting in extra garbage data/padding at the end of an extracted file. Need to make sure this extra garbage
		// data is ignored.
		const string Data = "Hello there!";
		var dataBytes = Encoding.ASCII.GetBytes(Data);

		var aesStr = "542E5211BD8A3AE494554DA4A18884B1C546258BCCA4B76D055D52602819525A";
		var aes = StringHelper.FromHexString(aesStr);
		var chunkSize = 0x10000U;
		var transform = new NefsDataTransform(chunkSize, false, aes);
		var transformer = new NefsTransformer(this.fileSystem);

		using (var inputStream = new MemoryStream())
		using (var transformedStream = new MemoryStream())
		using (var outputStream = new MemoryStream())
		{
			// Copy data to input stream
			inputStream.Write(dataBytes, 0, dataBytes.Length);

			// Add some garbage data to end of stream
			await transformedStream.WriteAsync(Encoding.ASCII.GetBytes("HAHAHAHAHA"), 0, 10);

			// Transform
			await transformer.TransformAsync(inputStream, 0, (uint)dataBytes.Length, transformedStream, 0, transform, new NefsProgress());
			transformedStream.Seek(0, SeekOrigin.Begin);

			// Setup chunk info
			var extractedSize = Data.Length;
			var transformedSize = transformedStream.Length;
			var chunk = new NefsDataChunk((uint)transformedSize, (uint)transformedSize, transform);
			var chunks = new List<NefsDataChunk> { chunk };

			// Extract
			await transformer.DetransformAsync(transformedStream, 0, outputStream, 0, (uint)extractedSize, chunks, new NefsProgress());
			outputStream.Seek(0, SeekOrigin.Begin);

			var outputBytes = new byte[Data.Length];
			await outputStream.ReadAsync(outputBytes, 0, (int)outputStream.Length);
			var outputStr = Encoding.ASCII.GetString(outputBytes);

			// Verify
			Assert.Equal(extractedSize, outputStream.Length);
			Assert.Equal(Data, outputStr);
		}
	}

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
		await transformer.DetransformFileAsync(compressedFilePath, 0, destFilePath, 0, (uint)Data.Length, size.Chunks, new NefsProgress());

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
		public uint ChunkSize { get; set; }

		public List<uint> ExpectedChunks { get; set; } = new List<uint>();

		public byte[] ExpectedData { get; set; }

		public uint Length { get; set; }

		public long Offset { get; set; }
	}
}
