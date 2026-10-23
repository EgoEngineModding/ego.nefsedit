// See LICENSE.txt for license information.

using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

public class NefsWriterTests
{
	private const string TempDir = @"C:\temp";
	private readonly MockFileSystem fileSystem = new MockFileSystem();
	private readonly INefsTransformer transformer;

	public NefsWriterTests()
	{
		this.fileSystem.AddDirectory(TempDir);
		this.transformer = new NefsTransformer(this.fileSystem);
	}

	[Fact]
	public async Task EncodeXorIntro()
	{
		var input = Enumerable.Range(-32, 32).ToArray();
		var inputBuffer = MemoryMarshal.Cast<int, byte>(input).ToArray();
		var inputStream = new MemoryStream(inputBuffer);
		var expectedBuffer = new byte[]
		{
			0xEC, 0xFF, 0xFF, 0xFF, 0x0B, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0xEB, 0xFF, 0xFF, 0xFF, 0xE8, 0xFF,
			0xFF, 0xFF, 0xEF, 0xFF, 0xFF, 0xFF, 0xE0, 0xFF, 0xFF, 0xFF, 0x0C, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00,
			0x08, 0x00, 0x00, 0x00, 0xE1, 0xFF, 0xFF, 0xFF, 0x07, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xEA, 0xFF,
			0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x1E, 0x00, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x00,
			0x1C, 0x00, 0x00, 0x00, 0x1D, 0x00, 0x00, 0x00, 0x1A, 0x00, 0x00, 0x00, 0x1B, 0x00, 0x00, 0x00, 0x18, 0x00,
			0x00, 0x00, 0x19, 0x00, 0x00, 0x00, 0x16, 0x00, 0x00, 0x00, 0x17, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00,
			0x15, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0xFF, 0xFF,
			0xFF, 0xFF
		};

		// Act
		await NefsWriter.EncodeXorIntroAsync(inputStream, 0, CancellationToken.None).ConfigureAwait(false);

		// Assert
		var actualBuffer = inputStream.ToArray();
		//var str = string.Join(',', actualBuffer.Select(x => $"0x{x:X2}"));
		Assert.Equal(expectedBuffer, actualBuffer);
	}

