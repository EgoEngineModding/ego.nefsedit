// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header.Version010;

[Flags]
public enum NefsTocEntryFlags010 : uint
{
	/// <summary>
	/// No flags set.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates if the entry's data has transforms applied (compressed, encrypted, etc).
	/// </summary>
	Transformed = 0x01,

	/// <summary>
	/// Indicates if the entry is a directory.
	/// </summary>
	Directory = 0x02,

	/// <summary>
	/// Indicates if the entry is a duplicate.
	/// </summary>
	Duplicate = 0x04,

	/// <summary>
	/// Indicates if the entry has duplicates.
	/// </summary>
	Duplicated = 0x08,

	/// <summary>
	/// Indicates if the entry is cacheable by game engine.
	/// </summary>
	Cacheable = 0x16
}
