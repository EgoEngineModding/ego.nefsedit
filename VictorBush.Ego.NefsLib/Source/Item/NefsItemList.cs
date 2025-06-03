// See LICENSE.txt for license information.

using System.Diagnostics;

namespace VictorBush.Ego.NefsLib.Item;

/// <summary>
/// A list of items in an archive. The list is sorted by item id.
/// </summary>
public sealed class NefsItemList : ICloneable
{
	private readonly SortedDictionary<NefsItemId, NefsItem> itemsById = new();
	private readonly SortedDictionary<NefsItemId, ItemContainer> containersById = new();
	private readonly List<ItemContainer> rootItems = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="NefsItemList"/> class.
	/// </summary>
	/// <param name="dataFilePath">The path to the file that contains the item data.</param>
	public NefsItemList(string dataFilePath)
	{
		DataFilePath = dataFilePath;
	}

	/// <summary>
	/// Gets the number of items in the list.
	/// </summary>
	public int Count => this.itemsById.Count;

	/// <summary>
	/// The name of the data file (without directory path, but with extension).
	/// </summary>
	public string DataFileName => Path.GetFileName(DataFilePath);

	/// <summary>
	/// Gets the path to the file that contains the item data for the archive. Usually this is the NeFS archive file.
	/// </summary>
	public string DataFilePath { get; }

	/// <summary>
	/// Adds the item to this list.
	/// </summary>
	/// <param name="item">The item to add.</param>
	public void Add(NefsItem item)
	{
		ArgumentNullException.ThrowIfNull(item);

		// Check if duplicate id
		if (item.IsDuplicate)
		{
			if (!this.containersById.TryGetValue(item.FirstDuplicateId, out var existingContainer))
			{
				throw new ArgumentException(
					$"The item's first duplicate id {item.FirstDuplicateId} does not exist. The first duplicate must be added to the list before the next duplicates.");
			}

			if (existingContainer.Self.Type is not NefsItemType.File)
			{
				throw new ArgumentException("The item's primary duplicate must be a file.", nameof(item));
			}

			// TODO: validate file name, blocks, etc. are same
			Debug.Assert(item.Id > item.FirstDuplicateId);
			existingContainer.Duplicates.Add(item);
			this.itemsById.Add(item.Id, item);
			return;
		}

		// Create a container for the item
		var container = new ItemContainer(item);

		// Check if in the root directory
		if (item.DirectoryId == item.Id)
		{
			// Add to root list
			this.rootItems.Add(container);
		}
		else
		{
			// Find parent
			if (!this.containersById.TryGetValue(item.DirectoryId, out var parentContainer))
			{
				throw new ArgumentException($"The item's parent id {item.DirectoryId} does not exist. The parent must be added to the list before the child.");
			}

			if (parentContainer.Self.Type is not NefsItemType.Directory)
			{
				throw new ArgumentException("The item's parent must be a directory.", nameof(item));
			}

			Debug.Assert(item.Id > item.DirectoryId);
			parentContainer.Children.Add(container);
			container.Parent = parentContainer;
		}

		// Add to master list
		this.containersById.Add(item.Id, container);
		this.itemsById.Add(item.Id, item);
	}

	/// <summary>
	/// Clears the items list.
	/// </summary>
	public void Clear()
	{
		this.itemsById.Clear();
		this.rootItems.Clear();
		this.containersById.Clear();
	}

	/// <summary>
	/// Creates a clone of this list. The items in the new list are clones of the source.
	/// </summary>
	/// <returns>A <see cref="NefsItemList"/>.</returns>
	public object Clone()
	{
		// Create new list
		var newList = new NefsItemList(DataFilePath);

		// Clone each item and add to new list
		foreach (var item in EnumerateDepthFirstByName())
		{
			var newItem = item with {};
			newList.Add(newItem);
		}

		return newList;
	}

	/// <summary>
	/// Enumerates items in order by id.
	/// </summary>
	/// <returns>List of items.</returns>
	public IEnumerable<NefsItem> EnumerateById() => this.itemsById.Values;

