// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A list of items in an archive.
    /// </summary>
    public class NefsItemList : IList<NefsItem>, ICloneable
    {
        private List<NefsItem> items;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsItemList"/> class.
        /// </summary>
        /// <param name="dataFilePath">The path to the file that contains the item data.</param>
        public NefsItemList(string dataFilePath)
        {
            this.items = new List<NefsItem>();
            this.DataFilePath = dataFilePath;
        }

        /// <inheritdoc/>
        public int Count => this.items.Count;

        /// <summary>
        /// Gets the path to the file that contains the item data for the archive. Usually this is
        /// the NeFS archive file.
        /// </summary>
        public string DataFilePath { get; }

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public NefsItem this[int index]
        {
            get => this.items[index];
            set => this.items[index] = value;
        }

        /// <inheritdoc/>
        public void Add(NefsItem item) => this.items.Add(item);

        /// <inheritdoc/>
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
                var newItem = item.Clone() as NefsItem;
                newList.Add(newItem);
            }

            return newList;
        }

        /// <inheritdoc/>
        public bool Contains(NefsItem item) => this.items.Contains(item);

        /// <inheritdoc/>
        public void CopyTo(NefsItem[] array, int arrayIndex) => this.items.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public IEnumerator<NefsItem> GetEnumerator() => this.items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <inheritdoc/>
        public int IndexOf(NefsItem item) => this.items.IndexOf(item);

        /// <inheritdoc/>
        public void Insert(int index, NefsItem item) => this.items.Insert(index, item);

        /// <inheritdoc/>
        public bool Remove(NefsItem item) => this.items.Remove(item);

        /// <inheritdoc/>
        public void RemoveAt(int index) => this.items.RemoveAt(index);
    }
}
