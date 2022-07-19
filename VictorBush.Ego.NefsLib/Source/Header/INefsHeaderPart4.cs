// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// Header part 4.
/// </summary>
public interface INefsHeaderPart4
{
	/// <summary>
	/// List of entries in order as they appear in the archive. Index is not item id. There can be multiple entries per
	/// single item.
	/// </summary>
	IReadOnlyList<INefsHeaderPartEntry> EntriesByIndex { get; }

	/// <summary>
	/// Gets the current size of header part 4.
	/// </summary>
	int Size { get; }

	/// <summary>
	/// Gets the index into part 4 for the specified item. This index would be for the first part 4 entry for the item
	/// (unless there's only a single data chunk for the item, then the index is for the only entry). The index into
	/// part 4 is not equivalent to the item's id.
	/// </summary>
	/// <param name="item">The item to get the index for.</param>
	/// <returns>The index into part 4.</returns>
	uint GetIndexForItem(NefsItem item);
}