	[Fact]
	public async Task WriteArchiveAsync_MultiVolume_CoLocatesVolumesAndPreservesData()
	{
		// Arrange
		const string destDir = @"C:\dest";
		const string destFilePath = @"C:\dest\test.nfs";
		const string srcVol0 = @"C:\src\test.nfs";
		const string srcVol1 = @"C:\src\test_001.nfs";

		// Create source volume files with known data
		var vol0Data = new byte[0x1000];
		RandomNumberGenerator.Fill(vol0Data);
		var vol1Data = new byte[0x1000];
		RandomNumberGenerator.Fill(vol1Data);

		fileSystem.AddFile(srcVol0, new MockFileData(vol0Data));
		fileSystem.AddFile(srcVol1, new MockFileData(vol1Data));
		fileSystem.AddDirectory(destDir);

		// Volume sources
		var vol0Source = new NefsVolumeSource(srcVol0, 0, 0);
		var vol1Source = new NefsVolumeSource(srcVol1, 0, 0);
		var volumes = new NefsVolumeSource[] { vol0Source, vol1Source };

		// Build items
		var items = new NefsItemList(volumes);
		var blockSize = 0x100u;
		var transform = new NefsDataTransform(blockSize);

		// Item 0: "a.txt" in vol0, root, offset 0x200, extractedSize 50
		var aChunks = NefsDataChunk.CreateChunkList(new List<uint> { 50 }, transform);
		var aSize = new NefsItemSize(50, aChunks);
		var aDataSrc = new NefsVolumeDataSource(vol0Source, 0x200, aSize);
		var aAttrs = new NefsItemAttributes { IsTransformed = false };
		var aItem = new NefsItem(new NefsItemId(0), "a.txt", new NefsItemId(0), aDataSrc, transform, aAttrs);
		items.Add(aItem);

		// Item 1: "root" directory
		var rootAttrs = new NefsItemAttributes(isDirectory: true);
		var rootItem = new NefsItem(new NefsItemId(1), "root", new NefsItemId(1), new NefsEmptyDataSource(), null, rootAttrs);
		items.Add(rootItem);

		// Item 2: "b.txt" in vol1, inside root, offset 0x100, extractedSize 30
		var bChunks = NefsDataChunk.CreateChunkList(new List<uint> { 30 }, transform);
		var bSize = new NefsItemSize(30, bChunks);
		var bDataSrc = new NefsVolumeDataSource(vol1Source, 0x100, bSize);
		var bAttrs = new NefsItemAttributes { IsTransformed = false, Volume = 1 };
		var bItem = new NefsItem(new NefsItemId(2), "b.txt", new NefsItemId(1), bDataSrc, transform, bAttrs);
		items.Add(bItem);

		// Build donor header (version 151) with 2 volumes
		var nameTable = new NefsHeaderNameTable(["root.txt", "test.nfs", "test_001.nfs"]);
		var volInfoEntries = new NefsTocVolumeInfo150[]
		{
			new() { Size = 0, NameOffset = nameTable.OffsetsByFileName["test.nfs"], DataOffset = 0 },
			new() { Size = 0xDEAD, NameOffset = nameTable.OffsetsByFileName["test_001.nfs"], DataOffset = 0 }
		};
		var volumeInfoTable = new NefsHeaderVolumeInfoTable150(volInfoEntries);
		var intro = new NefsTocHeader151
		{
			Version = (uint)NefsVersion.Version151,
			BlockSize = blockSize,
			SplitSize = 0,
		};
		var donorHeader = new NefsHeader151(
			new NefsWriterSettings(false, false, true),
			intro,
			new NefsHeaderEntryTable150([]),
			new NefsHeaderSharedEntryInfoTable150([]),
			nameTable,
			new NefsHeaderBlockTable151([]),
			volumeInfoTable);

		var archive = new NefsArchive(donorHeader, items);
		var writer = CreateWriter();

		// Act
		var (resultArchive, _) = await writer.WriteArchiveAsync(destFilePath, archive, new NefsProgress());

		// Assert: co-location — both files exist in dest directory
		Assert.True(fileSystem.File.Exists(destFilePath));
		Assert.True(fileSystem.File.Exists(Path.Combine(destDir, "test_001.nfs")));

		// Assert: returned archive volumes point to dest paths
		Assert.Equal(destFilePath, resultArchive.Items.Volumes[0].FilePath);
		Assert.Equal(Path.Combine(destDir, "test_001.nfs"), resultArchive.Items.Volumes[1].FilePath);

		// Assert: item count
		Assert.Equal(3, resultArchive.Items.Count);

		// Assert: vol0 items have volume 0, vol1 items have volume 1
		var resultA = resultArchive.Items.EnumerateById().First(x => x.FileName == "a.txt");
		Assert.Equal(0, (int)resultA.Attributes.Volume);
		var resultB = resultArchive.Items.EnumerateById().First(x => x.FileName == "b.txt");
		Assert.Equal(1, (int)resultB.Attributes.Volume);

		// Assert: vol1 file byte-identical to source (co-located copy)
		byte[] ReadAll(string path) { using var s = fileSystem.File.OpenRead(path); var b = new byte[s.Length]; s.Read(b); return b; }
		var vol1Hash = SHA256.HashData(ReadAll(srcVol1));
		var destVol1Hash = SHA256.HashData(ReadAll(Path.Combine(destDir, "test_001.nfs")));
		Assert.Equal(vol1Hash, destVol1Hash);

		// Read back with NefsReader
		var reader = new NefsReader(fileSystem);
		var readArchive = await reader.ReadArchiveAsync(destFilePath, new NefsProgress());

		// Assert: 2 volumes
		Assert.Equal(2, readArchive.Header.Volumes.Count);

		// Assert: vol1 volume info preserved from donor (Size = 0xDEAD)
		Assert.Equal(0xDEADul, readArchive.Header.Volumes[1].Size);

		// Assert: 3 items with correct names and volumes
		Assert.Equal(3, readArchive.Items.Count);
		var readFileA = readArchive.Items.EnumerateById().First(x => x.FileName == "a.txt");
		Assert.Equal(0, (int)readFileA.Attributes.Volume);
		Assert.Equal(50u, readFileA.ExtractedSize);

		var readFileB = readArchive.Items.EnumerateById().First(x => x.FileName == "b.txt");
		Assert.Equal(1, (int)readFileB.Attributes.Volume);
		Assert.Equal(30u, readFileB.ExtractedSize);

		// Assert: vol1 item data matches original source bytes
		using (var readStream = readFileB.DataSource.OpenRead(fileSystem))
		{
			readStream.Seek(readFileB.DataSource.Offset, SeekOrigin.Begin);
			var readBData = new byte[readFileB.ExtractedSize];
			await readStream.ReadExactlyAsync(readBData);
			var expectedBData = vol1Data.AsSpan(0x100, 30).ToArray();
			Assert.Equal(expectedBData, readBData);
		}

		// Assert: vol0 item data matches original source bytes
		using (var readStream = readFileA.DataSource.OpenRead(fileSystem))
		{
			readStream.Seek(readFileA.DataSource.Offset, SeekOrigin.Begin);
			var readAData = new byte[readFileA.ExtractedSize];
			await readStream.ReadExactlyAsync(readAData);
			var expectedAData = vol0Data.AsSpan(0x200, 50).ToArray();
			Assert.Equal(expectedAData, readAData);
		}
	}

