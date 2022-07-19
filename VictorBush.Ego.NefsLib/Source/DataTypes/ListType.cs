// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.DataTypes;

/// <summary>
/// A list of items that all share the same size and type.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public sealed class ListType<T> : DataType
{
	private readonly List<T> items;
	private byte[] bytes = Array.Empty<byte>();

	/// <summary>
	/// Initializes a new instance of the <see cref="ListType{T}"/> class.
	/// </summary>
	/// <param name="offset">See <see cref="DataType.Offset"/>.</param>
	/// <param name="itemSize">The size of an item in bytes.</param>
	/// <param name="itemCount">The number of items in the list.</param>
	/// <param name="createItem">A function that creates an item from a provided array of bytes.</param>
	/// <param name="getItemBytes">A function that gets the bytes for an item.</param>
	public ListType(int offset, int itemSize, int itemCount, Func<byte[], T> createItem, Func<T, byte[]> getItemBytes)
		: base(offset)
	{
		Size = itemSize * itemCount;
		ItemSize = itemSize;
		ItemCount = itemCount;
		CreateItem = createItem;
		GetItemBytes = getItemBytes;
		this.items = new List<T>(itemCount);
	}

	/// <summary>
	/// The number of items in the list.
	/// </summary>
	public int ItemCount { get; }

	/// <summary>
	/// The items in the list.
	/// </summary>
	public IReadOnlyList<T> Items => this.items;

	/// <summary>
	/// The size of an item.
	/// </summary>
	public int ItemSize { get; }

	/// <summary>
	/// The size of the array in bytes.
	/// </summary>
	public override int Size { get; }

	/// <summary>
	/// Function that creates an item using a provided array of bytes.
	/// </summary>
	private Func<byte[], T> CreateItem { get; }

	/// <summary>
	/// Function that gets the bytes for an item.
	/// </summary>
	private Func<T, byte[]> GetItemBytes { get; }

	/// <inheritdoc/>
	public override byte[] GetBytes()
	{
		return this.bytes;
	}

	/// <inheritdoc/>
	public override async Task ReadAsync(Stream stream, long baseOffset, NefsProgress p)
	{
		this.bytes = await DoReadAsync(stream, baseOffset, p);
		for (var i = 0; i < ItemCount; ++i)
		{
			var itemBytes = this.bytes.Skip(i * ItemSize).Take(ItemSize).ToArray();
			var item = CreateItem(itemBytes);
			this.items.Add(item);
		}
	}

	/// <summary>
	/// Loads the specified items into the list.
	/// </summary>
	/// <param name="items">The items to put into the list.</param>
	public void SetItems(IEnumerable<T> items)
	{
		var newBytes = new List<byte>();
		this.items.Clear();

		foreach (var item in items)
		{
			var itemBytes = GetItemBytes(item);
			if (itemBytes.Length != ItemSize)
			{
				throw new InvalidOperationException($"This list type is configured with an item size of {ItemSize} but got an item with size of {itemBytes.Length}.");
			}

			newBytes.AddRange(itemBytes);
			this.items.Add(item);
		}

		this.bytes = newBytes.ToArray();

		if (this.items.Count != ItemCount)
		{
			throw new ArgumentException($"This list type is configured for {ItemCount} items but only got {this.items.Count}.");
		}
	}

	/// <inheritdoc/>
	public override string ToString() => StringHelper.ByteArrayToString(this.bytes);
}
