// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item;

/// <summary>
/// A list of items in an archive. The list is sorted by item id.
/// </summary>
public sealed class NefsItemList : ICloneable
{
	private readonly Dictionary<NefsItemId, NefsItem> itemsById = new();
	private readonly SortedDictionary<NefsItemId, ItemContainer> containersById = new();
	private readonly List<ItemContainer> rootItems = new();

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
	public int Count => this.containersById.Count;

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
		this.itemsById.Add(item.Id, item);

		// Check if duplicate id
		if (item.Id != item.FirstDuplicateId)
		{
			if (!this.containersById.TryGetValue(item.FirstDuplicateId, out var existingContainer))
			{
				throw new ArgumentException(
					$"The item's first duplicate id {item.FirstDuplicateId} does not exist. The first duplicate must be added to the list before the next duplicates.");
			}

			existingContainer.Items.Add(item);
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

			parentContainer.Children.Add(container);
			container.Parent = parentContainer;
		}

		// Add to master list
		this.containersById.Add(item.Id, container);
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
			var newItem = (NefsItem)item.Clone();
			newList.Add(newItem);
		}

		return newList;
	}

	/// <summary>
	/// Enumerates items in order by id.
	/// </summary>
	/// <returns>List of items.</returns>
	public IEnumerable<NefsItem> EnumerateById() => this.containersById.Values.SelectMany(v => v.Items);

	/// <summary>
	/// Enumerates item in depth-first order based on directory structure, but sorts children by item id.
	/// </summary>
	/// <returns>List of items.</returns>
	public IEnumerable<NefsItem> EnumerateDepthFirstById()
	{
		var items = new List<NefsItem>();
		foreach (var item in this.rootItems.OrderBy(i => i.Items.First().Id))
		{
			items.AddRange(item.Items);
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
		var items = new List<NefsItem>();
		foreach (var item in this.rootItems.OrderBy(i => i.Items.First().FileName))
		{
			items.AddRange(item.Items);
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
		var item = this.containersById[id];
		return item.Children.SelectMany(v => v.Items);
	}

	/// <summary>
	/// Enumerates items in the root directory.
	/// </summary>
	/// <returns>Items in the root directory.</returns>
	public IEnumerable<NefsItem> EnumerateRootItems()
	{
		return this.rootItems.SelectMany(v => v.Items).OrderBy(i => i.FileName);
	}

	/// <summary>
	/// Gets the item with the specified id.
	/// </summary>
	/// <param name="id">The id of the item to get.</param>
	/// <returns>The item or null if it does not exist.</returns>
	public NefsItem? GetItem(NefsItemId id)
	{
		return this.itemsById[id];
	}

	/// <summary>
	/// Gets the directory id for an item. If the item is in the root directory, the directory id will equal the item's id.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <returns>The directory id.</returns>
	public NefsItemId GetItemDirectoryId(NefsItemId id)
	{
		var item = this.containersById[id];
		if (item.Parent == null)
		{
			return id;
		}

		return item.Parent.Items.First().Id;
	}

	/// <summary>
	/// Gets the file name of the item.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <returns>The item's file name.</returns>
	public string GetItemFileName(NefsItemId id)
	{
		return this.containersById[id].Items.First().FileName;
	}

	/// <summary>
	/// Gets the file path of the item within the archive.
	/// </summary>
	/// <param name="id">The item id.</param>
	/// <returns>The item's file path within the archive.</returns>
	public string GetItemFilePath(NefsItemId id)
	{
		var path = GetItemFileName(id);

		var dirId = GetItemDirectoryId(id);
		var prevDirId = id;

		while (dirId != prevDirId)
		{
			var dirName = GetItemFileName(dirId);
			path = Path.Combine(dirName, path);

			prevDirId = dirId;
			dirId = GetItemDirectoryId(dirId);
		}

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
		var item = this.containersById[id];
		return item.Children.Count > 0 ? item.Children.OrderBy(i => i.Items.First().Id).First().Items.First().Id : id;
	}

	/// <summary>
	/// Gets all items with the specified id. Throws an exception if not found.
	/// </summary>
	/// <param name="id">The id of the items to get.</param>
	/// <returns>The <see cref="NefsItem"/>.</returns>
	public IReadOnlyList<NefsItem> GetItems(NefsItemId id)
	{
		return this.containersById[id].Items;
	}

	/// <summary>
	/// Gets the item with the specified Guid. Throws an exception if not found.
	/// </summary>
	/// <param name="id">The id of item to get.</param>
	/// <returns>The item.</returns>
	public IReadOnlyList<NefsItem> GetItemsById(NefsItemId id)
	{
		return this.containersById[id].Items;
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
		var item = this.containersById[id];
		var parentList = item.Parent?.Children.OrderBy(i => i.Items.First().Id).Select(i => i.Items.First().Id).ToList()
			?? this.rootItems.OrderBy(i => i.Items.First().Id).Select(i => i.Items.First().Id).ToList();
		var itemIndex = parentList.IndexOf(item.Items.First().Id);

		if (itemIndex == parentList.Count - 1)
		{
			// This is the last item in the directory
			return id;
		}

		// Return id of next item in directory
		return parentList[itemIndex + 1];
	}

	/// <summary>
	/// Removes the item with the specified id.
	/// </summary>
	/// <param name="id">The id of the item to remove.</param>
	public void Remove(NefsItemId id)
	{
		if (!this.containersById.ContainsKey(id))
		{
			return;
		}

		var item = this.containersById[id];
		this.containersById.Remove(id);

		// Check if in root
		if (item.Parent == null)
		{
			this.rootItems.Remove(item);
		}
		else
		{
			// Remove item from parent
			item.Parent?.Children.Remove(item);
			item.Parent = null;
		}
	}

	private class ItemContainer
	{
		public ItemContainer(NefsItem item)
		{
			Items = new List<NefsItem> { item };
		}

		/// <summary>
		/// List of children.
		/// </summary>
		public List<ItemContainer> Children { get; } = new List<ItemContainer>();

		/// <summary>
		/// This is a list to handle duplicate items.
		/// </summary>
		public List<NefsItem> Items { get; set; }

		public ItemContainer? Parent { get; set; }

		/// <summary>
		/// Enumerates children depth first, but sorts children by id.
		/// </summary>
		/// <returns>List of items.</returns>
		public IEnumerable<NefsItem> EnumerateDepthFirstById()
		{
			var items = new List<NefsItem>();
			foreach (var child in Children.OrderBy(i => i.Items.First().Id))
			{
				items.AddRange(child.Items);
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
			foreach (var child in Children.OrderBy(i => i.Items.First().FileName))
			{
				items.AddRange(child.Items);
				items.AddRange(child.EnumerateDepthFirstByName());
			}

			return items;
		}
	}
}
