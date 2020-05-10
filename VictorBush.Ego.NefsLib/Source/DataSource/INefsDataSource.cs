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
        /// Gets a value indicating whether the data in this data source has had any applicable
        /// transformations applied. When replacing a file in an archive, the replacement data
        /// source will most likely set this to false. When the archive is saved, the transformation
        /// is applied.
        /// </summary>
        bool IsTransformed { get; }

        /// <summary>
        /// Gets the offset in the source file where the data begins.
        /// </summary>
        UInt64 Offset { get; }

        /// <summary>
        /// Gets the size information about the source data.
        /// </summary>
        NefsItemSize Size { get; }
    }
}
