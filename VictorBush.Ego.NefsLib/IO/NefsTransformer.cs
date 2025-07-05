// See LICENSE.txt for license information.

using System.IO.Abstractions;
using System.IO.Compression;
using System.Security.Cryptography;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.IO;

/// <summary>
/// Handles compression and encryption for NeFS item data.
/// </summary>
public class NefsTransformer : INefsTransformer
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsTransformer"/> class.
	/// </summary>
	/// <param name="fileSystem">The file system to use.</param>
	public NefsTransformer(IFileSystem fileSystem)
	{
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
	}

	/// <summary>
	/// Gets the file system to use.
	/// </summary>
	private IFileSystem FileSystem { get; }

	/// <inheritdoc/>
	public async Task DetransformAsync(
		Stream input,
		long inputOffset,
		Stream output,
		long outputOffset,
		uint extractedSize,
		IReadOnlyList<NefsDataChunk> chunks,
		NefsProgress p)
	{
		input.Seek(inputOffset, SeekOrigin.Begin);
		output.Seek(outputOffset, SeekOrigin.Begin);

		using var t = p.BeginTask(1.0f, $"Detransforming stream");
		if (chunks.Count == 0)
		{
			// Assume data is an untransformed single block
			await input.CopyPartialAsync(output, extractedSize, p.CancellationToken);
			return;
		}

		var numChunks = chunks.Count;
		var bytesRemaining = extractedSize;
		for (int i = 0; i < numChunks; i++)
		{
			using var st = p.BeginSubTask(1.0f / numChunks, $"Detransforming chunk {i + 1}/{numChunks}...");

			// Determine the maximum output size for this chunk based on expected output size
			var maxChunkSize = Math.Min(bytesRemaining, chunks[i].Transform.ChunkSize);

			// Revert the transform
			var chunkSize = await DetransformChunkAsync(input, output, chunks[i], maxChunkSize, p);
			bytesRemaining -= chunkSize;
		}
	}

	/// <inheritdoc/>
	public async Task<uint> DetransformChunkAsync(
		 Stream input,
		 Stream output,
		 NefsDataChunk chunk,
		 uint maxOutputSize,
		 NefsProgress p)
	{
		using var detransformedStream = new MemoryStream();
		CryptoStream? cryptoStream = null;

		// Copy chunk to temp stream
		await input.CopyPartialAsync(detransformedStream, chunk.Size, p.CancellationToken);
		detransformedStream.Seek(0, SeekOrigin.Begin);

		try
		{
			// Decrypt
			if (chunk.Transform.IsAesEncrypted)
			{
				using var aesManager = CreateAesManager(chunk.Transform.Aes256Key!);
				using var tempStream = new MemoryStream();

				cryptoStream = new CryptoStream(detransformedStream, aesManager.CreateDecryptor(), CryptoStreamMode.Read);
				await cryptoStream.CopyToAsync(tempStream, p.CancellationToken);
				tempStream.Seek(0, SeekOrigin.Begin);

				detransformedStream.Seek(0, SeekOrigin.Begin);
				await tempStream.CopyToAsync(detransformedStream, p.CancellationToken);
				detransformedStream.Seek(0, SeekOrigin.Begin);
				detransformedStream.SetLength(tempStream.Length);
			}

			// Decompress
			if (chunk.Transform.IsZlibCompressed)
			{
				using var inflater = new DeflateStream(detransformedStream, CompressionMode.Decompress, leaveOpen: true);
				using var tempStream = new MemoryStream();

				await inflater.CopyToAsync(tempStream, p.CancellationToken);
				tempStream.Seek(0, SeekOrigin.Begin);

				detransformedStream.Seek(0, SeekOrigin.Begin);
				await tempStream.CopyToAsync(detransformedStream, p.CancellationToken);
				detransformedStream.Seek(0, SeekOrigin.Begin);
				detransformedStream.SetLength(tempStream.Length);
			}

			if (chunk.Transform.IsLzssCompressed)
			{
				var lzss = new LzssDecompress();
				using var tempStream = new MemoryStream();

				await lzss.DecompressAsync(detransformedStream, tempStream, p.CancellationToken).ConfigureAwait(false);
				tempStream.Seek(0, SeekOrigin.Begin);

				detransformedStream.Seek(0, SeekOrigin.Begin);
				await tempStream.CopyToAsync(detransformedStream, p.CancellationToken);
				detransformedStream.Seek(0, SeekOrigin.Begin);
				detransformedStream.SetLength(tempStream.Length);
			}

			// Copy detransformed chunk to output stream
			var chunkSize = Math.Min(detransformedStream.Length, maxOutputSize);
			await detransformedStream.CopyPartialAsync(output, chunkSize, p.CancellationToken);
			return (uint)chunkSize;
		}
		finally
		{
			// Manually cleanup crypto stream. The .NET standard version does not have the ability to leave the
			// underlying stream open.
			if (cryptoStream is not null)
			{
				cryptoStream.Dispose();
			}
		}
	}

	/// <inheritdoc/>
	public async Task DetransformFileAsync(
		string inputFile,
		long inputOffset,
		string outputFile,
		long outputOffset,
		uint extractedSize,
		IReadOnlyList<NefsDataChunk> chunks,
		NefsProgress p)
	{
		using (var t = p.BeginTask(1.0f))
		using (var inputStream = FileSystem.File.OpenRead(inputFile))
		using (var outputStream = FileSystem.File.OpenWrite(outputFile))
		{
			await DetransformAsync(inputStream, inputOffset, outputStream, outputOffset, extractedSize, chunks, p);
		}
	}

	/// <inheritdoc />
	public async Task DetransformAsync(INefsDataSource dataSource, string outputFile, long outputOffset, NefsProgress p)
	{
		using (p.BeginTask(1.0f))
		using (var inputStream = FileSystem.OpenRead(dataSource))
		using (var outputStream = FileSystem.File.OpenWrite(outputFile))
		{
			await DetransformAsync(inputStream, dataSource.Offset, outputStream, outputOffset,
				dataSource.Size.ExtractedSize, dataSource.Size.Chunks, p);
		}
	}

	/// <inheritdoc/>
	public async Task<NefsItemSize> TransformAsync(
		Stream input,
		long inputOffset,
		uint inputLength,
		Stream output,
		long outputOffset,
		NefsDataTransform transform,
		NefsProgress p)
	{
		var chunks = new List<NefsDataChunk>();
		var rawChunkSize = transform.ChunkSize;

		input.Seek(inputOffset, SeekOrigin.Begin);
		output.Seek(outputOffset, SeekOrigin.Begin);

		// Split file into chunks and transform them
		using (var t = p.BeginTask(1.0f, $"Transforming stream"))
		{
			var cumulativeChunkSize = 0U;
			var bytesRemaining = (int)inputLength;

			// Determine how many chunks to split file into
			var numChunks = (int)Math.Ceiling(inputLength / (double)rawChunkSize);

			for (var i = 0; i < numChunks; ++i)
			{
				using var st = p.BeginSubTask(1.0f / numChunks, $"Transforming chunk {i + 1}/{numChunks}");

				// The last chunk may not be exactly equal to the raw chunk size
				var nextChunkSize = (int)Math.Min(rawChunkSize, bytesRemaining);
				bytesRemaining -= nextChunkSize;

				// Transform chunk and write to output stream
				var chunkSize = await TransformChunkAsync(input, (uint)nextChunkSize, output, transform, p);
				cumulativeChunkSize += chunkSize;

				// Record chunk info
				var chunk = new NefsDataChunk(chunkSize, cumulativeChunkSize, transform);
				chunks.Add(chunk);
			}
		}

		// Return item size
		return new NefsItemSize(inputLength, chunks);
	}

	/// <inheritdoc/>
	public async Task<uint> TransformChunkAsync(
		Stream input,
		uint inputChunkSize,
		Stream output,
		NefsDataTransform transform,
		NefsProgress p)
	{
		using var transformedStream = new MemoryStream();
		CryptoStream? cryptoStream = null;

		// Copy raw chunk to temp stream
		await input.CopyPartialAsync(transformedStream, inputChunkSize, p.CancellationToken);
		transformedStream.Seek(0, SeekOrigin.Begin);

		try
		{
			// Compress
			if (transform.IsZlibCompressed)
			{
				using var tempStream = new MemoryStream();

				await DeflateHelper.DeflateAsync(transformedStream, (int)inputChunkSize, tempStream, p.CancellationToken);
				tempStream.Seek(0, SeekOrigin.Begin);

				transformedStream.Seek(0, SeekOrigin.Begin);
				await tempStream.CopyPartialAsync(transformedStream, tempStream.Length, p.CancellationToken);
				transformedStream.Seek(0, SeekOrigin.Begin);
				transformedStream.SetLength(tempStream.Length);
			}

			// Encrypt
			if (transform.IsAesEncrypted)
			{
				using var aesManager = CreateAesManager(transform.Aes256Key!);
				using var tempStream = new MemoryStream();

				cryptoStream = new CryptoStream(transformedStream, aesManager.CreateEncryptor(), CryptoStreamMode.Read);
				await cryptoStream.CopyToAsync(tempStream, p.CancellationToken);
				tempStream.Seek(0, SeekOrigin.Begin);

				transformedStream.Seek(0, SeekOrigin.Begin);
				await tempStream.CopyPartialAsync(transformedStream, tempStream.Length, p.CancellationToken);
				transformedStream.Seek(0, SeekOrigin.Begin);
				transformedStream.SetLength(tempStream.Length);
			}

			// Copy transformed chunk to output stream
			await transformedStream.CopyToAsync(output, p.CancellationToken);

			// Return size of transformed chunk
			return (uint)transformedStream.Length;
		}
		finally
		{
			// Manually cleanup crypto stream. The .NET standard version does not have the ability to leave the
			// underlying stream open.
			if (cryptoStream is not null)
			{
				cryptoStream.Dispose();
			}
		}
	}

	/// <inheritdoc/>
	public async Task<NefsItemSize> TransformFileAsync(
		string inputFile,
		string outputFile,
		NefsDataTransform transform,
		NefsProgress p)
	{
		using (var inputStream = FileSystem.File.OpenRead(inputFile))
		using (var outputStream = FileSystem.File.OpenWrite(outputFile))
		{
			return await TransformAsync(inputStream, 0, (uint)inputStream.Length, outputStream, 0, transform, p);
		}
	}

	/// <inheritdoc/>
	public async Task<NefsItemSize> TransformFileAsync(
		INefsDataSource input,
		string outputFile,
		NefsDataTransform transform,
		NefsProgress p)
	{
		using (var inputStream = FileSystem.OpenRead(input))
		using (var outputStream = FileSystem.File.OpenWrite(outputFile))
		{
			return await TransformAsync(inputStream, 0, (uint)inputStream.Length, outputStream, 0, transform, p);
		}
	}

	/// <inheritdoc/>
	public async Task<NefsItemSize> TransformFileAsync(
		string inputFile,
		long inputOffset,
		uint inputLength,
		Stream output,
		long outputOffset,
		NefsDataTransform transform,
		NefsProgress p)
	{
		using (var inputStream = FileSystem.File.OpenRead(inputFile))
		{
			return await TransformAsync(inputStream, inputOffset, inputLength, output, outputOffset, transform, p);
		}
	}

	/// <inheritdoc/>
	public async Task<NefsItemSize> TransformFileAsync(
		string inputFile,
		long inputOffset,
		uint inputLength,
		string outputFile,
		long outputOffset,
		NefsDataTransform transform,
		NefsProgress p)
	{
		using (var inputStream = FileSystem.File.OpenRead(inputFile))
		using (var outputStream = FileSystem.File.OpenWrite(outputFile))
		{
			return await TransformAsync(inputStream, inputOffset, inputLength, outputStream, outputOffset, transform, p);
		}
	}

	private static Aes CreateAesManager(byte[] aes256Key)
	{
		var aes = Aes.Create();
		aes.KeySize = 256;
		aes.Key = aes256Key;
		aes.Mode = CipherMode.ECB;
		aes.BlockSize = NefsConstants.AesBlockSize;
		aes.Padding = PaddingMode.Zeros;
		return aes;
	}
}
