// See LICENSE.txt for license information.

using System.IO.Abstractions.TestingHelpers;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.IO;
using Xunit;

namespace VictorBush.Ego.NefsLib.Tests.IO;

public class SplitFileStreamTests
{
	private const string TempDir = "/temp";
	private const string FilePath = TempDir + "/file.nfs";
	private readonly MockFileSystem fileSystem = new();

	public SplitFileStreamTests()
	{
		this.fileSystem.AddDirectory(TempDir);
	}

	[Fact]
	public void Reads()
	{
		const int splitSize = 3;
		const int dataOffset = 10;
		var buffer = Enumerable.Range(0, 25).Select(x => (byte)x).ToArray();
		SetupFile(splitSize, buffer);
		var volumeSource = new NefsVolumeSource(FilePath, dataOffset, splitSize);

		using var sut = new SplitFileStream(volumeSource, this.fileSystem,
			new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read });

		// Read
		using var ms = new MemoryStream();
		sut.CopyTo(ms);
		var actual = ms.ToArray();

		// Verify
		Assert.Equal(buffer.Length + dataOffset, sut.Position);
		Assert.Equal(buffer.Length + dataOffset, sut.Length);
		Assert.Equal(buffer, actual);
	}

	[Fact]
	public void Seeks()
	{
		const int splitSize = 3;
		const int dataOffset = 10;
		var buffer = Enumerable.Range(0, 25).Select(x => (byte)x).ToArray();
		SetupFile(splitSize, buffer);
		var volumeSource = new NefsVolumeSource(FilePath, dataOffset, splitSize);

		using var sut = new SplitFileStream(volumeSource, this.fileSystem,
			new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read });

		// Seek
		sut.Seek(buffer.Length - 1 + dataOffset, SeekOrigin.Begin);

		// Verify
		Assert.Equal(buffer.Length - 1 + dataOffset, sut.Position);
		Assert.Equal(buffer.Length + dataOffset, sut.Length);

		// Seek
		sut.Seek(-2, SeekOrigin.End);

		// Verify
		Assert.Equal(buffer.Length - 2 + dataOffset, sut.Position);
		Assert.Equal(buffer.Length + dataOffset, sut.Length);

		// Seek
		sut.Seek(1, SeekOrigin.Current);

		// Verify
		Assert.Equal(buffer.Length - 1 + dataOffset, sut.Position);
		Assert.Equal(buffer.Length + dataOffset, sut.Length);
	}

	[Fact]
	public void SeeksBeyondLength()
	{
		const int splitSize = 3;
		const int dataOffset = 10;
		var buffer = Enumerable.Range(0, 25).Select(x => (byte)x).ToArray();
		var fileCount = (buffer.Length + splitSize - 1) / splitSize;
		SetupFile(splitSize, buffer);
		var volumeSource = new NefsVolumeSource(FilePath, dataOffset, splitSize);

		using var sut = new SplitFileStream(volumeSource, this.fileSystem,
			new FileStreamOptions { Mode = FileMode.OpenOrCreate, Access = FileAccess.Write });

		// Seek
		sut.Seek(splitSize, SeekOrigin.End);

		// Verify
		Assert.Equal(buffer.Length + splitSize + dataOffset, sut.Position);
		Assert.Equal(buffer.Length + splitSize + dataOffset - 1, sut.Length); // MemS doesn't adjust length like FileS
		Assert.Equal(fileCount + 1,
			this.fileSystem.Directory.EnumerateFiles(TempDir, "*", SearchOption.AllDirectories).Count());

		// Write
		sut.WriteByte(0);

		// Verify
		Assert.Equal(buffer.Length + splitSize + dataOffset + 1, sut.Position);
		Assert.Equal(buffer.Length + splitSize + dataOffset + 1, sut.Length);
		Assert.Equal(fileCount + 1,
			this.fileSystem.Directory.EnumerateFiles(TempDir, "*", SearchOption.AllDirectories).Count());
	}

	[Fact]
	public void Writes()
	{
		const int splitSize = 3;
		const int dataOffset = 10;
		var buffer = Enumerable.Range(0, 25).Select(x => (byte)x).ToArray();
		var fileCount = (buffer.Length + splitSize - 1) / splitSize;
		var volumeSource = new NefsVolumeSource(FilePath, dataOffset, splitSize);

		using var sut = new SplitFileStream(volumeSource, this.fileSystem,
			new FileStreamOptions { Mode = FileMode.OpenOrCreate, Access = FileAccess.Write });

		// Write
		sut.Write(buffer);
		sut.Flush();
		var actual = ReadFile();

		// Verify
		Assert.Equal(buffer.Length + dataOffset, sut.Position);
		Assert.Equal(buffer.Length + dataOffset, sut.Length);
		Assert.Equal(buffer, actual);
		Assert.Equal(fileCount,
			this.fileSystem.Directory.EnumerateFiles(TempDir, "*", SearchOption.AllDirectories).Count());
	}

	private void SetupFile(int splitSize, ReadOnlySpan<byte> buffer)
	{
		var basePath = Path.Combine(TempDir, Path.GetFileNameWithoutExtension(FilePath)) + ".";
		var fNum = 0;
		for (var i = 0; i < buffer.Length; i += splitSize, ++fNum)
		{
			var buffSize = Math.Min(splitSize, buffer.Length - i);
			this.fileSystem.File.WriteAllBytes(basePath + fNum.ToString("D3"), buffer.Slice(i, buffSize).ToArray());
		}
	}

	private byte[] ReadFile()
	{
		using var ms = new MemoryStream();
		var basePath = this.fileSystem.Path.GetFullPath(Path.Combine(TempDir, Path.GetFileNameWithoutExtension(FilePath)) + ".");
		foreach (var file in this.fileSystem.Directory
			         .EnumerateFiles(TempDir, "*", SearchOption.TopDirectoryOnly)
			         .OrderBy(x => int.Parse(x[basePath.Length..])))
		{
			using var fs = this.fileSystem.File.OpenRead(file);
			fs.CopyTo(ms);
		}

		return ms.ToArray();
	}
}
