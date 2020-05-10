// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib
{
    using System;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// A NeFS archive.
    /// </summary>
    public class NefsArchive
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsArchive"/> class.
        /// </summary>
        /// <param name="header">The archive's header.</param>
        /// <param name="items">List of items for this archive.</param>
        public NefsArchive(INefsHeader header, NefsItemList items)
        {
            this.Header = header ?? throw new ArgumentNullException(nameof(header));
            this.Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <summary>
        /// NeFS file header.
        /// </summary>
        public INefsHeader Header { get; }

        /// <summary>
        /// List of items in this archive. This list should always be ordered by item id.
        /// </summary>
        public NefsItemList Items { get; }
    }
}
