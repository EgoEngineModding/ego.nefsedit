// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib;

/// <summary>
/// A NeFS archive.
/// </summary>
public sealed class NefsArchive
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsArchive"/> class.
	/// </summary>
	/// <param name="header">The archive's header.</param>
	/// <param name="items">List of items for this archive.</param>
	public NefsArchive(INefsHeader header, NefsItemList items)
	{
		Header = header ?? throw new ArgumentNullException(nameof(header));
		Items = items ?? throw new ArgumentNullException(nameof(items));
	}

	/// <summary>
	/// NeFS file header.
	/// </summary>
	public INefsHeader Header { get; }

	/// <summary>
	/// List of items in this archive. This list should always be ordered by item id.
	/// </summary>
	public NefsItemList Items { get; }
}
