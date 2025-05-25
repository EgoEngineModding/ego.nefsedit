// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

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
	/// Unknown.
	/// </summary>
	Unknown0x10 = 0x10,

	/// <summary>
	/// Unknown.
	/// </summary>
	Unknown0x20 = 0x20,

	/// <summary>
	/// Unknown.
	/// </summary>
	Unknown0x40 = 0x40,

	/// <summary>
	/// Unknown.
	/// </summary>
	Unknown0x80 = 0x80,
}
