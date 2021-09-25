// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataSource
{
    using System;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Defines an item data source from an existing archive. The item list defines the path to the
    /// file that contains the item data content.
    /// </summary>
    /// <remarks>
    /// When saving an archive, the majority of items are typically unchanged. In this case, the
    /// item data is copied from the existing archive into the new archive. The <see
    /// cref="NefsItemListDataSource"/> allows an item to have access to the file path that contains
    /// the item's data without having to have a copy of the file path string in every item.
    /// </remarks>
    public class NefsItemListDataSource : INefsDataSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsItemListDataSource"/> class.
        /// </summary>
        /// <param name="items">
        /// The items list that specifies the file the item's data is stored in.
        /// </param>
        /// <param name="offset">The offset in the source file where the data begins.</param>
        /// <param name="size">The size of the item's data in bytes.</param>
        public NefsItemListDataSource(
            NefsItemList items,
            long offset,
            NefsItemSize size)
        {
            this.Items = items ?? throw new ArgumentNullException(nameof(items));
            this.Offset = offset;
            this.Size = size;
        }

        /// <inheritdoc/>
        public string FilePath => this.Items.DataFilePath;

        /// <inheritdoc/>
        /// <remarks>For an item list data source, the data is already transformed.</remarks>
        public bool IsTransformed => true;

        /// <inheritdoc/>
        public long Offset { get; }

        /// <inheritdoc/>
        public NefsItemSize Size { get; }

        /// <summary>
        /// Gets item list that defines the source file path for item data.
        /// </summary>
        private NefsItemList Items { get; }
    }
}
