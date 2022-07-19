// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item;

/// <summary>
/// Additional attributes that describe an item.
/// </summary>
public struct NefsItemAttributes
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemAttributes"/> struct.
	/// </summary>
	/// <param name="isCacheable">Indicates if item is cacheable.</param>
	/// <param name="isDirectory">Indicates if item is directory.</param>
	/// <param name="isDuplicated">Indicates if item has duplicates.</param>
	/// <param name="isPatched">Indicates if item is patched.</param>
	/// <param name="v16IsTransformed">Indicates if item's data has transformations applied.</param>
	/// <param name="v16Unknown0x10">Unknown flag 0x10 (v1.6).</param>
	/// <param name="v16Unknown0x40">Unknown flag 0x40 (v1.6).</param>
	/// <param name="v16Unknown0x80">Unknown flag 0x80 (v1.6).</param>
	/// <param name="v20IsZlib">Version 2.0 - Indicates if item's data is zlib compressed.</param>
	/// <param name="v20IsAes">Version 2.0 - Indicates if item's data is AES encrypted.</param>
	/// <param name="v20Unknown0x10">Unknown flag 0x10.</param>
	/// <param name="v20Unknown0x20">Unknown flag 0x20.</param>
	/// <param name="v20Unknown0x40">Unknown flag 0x40.</param>
	/// <param name="v20Unknown0x80">Unknown flag 0x80.</param>
	/// <param name="part6Volume">Item volume.</param>
	/// <param name="part6Unknown0x3">Unknown value.</param>
	public NefsItemAttributes(
		bool isCacheable = false,
		bool isDirectory = false,
		bool isDuplicated = false,
		bool isPatched = false,
		bool v16IsTransformed = false,
		bool v16Unknown0x10 = false,
		bool v16Unknown0x40 = false,
		bool v16Unknown0x80 = false,
		bool v20IsZlib = false,
		bool v20IsAes = false,
		bool v20Unknown0x10 = false,
		bool v20Unknown0x20 = false,
		bool v20Unknown0x40 = false,
		bool v20Unknown0x80 = false,
		ushort part6Volume = 0,
		byte part6Unknown0x3 = 0)
	{
		IsCacheable = isCacheable;
		IsDirectory = isDirectory;
		IsDuplicated = isDuplicated;
		IsPatched = isPatched;
		V16IsTransformed = v16IsTransformed;
		V16Unknown0x10 = v16Unknown0x10;
		V16Unknown0x40 = v16Unknown0x40;
		V16Unknown0x80 = v16Unknown0x80;
		V20IsZlib = v20IsZlib;
		V20IsAes = v20IsAes;
		V20Unknown0x10 = v20Unknown0x10;
		V20Unknown0x20 = v20Unknown0x20;
		V20Unknown0x40 = v20Unknown0x40;
		V20Unknown0x80 = v20Unknown0x80;
		Part6Volume = part6Volume;
		Part6Unknown0x3 = part6Unknown0x3;
	}

	/// <summary>
	/// A flag indicating whether this item is cacheable (presumably by the game engine).
	/// </summary>
	public bool IsCacheable { get; }

	/// <summary>
	/// A flag indicating whether this item is a directory.
	/// </summary>
	public bool IsDirectory { get; }

	/// <summary>
	/// A flag indicating whether this item is duplicated.
	/// </summary>
	public bool IsDuplicated { get; }

	/// <summary>
	/// A flag indicating whether this item is patched (meaning unknown).
	/// </summary>
	public bool IsPatched { get; }

	/// <summary>
	/// Unknown data (from part 6).
	/// </summary>
	public byte Part6Unknown0x3 { get; }

	/// <summary>
	/// Meaning unknown (from part 6).
	/// </summary>
	public ushort Part6Volume { get; }

	/// <summary>
	/// Version 1.6 - A flag indicating whether the item's data has transforms applied (compressed, encrypted, etc).
	/// </summary>
	public bool V16IsTransformed { get; }

	/// <summary>
	/// Version 1.6 unknown flag.
	/// </summary>
	public bool V16Unknown0x10 { get; }

	/// <summary>
	/// Version 1.6 unknown flag.
	/// </summary>
	public bool V16Unknown0x40 { get; }

	/// <summary>
	/// Version 1.6 unknown flag.
	/// </summary>
	public bool V16Unknown0x80 { get; }

	/// <summary>
	/// Version 2.0 - indicates whether the item's data is AES encrypted.
	/// </summary>
	public bool V20IsAes { get; }

	/// <summary>
	/// Version 2.0 - indicates whether the item's data is zlib compressed.
	/// </summary>
	public bool V20IsZlib { get; }

	/// <summary>
	/// Version 2.0 unknown flag.
	/// </summary>
	public bool V20Unknown0x10 { get; }

	/// <summary>
	/// Version 2.0 unknown flag.
	/// </summary>
	public bool V20Unknown0x20 { get; }

	/// <summary>
	/// Version 2.0 unknown flag.
	/// </summary>
	public bool V20Unknown0x40 { get; }

	/// <summary>
	/// Version 2.0 unknown flag.
	/// </summary>
	public bool V20Unknown0x80 { get; }
}