	[Fact]
	public async Task WriteArchiveAsync_AddedChild_RecomputesLastSiblingFlags()
	{
		// Arrange
		const string destDir = @"C:\dest";
		const string destFilePath = @"C:\dest\test.nfs";
		const string srcVol0 = @"C:\src\test.nfs";
		const string newFilePath = @"C:\src\new.txt";

		var vol0Data = new byte[0x1000];
		RandomNumberGenerator.Fill(vol0Data);
		var newData = "new child"u8.ToArray();
		this.fileSystem.AddFile(srcVol0, new MockFileData(vol0Data));
		this.fileSystem.AddFile(newFilePath, new MockFileData(newData));
		this.fileSystem.AddDirectory(destDir);

		var vol0Source = new NefsVolumeSource(srcVol0, 0, 0);
		var items = new NefsItemList([vol0Source]);
		var blockSize = 0x100u;
		var transform = new NefsDataTransform(blockSize);

		var root = new NefsItem(
			new NefsItemId(0),
			"root",
			new NefsItemId(0),
			new NefsEmptyDataSource(),
			null,
			new NefsItemAttributes(isDirectory: true) { IsLastSibling = true });
		items.Add(root);

		var oldChunks = NefsDataChunk.CreateChunkList(new List<uint> { 50 }, transform);
		var oldSize = new NefsItemSize(50, oldChunks);
		var oldSource = new NefsVolumeDataSource(vol0Source, 0x200, oldSize);
		var oldItem = new NefsItem(
			new NefsItemId(1),
			"old.txt",
			new NefsItemId(0),
			oldSource,
			transform,
			new NefsItemAttributes { IsTransformed = true, IsLastSibling = true });
		items.Add(oldItem);

		var newItem = new NefsItem(
			new NefsItemId(2),
			new NefsItemId(2),
			"new.txt",
			new NefsItemId(0),
			new NefsFileDataSource(newFilePath, 0, new NefsItemSize((uint)newData.Length), false),
			new NefsDataTransform(blockSize, true),
			new NefsItemAttributes { IsTransformed = true },
			NefsItemState.Added);
		items.Add(newItem);

		var nameTable = new NefsHeaderNameTable(["test.nfs"]);
		var volumeInfoTable = new NefsHeaderVolumeInfoTable150(
		[
			new NefsTocVolumeInfo150
			{
				Size = 0,
				NameOffset = nameTable.OffsetsByFileName["test.nfs"],
				DataOffset = 0
			}
		]);
		var intro = new NefsTocHeader151
		{
			Version = (uint)NefsVersion.Version151,
			BlockSize = blockSize,
			SplitSize = 0,
		};
		var donorHeader = new NefsHeader151(
			new NefsWriterSettings(false, false, true),
			intro,
			new NefsHeaderEntryTable150([]),
			new NefsHeaderSharedEntryInfoTable150([]),
			nameTable,
			new NefsHeaderBlockTable151([]),
			volumeInfoTable);
		var archive = new NefsArchive(donorHeader, items);

		// Act
		await CreateWriter().WriteArchiveAsync(destFilePath, archive, new NefsProgress());
		var reader = new NefsReader(this.fileSystem);
		var readArchive = await reader.ReadArchiveAsync(destFilePath, new NefsProgress());

		// Assert
		var readOld = readArchive.Items.EnumerateById().First(x => x.FileName == "old.txt");
		var readNew = readArchive.Items.EnumerateById().First(x => x.FileName == "new.txt");
		Assert.False(readOld.Attributes.IsLastSibling);
		Assert.True(readNew.Attributes.IsLastSibling);
		Assert.Equal((uint)newData.Length, readNew.ExtractedSize);

		using var input = readNew.DataSource.OpenRead(this.fileSystem);
		using var output = new MemoryStream();
		var transformer = new NefsTransformer(this.fileSystem);
		await transformer.DetransformAsync(
			input,
			readNew.DataSource.Offset,
			output,
			0,
			readNew.ExtractedSize,
			readNew.DataSource.Size.Chunks,
			new NefsProgress());
		Assert.Equal(newData, output.ToArray());
	}
	private NefsWriter CreateWriter()
	{
		return new NefsWriter(TempDir, this.fileSystem, this.transformer);
	}
}
