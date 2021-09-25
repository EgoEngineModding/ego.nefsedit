// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Header part 4.
    /// </summary>
    public interface INefsHeaderPart4
    {
        /// <summary>
        /// Gets the current size of header part 4.
        /// </summary>
        int Size { get; }

        IReadOnlyList<INefsHeaderPartEntry> EntriesByIndex { get; }

        /// <summary>
        /// Gets the index into part 4 for the specified item. The index into part 4 is potentially
        /// different from the item's id.
        /// </summary>
        /// <param name="item">The item to get the index for.</param>
        /// <returns>The index into part 4.</returns>
        UInt32 GetIndexForItem(NefsItem item);
    }
}
