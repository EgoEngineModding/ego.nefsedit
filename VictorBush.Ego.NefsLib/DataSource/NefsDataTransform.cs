// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataSource;

/// <summary>
/// Describes a transformation applied to a file before being put in the archive.
/// </summary>
public record NefsDataTransform
{
	/// <summary>
	/// Whether the data is transformed.
	/// </summary>
	public bool IsTransformed => IsLzssCompressed || IsAesEncrypted || IsZlibCompressed;

	/// <summary>
	/// Whether the data is LZSS compressed.
	/// </summary>
	public bool IsLzssCompressed { get; init; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsDataTransform"/> class.
	/// </summary>
	/// <param name="chunkSize">The chunk size to use.</param>
	/// <param name="isZlib">Whether to compress with zlib.</param>
	/// <param name="aesKey">The AES-256 key for encryption. Use null if not encrypted.</param>
	public NefsDataTransform(uint chunkSize, bool isZlib, byte[]? aesKey = null)
	{
		ChunkSize = chunkSize;
		IsZlibCompressed = isZlib;
		Aes256Key = aesKey;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsDataTransform"/> class. This transform does nothing (file is
	/// simply placed in the archive as-is).
	/// </summary>
	/// <param name="fileSize">The file size.</param>
	public NefsDataTransform(uint fileSize)
	{
		ChunkSize = fileSize;
		IsZlibCompressed = false;
		Aes256Key = null;
	}

	/// <summary>
	/// The AES 256 key. If the chunk is not encrypted, this will be null.
	/// </summary>
	public byte[]? Aes256Key { get; }

	/// <summary>
	/// The size of chunks to split the input file up into before transforming each chunk.
	/// </summary>
	public uint ChunkSize { get; }

	/// <summary>
	/// Whether data chunks are AES encrypted.
	/// </summary>
	public bool IsAesEncrypted => Aes256Key != null;

	/// <summary>
	/// Whether data chunks are zlib compressed.
	/// </summary>
	public bool IsZlibCompressed { get; }
}
