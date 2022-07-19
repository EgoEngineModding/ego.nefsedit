// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item;

/// <summary>
/// A (kind of) unique identifier for an item in a NeFS archive. Used to identify parent/sibling relationships. There
/// can be items with duplicate ids in an archive though.
/// </summary>
public struct NefsItemId : IComparable<NefsItemId>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemId"/> struct.
	/// </summary>
	/// <param name="id">The value of the id.</param>
	public NefsItemId(uint id)
	{
		Value = id;
	}

	/// <summary>
	/// Gets or sets the value of this id.
	/// </summary>
	public uint Value { get; }

	/// <summary>
	/// Checks if two ids are not equal.
	/// </summary>
	/// <param name="a">The first id.</param>
	/// <param name="b">The second id.</param>
	/// <returns>True if the ids are not equal.</returns>
	public static bool operator !=(NefsItemId a, NefsItemId b) => !(a == b);

	/// <summary>
	/// Checks if two ids are equal.
	/// </summary>
	/// <param name="a">The first id.</param>
	/// <param name="b">The second id.</param>
	/// <returns>True if the ids are equal.</returns>
	public static bool operator ==(NefsItemId a, NefsItemId b) => a.Value == b.Value;

	/// <inheritdoc/>
	public int CompareTo(NefsItemId other)
	{
		return Value.CompareTo(other.Value);
	}

	/// <inheritdoc/>
	public override bool Equals(object obj) => obj is NefsItemId id && id == this;

	/// <inheritdoc/>
	public override int GetHashCode() => Value.GetHashCode();

	/// <inheritdoc/>
	public override string ToString()
	{
		return Value.ToString();
	}
}
