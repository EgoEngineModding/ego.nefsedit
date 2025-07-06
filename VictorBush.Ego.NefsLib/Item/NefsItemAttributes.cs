// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item;

/// <summary>
/// Additional attributes that describe an item.
/// </summary>
public readonly record struct NefsItemAttributes
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemAttributes"/> struct.
	/// </summary>
	/// <param name="isCacheable">Indicates if item is cacheable.</param>
	/// <param name="isDirectory">Indicates if item is directory.</param>
	/// <param name="isDuplicated">Indicates if item has duplicates.</param>
	/// <param name="isPatched">Indicates if item is patched.</param>
	/// <param name="v20IsZlib">Version 2.0 - Indicates if item's data is zlib compressed.</param>
	/// <param name="v20IsAes">Version 2.0 - Indicates if item's data is AES encrypted.</param>
	public NefsItemAttributes(
		bool isCacheable = false,
		bool isDirectory = false,
		bool isDuplicated = false,
		bool isPatched = false,
		bool v20IsZlib = false,
		bool v20IsAes = false)
	{
		IsCacheable = isCacheable;
		IsDirectory = isDirectory;
		IsDuplicated = isDuplicated;
		IsPatched = isPatched;
		V20IsZlib = v20IsZlib;
		V20IsAes = v20IsAes;
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
	/// Whether this item is the last sibling within its parent.
	/// </summary>
	public bool IsLastSibling { get; init; }

	/// <summary>
	/// A flag indicating whether this item is patched (meaning unknown).
	/// </summary>
	public bool IsPatched { get; }

	/// <summary>
	/// The volume containing the item.
	/// </summary>
	public ushort Volume { get; init; }

	/// <summary>
	/// A flag indicating whether the item's data has transforms applied (compressed, encrypted, etc).
	/// </summary>
	public bool IsTransformed { get; init; }

	/// <summary>
	/// Version 2.0 - indicates whether the item's data is AES encrypted.
	/// </summary>
	public bool V20IsAes { get; }

	/// <summary>
	/// Version 2.0 - indicates whether the item's data is zlib compressed.
	/// </summary>
	public bool V20IsZlib { get; }
}
