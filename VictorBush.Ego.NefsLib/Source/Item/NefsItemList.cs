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
            foreach (var item in this.EnumerateDepthFirst())
            {
                var newItem = item.Clone() as NefsItem;
                newList.Add(newItem);
            }

            return newList;
        }

        /// <summary>
        /// Checks if the item list contains an item with the specified id.
        /// </summary>
        /// <param name="id">The id to check.</param>
        /// <returns>True if the list contains the item id.</returns>
        public bool ContainsKey(NefsItemId id)
        {
            return this.itemsById.ContainsKey(id);
        }

        /// <summary>
        /// Enumerates items in order by id.
        /// </summary>
        /// <returns>List of items.</returns>
        public IEnumerable<NefsItem> EnumerateById() => this.itemsById.Values.Select(v => v.Item);

        /// <summary>
        /// Enumerates item in depth-first order based on alphabetized directory structure.
        /// </summary>
        /// <returns>List of items.</returns>
        public IEnumerable<NefsItem> EnumerateDepthFirst()
        {
            var items = new List<NefsItem>();
            foreach (var item in this.rootItems.Values)
            {
                items.Add(item.Item);
                items.AddRange(item.EnumerateDepthFirst());
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
            return item.Children.Values.Select(v => v.Item);
        }

        /// <summary>
        /// Enumerates items in the root directory.
        /// </summary>
        /// <returns>Items in the root directory.</returns>
        public IEnumerable<NefsItem> EnumerateRootItems()
        {
            return this.rootItems.Values.Select(v => v.Item);
        }

        /// <summary>
        /// Gets the item with the specified id. Throws an exception if not found.
        /// </summary>
        /// <param name="id">The id of the item to get.</param>
        /// <returns>The <see cref="NefsItem"/>.</returns>
        public NefsItem GetItem(NefsItemId id)
        {
            return this.itemsById[id].Item;
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

            return item.Parent.Item.Id;
        }

        /// <summary>
        /// Gets the file name of the item.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The item's file name.</returns>
        public string GetItemFileName(NefsItemId id)
        {
            return this.itemsById[id].Item.FileName;
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
        /// is returned.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <returns>The first child id.</returns>
        public NefsItemId GetItemFirstChildId(NefsItemId id)
        {
            var item = this.itemsById[id];
            return item.Children.Count > 0 ? item.Children.First().Value.Item.Id : id;
        }

        /// <summary>
        /// Gets the id of the next item in the same directory as the specified item. If the
        /// specified item is that last item in the directory, the sibling id is equal to the item's id.
        /// </summary>
        /// <param name="id">The id of the item to get the sibling for.</param>
        /// <returns>The sibling id.</returns>
        public NefsItemId GetItemSiblingId(NefsItemId id)
        {
            var item = this.itemsById[id];
            var parentList = item.Parent?.Children ?? this.rootItems;
            var itemIndex = parentList.IndexOfKey(item.Item.FileName);

            if (itemIndex == parentList.Count - 1)
            {
                // This is the last item in the directory
                return id;
            }

            // Return id of next item in directory
            return parentList.Values[itemIndex + 1].Item.Id;
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
                this.rootItems.Remove(item.Item.FileName);
            }
            else
            {
                // Remove item from parent
                item.Parent?.Children.Remove(item.Item.FileName);
                item.Parent = null;
            }
        }

        private class ItemContainer
        {
            public ItemContainer(NefsItem item)
            {
                this.Item = item;
            }

            /// <summary>
            /// List of children sorted by file name.
            /// </summary>
            public SortedList<string, ItemContainer> Children { get; } = new SortedList<string, ItemContainer>();

            public NefsItem Item { get; set; }

            public ItemContainer Parent { get; set; }

            public IEnumerable<NefsItem> EnumerateDepthFirst()
            {
                var items = new List<NefsItem>();
                foreach (var child in this.Children.Values)
                {
                    items.Add(child.Item);
                    items.AddRange(child.EnumerateDepthFirst());
                }

                return items;
            }
        }
    }
}