	/// <summary>
	/// Enumerates item in depth-first order based on directory structure, but sorts children by item id.
	/// </summary>
	/// <returns>List of items.</returns>
	public IEnumerable<NefsItem> EnumerateDepthFirstById()
	{
		var items = new List<NefsItem>(this.itemsById.Count);
		foreach (var item in this.rootItems.OrderBy(i => i.Self.Id))
		{
			items.AddRange(item.Duplicates);
			items.AddRange(item.EnumerateDepthFirstById());
		}

		return items;
	}

	/// <summary>
	/// Enumerates item in depth-first order based on alphabetized directory structure.
	/// </summary>
	/// <returns>List of items.</returns>
	public IEnumerable<NefsItem> EnumerateDepthFirstByName()
	{
		var items = new List<NefsItem>(this.itemsById.Count);
		foreach (var item in this.rootItems.OrderBy(i => i.Self.FileName.ToLowerInvariant(), StringComparer.Ordinal))
		{
			items.AddRange(item.Duplicates);
			items.AddRange(item.EnumerateDepthFirstByName());
		}

		return items;
	}

	/// <summary>
	/// Enumerates the children of an item.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <returns>The items children.</returns>
	public IEnumerable<NefsItem> EnumerateItemChildren(NefsItemId id)
	{
		var item = GetItemContainer(id);
		return item.Children.SelectMany(v => v.Duplicates);
	}

	/// <summary>
	/// Enumerates items in the root directory.
	/// </summary>
	/// <returns>Items in the root directory.</returns>
	public IEnumerable<NefsItem> EnumerateRootItems()
	{
		return this.rootItems.SelectMany(v => v.Duplicates).OrderBy(i => i.FileName.ToLowerInvariant(), StringComparer.Ordinal);
	}

	/// <summary>
	/// Gets the item with the specified id. Throws if the item does not exist.
	/// </summary>
	/// <param name="id">The id of the item to get.</param>
	/// <returns>The item.</returns>
	public NefsItem GetItem(NefsItemId id)
	{
		return this.itemsById[id];
	}

	/// <summary>
	/// Gets the parent id for the item.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <returns>The parent id, or the item id if the item is at the root level.</returns>
	public NefsItemId GetItemParentId(NefsItemId id)
	{
		var parent = GetItemParent(id);
		return parent?.Id ?? id;
	}

	/// <summary>
	/// Gets the parent for the item.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <returns>The parent, or null if item is at the root level.</returns>
	public NefsItem? GetItemParent(NefsItemId id)
	{
		var item = GetItemContainer(id);
		return item.Parent?.Self;
	}

	/// <summary>
	/// Gets the file name of the item.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <returns>The item's file name.</returns>
	public string GetItemFileName(NefsItemId id)
	{
		return this.itemsById[id].FileName;
	}

	/// <summary>
	/// Gets the file path of the item within the archive.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <returns>The item's file path within the archive.</returns>
	public string GetItemFilePath(NefsItemId id)
	{
		var path = GetItemFileName(id);
		do
		{
			var parent = GetItemParent(id);
			if (parent is null)
			{
				break;
			}

			path = Path.Combine(GetItemFileName(parent.Id), path);
			id = parent.Id;
		} while (true);

		return path;
	}

	/// <summary>
	/// Gets the id of the first child for an item. If the item has no children, the item's id is returned. The first
	/// child id is based on the children being sorted by id, not file name.
	/// </summary>
	/// <param name="id">The id of the item.</param>
	/// <returns>The first child id.</returns>
	public NefsItemId GetItemFirstChildId(NefsItemId id)
	{
		// First child id is based on children items being sorted by id, NOT by file name
		var item = GetItemContainer(id);
		return item.Children.Count > 0 ? item.Children.OrderBy(i => i.Self.Id).First().Self.Id : id;
	}

