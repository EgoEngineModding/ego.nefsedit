// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataSource
{
    using System;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Defines the source of data for an item that is replaced or added.
    /// </summary>
    public interface INefsDataSource
    {
        /// <summary>
        /// Gets the path of the file that contains the item's data.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Gets the offset in the source file where the data begins.
        /// </summary>
        UInt64 Offset { get; }

        /// <summary>
        /// Gets a value indicating whether the item's data should be compressed before writing into
        /// the target NeFS archive.
        /// </summary>
        bool ShouldCompress { get; }

        /// <summary>
        /// Gets the size information about the source data.
        /// </summary>
        NefsItemSize Size { get; }
    }
}
