// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataSource
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Data source that has no data.
    /// </summary>
    public class NefsEmptyDataSource : INefsDataSource
    {
        /// <inheritdoc/>
        public string FilePath => "";

        /// <inheritdoc/>
        public Boolean IsTransformed => true;

        /// <inheritdoc/>
        public UInt64 Offset => 0;

        /// <inheritdoc/>
        public NefsItemSize Size => new NefsItemSize(0, new List<NefsDataChunk>());
    }
}
