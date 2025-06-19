// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header;

/// <inheritdoc />
public abstract class NefsTocTable<T> : INefsTocTable<T>
	where T : unmanaged, INefsTocData<T>
{
	/// <inheritdoc />
	public IReadOnlyList<T> Entries { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsTocTable{T}"/> class.
	/// </summary>
	protected NefsTocTable(IReadOnlyList<T> entries)
	{
		Entries = entries;
	}

	public T GetEntryByOffset(ulong offset)
	{
		var index = Convert.ToInt32(offset / (uint)T.ByteCount);
		return Entries[index];
	}
}
