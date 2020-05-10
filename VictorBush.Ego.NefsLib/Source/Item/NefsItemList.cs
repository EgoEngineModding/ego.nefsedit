// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// A list of items in an archive. The list is sorted by item id.
    /// </summary>
    public class NefsItemList : ICloneable
    {
        private readonly Dictionary<Guid, NefsItem> itemsByGuid = new Dictionary<Guid, NefsItem>();

        private readonly SortedDictionary<NefsItemId, ItemContainer> itemsById =
            new SortedDictionary<NefsItemId, ItemContainer>();

        private readonly SortedList<string, ItemContainer> rootItems =
            new SortedList<string, ItemContainer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsItemList"/> class.
        /// </summary>
        /// <param name="dataFilePath">The path to the file that contains the item data.</param>
        public NefsItemList(string dataFilePath)
        {
            this.DataFilePath = dataFilePath;
        }

        /// <summary>
        /// Gets the number of items in the list.
        /// </summary>
        public int Count => this.itemsById.Count;

        /// <summary>
        /// The name of the data file (without directory path, but with extension).
        /// </summary>
        public string DataFileName => Path.GetFileName(this.DataFilePath);

        /// <summary>
        /// Gets the path to the file that contains the item data for the archive. Usually this is
        /// the NeFS archive file.
        /// </summary>
        public string DataFilePath { get; }

        /// <summary>
        /// Adds the item to this list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(NefsItem item)
        {
            // Add to guid lookup
            this.itemsByGuid.Add(item.Guid, item);

            // Check if duplicate id
            if (this.itemsById.ContainsKey(item.Id))
            {
                var existingContainer = this.itemsById[item.Id];
                existingContainer.Items.Add(item);
                return;
            }

            // Create a container for the item
            var container = new ItemContainer(item);

            // Check if in the root directory
            if (item.DirectoryId == item.Id)
            {
                // Add to root list
                this.rootItems.Add(item.FileName, container);
            }
            else
            {
                // Find parent
                if (!this.itemsById.ContainsKey(item.DirectoryId))
                {
                    throw new ArgumentException($"The item's parent id {item.DirectoryId} does not exist. The parent must be added to the list before the child.");
                }

                var parentContainer = this.itemsById[item.DirectoryId];
                parentContainer.Children.Add(item.FileName, container);
                container.Parent = parentContainer;
            }

            // Add to master list
            this.itemsById.Add(item.Id, container);
        }

        /// <summary>
        /// Clears the items list.
        /// </summary>
        public void Clear()
        {
            this.itemsByGuid.Clear();
            this.rootItems.Clear();
            this.itemsById.Clear();
        }

        /// <summary>
        /// Creates a clone of this list. The items in the new list are clones of the source.
        /// </summary>
        /// <returns>A <see cref="NefsItemList"/>.</returns>
        public object Clone()
        {
            // Create new list
            var newList = new NefsItemList(this.DataFilePath);

            // Clone each item and add to new list
            foreach (var item in this.EnumerateDepthFirstByName())
            {
                var newItem = item.Clone() as NefsItem;
                newList.Add(newItem);
            }

            return newList;
        }

        /// <summary>
        /// Checks if the item list contains an item with the specified Guid.
        /// </summary>
        /// <param name="guid">The Guid to check.</param>
        /// <returns>True if the list contains the item id.</returns>
        public bool ContainsKey(Guid guid)
        {
            return this.itemsByGuid.ContainsKey(guid);
        }

        /// <summary>
        /// Enumerates items in order by id.
        /// </summary>
        /// <returns>List of items.</returns>
        public IEnumerable<NefsItem> EnumerateById() => this.itemsById.Values.SelectMany(v => v.Items);

        /// <summary>
        /// Enumerates item in depth-first order based on directory structure, but sorts children by
        /// item id.
        /// </summary>
        /// <returns>List of items.</returns>
        public IEnumerable<NefsItem> EnumerateDepthFirstById()
        {
            var items = new List<NefsItem>();
            foreach (var item in this.rootItems.Values.OrderBy(i => i.Items.First().Id))
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
            foreach (var item in this.rootItems.Values)
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
            var item = this.itemsById[id];
            return item.Children.Values.SelectMany(v => v.Items);
        }

        /// <summary>
        /// Enumerates items in the root directory.
        /// </summary>
        /// <returns>Items in the root directory.</returns>
        public IEnumerable<NefsItem> EnumerateRootItems()
        {
            return this.rootItems.Values.SelectMany(v => v.Items);
        }

        /// <summary>
        /// Gets the item with the specified Guid. Throws an exception if not found.
        /// </summary>
        /// <param name="guid">The guid to get the item for.</param>
        /// <returns>The item.</returns>
        public NefsItem GetItem(Guid guid)
        {
            return this.itemsByGuid[guid];
        }

        /// <summary>
        /// Gets the directory id for an item. If the item is in the root directory, the directory
        /// id will equal the item's id.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The directory id.</returns>
        public NefsItemId GetItemDirectoryId(NefsItemId id)
        {
            var item = this.itemsById[id];
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
            return this.itemsById[id].Items.First().FileName;
        }

        /// <summary>
        /// Gets the file path of the item within the archive.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The item's file path within the archive.</returns>
        public string GetItemFilePath(NefsItemId id)
        {
            var path = this.GetItemFileName(id);

            var dirId = this.GetItemDirectoryId(id);
            var prevDirId = id;

            while (dirId != prevDirId)
            {
                var dirName = this.GetItemFileName(dirId);
                path = Path.Combine(dirName, path);

                prevDirId = dirId;
                dirId = this.GetItemDirectoryId(dirId);
            }

            return path;
        }

        /// <summary>
        /// Gets the id of the first child for an item. If the item has no children, the item's id
        /// is returned. The first child id is based on the children being sorted by id, not file name.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <returns>The first child id.</returns>
        public NefsItemId GetItemFirstChildId(NefsItemId id)
        {
            // First child id is based on children items being sorted by id, NOT by file name
            var item = this.itemsById[id];
            return item.Children.Count > 0 ? item.Children.OrderBy(i => i.Value.Items.First().Id).First().Value.Items.First().Id : id;
        }

        /// <summary>
        /// Gets all items with the specified id. Throws an exception if not found.
        /// </summary>
        /// <param name="id">The id of the items to get.</param>
        /// <returns>The <see cref="NefsItem"/>.</returns>
        public IReadOnlyList<NefsItem> GetItems(NefsItemId id)
        {
            return this.itemsById[id].Items;
        }

        /// <summary>
        /// Gets the id of the next item in the same directory as the specified item. If the
        /// specified item is that last item in the directory, the sibling id is equal to the item's
        /// id. Sibling id is based on directory structure with the children sorted by id, not file name.
        /// </summary>
        /// <param name="id">The id of the item to get the sibling for.</param>
        /// <returns>The sibling id.</returns>
        public NefsItemId GetItemSiblingId(NefsItemId id)
        {
            // Sibling id is based on children items being sorted by id, NOT by file name
            var item = this.itemsById[id];
            var parentList = item.Parent?.Children.OrderBy(i => i.Value.Items.First().Id).Select(i => i.Value.Items.First().Id).ToList()
                ?? this.rootItems.OrderBy(i => i.Value.Items.First().Id).Select(i => i.Value.Items.First().Id).ToList();
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
            if (!this.itemsById.ContainsKey(id))
            {
                return;
            }

            var item = this.itemsById[id];
            this.itemsById.Remove(id);

            // Check if in root
            if (item.Parent == null)
            {
                this.rootItems.Remove(item.Items.First().FileName);
            }
            else
            {
                // Remove item from parent
                item.Parent?.Children.Remove(item.Items.First().FileName);
                item.Parent = null;
            }
        }

        private class ItemContainer
        {
            public ItemContainer(NefsItem item)
            {
                this.Items = new List<NefsItem> { item };
            }

            /// <summary>
            /// List of children sorted by file name.
            /// </summary>
            public SortedList<string, ItemContainer> Children { get; } = new SortedList<string, ItemContainer>();

            /// <summary>
            /// This is a list to handle duplicate items.
            /// </summary>
            public List<NefsItem> Items { get; set; }

            public ItemContainer Parent { get; set; }

            /// <summary>
            /// Enumerates children depth first, but sorts children by id.
            /// </summary>
            /// <returns>List of items.</returns>
            public IEnumerable<NefsItem> EnumerateDepthFirstById()
            {
                var items = new List<NefsItem>();
                foreach (var child in this.Children.Values.OrderBy(i => i.Items.First().Id))
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
                foreach (var child in this.Children.Values)
                {
                    items.AddRange(child.Items);
                    items.AddRange(child.EnumerateDepthFirstByName());
                }

                return items;
            }
        }
    }
}