	/// <summary>
	/// Gets the id of the next item in the same directory as the specified item. If the specified item is that last
	/// item in the directory, the sibling id is equal to the item's id. Sibling id is based on directory structure with
	/// the children sorted by id, not file name.
	/// </summary>
	/// <param name="id">The id of the item to get the sibling for.</param>
	/// <returns>The sibling id.</returns>
	public NefsItemId GetItemSiblingId(NefsItemId id)
	{
		// Sibling id is based on children items being sorted by id, NOT by file name
		var item = GetItemContainer(id);
		var parentList = item.Parent?.Children.OrderBy(i => i.Self.Id).Select(i => i.Self.Id).ToList()
			?? this.rootItems.OrderBy(i => i.Self.Id).Select(i => i.Self.Id).ToList();
		var itemIndex = parentList.IndexOf(item.Self.Id);

		if (itemIndex == parentList.Count - 1)
		{
			// This is the last item in the directory
			return id;
		}

		// Return id of next item in directory
		return parentList[itemIndex + 1];
	}

	/// <summary>
	/// Gets the id of the next duplicate.
	/// </summary>
	/// <param name="id">The id of the item.</param>
	/// <returns>The next duplicate id, or the given id if no duplicates.</returns>
	public NefsItemId GetItemNextDuplicateId(NefsItemId id)
	{
		var item = GetItemContainer(id);
		var duplicates = item.Duplicates.OrderBy(x => x.Id);
		var nextDuplicate = duplicates.SkipWhile(x => x.Id < id).Take(2).Last();
		return nextDuplicate.Id;
	}

	/// <summary>
	/// Gets the duplicates of the item, including itself.
	/// </summary>
	/// <param name="id">The id of the item.</param>
	/// <returns>The duplicates including self.</returns>
	public IReadOnlyList<NefsItem> GetItemDuplicates(NefsItemId id)
	{
		var item = GetItemContainer(id);
		return item.Duplicates;
	}

	/// <summary>
	/// Removes the item with the specified id.
	/// </summary>
	/// <param name="id">The id of the item to remove.</param>
	public void Remove(NefsItemId id)
	{
		// Unused, fix implementation if needed
		throw new NotImplementedException();
		// if (!this.itemsById.ContainsKey(id))
		// {
		// 	return;
		// }
		//
		// var item = GetItemContainer(id);
		// this.containersById.Remove(id);
		// // BUG: remove from items by id and duplicates if any
		//
		// // Check if in root
		// if (item.Parent == null)
		// {
		// 	this.rootItems.Remove(item);
		// }
		// else
		// {
		// 	// Remove item from parent
		// 	item.Parent?.Children.Remove(item);
		// 	item.Parent = null;
		// }
	}

	private ItemContainer GetItemContainer(NefsItemId id)
	{
		if (this.containersById.TryGetValue(id, out var container))
		{
			return container;
		}

		// This must be a duplicate so return the main item's container.
		var item = this.itemsById[id];
		Debug.Assert(item.IsDuplicate);
		return this.containersById[item.FirstDuplicateId];
	}

	private class ItemContainer
	{
		public NefsItem Self { get; }

		public ItemContainer(NefsItem self)
		{
			Self = self;
			Duplicates = [self];
			Children = [];
		}

		/// <summary>
		/// List of children.
		/// </summary>
		public List<ItemContainer> Children { get; }

		/// <summary>
		/// The list of duplicates including self.
		/// </summary>
		public List<NefsItem> Duplicates { get; }

		public ItemContainer? Parent { get; set; }

		/// <summary>
		/// Enumerates children depth first, but sorts children by id.
		/// </summary>
		/// <returns>List of items.</returns>
		public IEnumerable<NefsItem> EnumerateDepthFirstById()
		{
			var items = new List<NefsItem>();
			foreach (var child in Children.OrderBy(i => i.Self.Id))
			{
				items.AddRange(child.Duplicates);
				items.AddRange(child.EnumerateDepthFirstById());
			}

			return items;
		}

		/// <summary>
		/// Enumerates children depth first, but sorts children by name.
		/// </summary>
		/// <returns>List of items.</returns>
		public IEnumerable<NefsItem> EnumerateDepthFirstByName()
		{
			var items = new List<NefsItem>();
			foreach (var child in Children.OrderBy(i => i.Self.FileName.ToLowerInvariant(), StringComparer.Ordinal))
			{
				items.AddRange(child.Duplicates);
				items.AddRange(child.EnumerateDepthFirstByName());
			}

			return items;
		}
	}
}
