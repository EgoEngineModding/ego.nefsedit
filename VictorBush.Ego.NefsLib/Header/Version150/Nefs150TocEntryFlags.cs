// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version150;

[Flags]
public enum Nefs150TocEntryFlags : ushort
{
	/// <summary>
	/// No flags set.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates if the entry's data has transforms applied (compressed, encrypted, etc).
	/// </summary>
	Transformed = 1 << 0,

	/// <summary>
	/// Indicates if the entry is a directory.
	/// </summary>
	Directory = 1 << 1,

	/// <summary>
	/// Indicates if the entry has duplicates.
	/// </summary>
	Duplicated = 1 << 2,

	/// <summary>
	/// Indicates if the entry is cacheable by game engine.
	/// </summary>
	Cacheable = 1 << 3,

	/// <summary>
	/// Indicates if the entry is the last sibling in the directory.
	/// </summary>
	LastSibling = 1 << 4,

	/// <summary>
	/// Indicates if item is patched by game engine.
	/// </summary>
	Patched = 1 << 5
}
