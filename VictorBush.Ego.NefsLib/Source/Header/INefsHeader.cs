// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using VictorBush.Ego.NefsLib.Item;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// A NeFS archive header.
    /// </summary>
    public interface INefsHeader
    {
        /// <summary>
        /// Gets a value indicating whether the header is encrypted.
        /// </summary>
        bool IsEncrypted { get; }

        /// <summary>
        /// Builds an item object from header data.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="dataSourceList">The item list to use as the item data source.</param>
        /// <returns>A new <see cref="NefsItem"/>.</returns>
        NefsItem CreateItemInfo(NefsItemId id, NefsItemList dataSourceList);

        /// <summary>
        /// Generates a new item list from the header metadata.
        /// </summary>
        /// <param name="dataFilePath">The path to the data file for the item list.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>A new <see cref="NefsItemList"/>.</returns>
        NefsItemList CreateItemList(string dataFilePath, NefsProgress p);

        /// <summary>
        /// Gets the directory id for an item. If the item is in the root directory, the directory
        /// id will equal the item's id.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The directory id.</returns>
        NefsItemId GetItemDirectoryId(NefsItemId id);

        /// <summary>
        /// Gets the file name of an item.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>The item's file name.</returns>
        string GetItemFileName(NefsItemId id);
    }
}
