// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Commands
{
    using System;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Command to replace an item's data source.
    /// </summary>
    internal class RemoveFileCommand : INefsEditCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveFileCommand"/> class.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="oldState">The old item state.</param>
        public RemoveFileCommand(
            NefsItem item,
            NefsItemState oldState)
        {
            this.Item = item ?? throw new ArgumentNullException(nameof(item));
            this.OldState = oldState;
            this.NewState = NefsItemState.Removed;
        }

        /// <summary>
        /// Gets the item the action is performed on.
        /// </summary>
        public NefsItem Item { get; }

        /// <summary>
        /// Gets the new item state.
        /// </summary>
        public NefsItemState NewState { get; }

        /// <summary>
        /// Gets the old item state.
        /// </summary>
        public NefsItemState OldState { get; }

        /// <inheritdoc/>
        public void Do()
        {
            this.Item.UpdateState(this.NewState);
        }

        /// <inheritdoc/>
        public void Undo()
        {
            this.Item.UpdateState(this.OldState);
        }
    }
}
