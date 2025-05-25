// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item;

/// <summary>
/// A (kind of) unique identifier for an item in a NeFS archive. Used to identify parent/sibling relationships. There
/// can be items with duplicate ids in an archive though.
/// </summary>
public readonly record struct NefsItemId : IComparable<NefsItemId>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemId"/> struct.
	/// </summary>
	/// <param name="id">The value of the id.</param>
	public NefsItemId(uint id) : this(Convert.ToInt32(id))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemId"/> struct.
	/// </summary>
	/// <param name="index">The value of the index.</param>
	public NefsItemId(int index)
	{
		Index = index;
	}

	/// <summary>
	/// Gets or sets the value of this id.
	/// </summary>
	public uint Value => (uint)Index;

	/// <summary>
	/// Gets the id in index form.
	/// </summary>
	public int Index { get; }

	/// <inheritdoc/>
	public int CompareTo(NefsItemId other)
	{
		return Value.CompareTo(other.Value);
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return Value.ToString();
	}
}
