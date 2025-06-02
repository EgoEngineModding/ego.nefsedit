// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version200;

/// <summary>
/// Bitfield flags for part 6 data.
/// </summary>
[Flags]
public enum Nefs200TocEntryFlags : ushort
{
	/// <summary>
	/// No flags set.
	/// </summary>
	None = 0x0,

	/// <summary>
	/// Indicates if the item's data is zlib compressed.
	/// </summary>
	IsZlib = 0x1,

	/// <summary>
	/// Indicates if the item's data is AES encrypted.
	/// </summary>
	IsAes = 0x2,

	/// <summary>
	/// A flag indicating whether this item is a directory.
	/// </summary>
	IsDirectory = 0x4,

	/// <summary>
	/// Indicates if item has duplicate entries with the same id.
	/// </summary>
	IsDuplicated = 0x8,

	/// <summary>
	/// Indicates if the entry is the last sibling in the directory.
	/// </summary>
	LastSibling = 0x10,
}
