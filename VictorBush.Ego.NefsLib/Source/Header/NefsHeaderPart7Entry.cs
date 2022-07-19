// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsLib.Header;

/// <summary>
/// An entry in header part 7 for an item in an archive.
/// </summary>
public class NefsHeaderPart7Entry : INefsHeaderPartEntry
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsHeaderPart7Entry"/> class.
	/// </summary>
	internal NefsHeaderPart7Entry()
	{
	}

	/// <summary>
	/// Gets the id of the item this entry is for.
	/// </summary>
	public NefsItemId Id
	{
		get => new NefsItemId(Data0x04_Id.Value);
		init => Data0x04_Id.Value = value.Value;
	}

	/// <summary>
	/// Gets the id of the next item in the same directory as this item. If this item is the last item in the directory,
	/// the sibling id will equal the item id.
	/// </summary>
	public NefsItemId SiblingId
	{
		get => new NefsItemId(Data0x00_SiblingId.Value);
		init => Data0x00_SiblingId.Value = value.Value;
	}

	public int Size => NefsHeaderPart7.EntrySize;

	[FileData]
	private UInt32Type Data0x00_SiblingId { get; } = new UInt32Type(0x00);

	[FileData]
	private UInt32Type Data0x04_Id { get; } = new UInt32Type(0x04);
}
