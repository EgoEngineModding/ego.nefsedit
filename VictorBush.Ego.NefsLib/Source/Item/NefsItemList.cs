// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A list of items in an archive. The list is sorted by item id.
    /// </summary>
    public class NefsItemList : IEnumerable<NefsItem>, ICloneable
    {
        private readonly SortedDictionary<NefsItemId, NefsItem> items =
            new SortedDictionary<NefsItemId, NefsItem>();

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
        public int Count => this.items.Count;

        /// <summary>
        /// Gets the path to the file that contains the item data for the archive. Usually this is
        /// the NeFS archive file.
        /// </summary>
        public string DataFilePath { get; }

        /// <summary>
        /// Gets the item with the specified id.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The item.</returns>
        public NefsItem this[NefsItemId id]
        {
            get => this.items[id];
            set => this.items[id] = value;
        }

        /// <summary>
        /// Adds the item to this list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(NefsItem item)
        {
            this.items.Add(item.Id, item);
        }

        /// <summary>
        /// Clears the items list.
        /// </summary>
        public void Clear() => this.items.Clear();

        /// <summary>
        /// Creates a clone of this list. The items in the new list are clones of the source.
        /// </summary>
        /// <returns>A <see cref="NefsItemList"/>.</returns>
        public object Clone()
        {
            // Create new list
            var newList = new NefsItemList(this.DataFilePath);

            // Clone each item and add to new list
            foreach (var item in this.items)
            {
                var newItem = item.Value.Clone() as NefsItem;
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
            return this.items.ContainsKey(id);
        }

        /// <inheritdoc/>
        public IEnumerator<NefsItem> GetEnumerator()
        {
            return this.items.Values.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Removes the item with the specified id.
        /// </summary>
        /// <param name="id">The id of the item to remove.</param>
        public void Remove(NefsItemId id)
        {
            if (this.items.ContainsKey(id))
            {
                this.items.Remove(id);
            }
        }
    }
}
