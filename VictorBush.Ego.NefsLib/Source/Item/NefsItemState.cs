// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item
{
    /// <summary>
    /// Identifies the modification state of an item in an archive. When an item
    /// is modified, the action does not take effect until the archive is saved.
    /// In the mean time, the modification state identifies the pending change.
    /// </summary>
    public enum NefsItemState
    {
        /// <summary>
        /// No change has been made to the item.
        /// </summary>
        None = 0,

        /// <summary>
        /// The item is being added to the archive.
        /// </summary>
        Added,

        /// <summary>
        /// The item is being removed from the archive.
        /// </summary>
        Removed,

        /// <summary>
        /// The item's file data is being replaced.
        /// </summary>
        Replaced,
    }
}
