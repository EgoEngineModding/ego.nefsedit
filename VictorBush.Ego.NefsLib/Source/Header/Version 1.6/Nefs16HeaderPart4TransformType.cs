// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Types of transformations applied to a data chunk.
/// </summary>
public enum Nefs16HeaderPart4TransformType
{
	/// <summary>
	/// No transformation.
	/// </summary>
	None = 0x0,

	/// <summary>
	/// Chunk is LZSS compressed.
	/// </summary>
	Lzss = 0x1,

	/// <summary>
	/// Chunk is AES encrypted.
	/// </summary>
	Aes = 0x4,

	/// <summary>
	/// Chunk is zlib compressed.
	/// </summary>
	Zlib = 0x7,
}
