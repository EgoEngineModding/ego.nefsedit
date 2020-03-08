// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Commands
{
    using System;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Command to replace an item's data source.
    /// </summary>
    internal class ReplaceFileCommand : INefsEditCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceFileCommand"/> class.
        /// </summary>
        /// <param name="item">The item to perform the command on.</param>
        /// <param name="oldDataSource">The old data source.</param>
        /// <param name="oldState">The old state.</param>
        /// <param name="newDataSource">The new data source.</param>
        public ReplaceFileCommand(
            NefsItem item,
            INefsDataSource oldDataSource,
            NefsItemState oldState,
            INefsDataSource newDataSource)
        {
            this.Item = item ?? throw new ArgumentNullException(nameof(item));
            this.OldDataSource = oldDataSource ?? throw new ArgumentNullException(nameof(oldDataSource));
            this.OldState = oldState;
            this.NewDataSource = newDataSource ?? throw new ArgumentNullException(nameof(newDataSource));
            this.NewState = NefsItemState.Replaced;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceFileCommand"/> class.
        /// </summary>
        /// <param name="item">The item to replace.</param>
        /// <param name="newDataSource">The new data source.</param>
        public ReplaceFileCommand(NefsItem item, INefsDataSource newDataSource)
        {
            this.Item = item;
            this.OldDataSource = item.DataSource;
            this.OldState = item.State;
            this.NewDataSource = newDataSource;
            this.NewState = NefsItemState.Replaced;
        }

        /// <summary>
        /// Gets the item the action is performed on.
        /// </summary>
        public NefsItem Item { get; }

        /// <summary>
        /// Gets the new data source.
        /// </summary>
        public INefsDataSource NewDataSource { get; }

        /// <summary>
        /// Gets the new item state.
        /// </summary>
        public NefsItemState NewState { get; }

        /// <summary>
        /// Gets the old data source.
        /// </summary>
        public INefsDataSource OldDataSource { get; }

        /// <summary>
        /// Gets the old item state.
        /// </summary>
        public NefsItemState OldState { get; }

        /// <inheritdoc/>
        public void Do()
        {
            this.Item.UpdateDataSource(this.NewDataSource, this.NewState);
        }

        /// <inheritdoc/>
        public void Undo()
        {
            this.Item.UpdateDataSource(this.OldDataSource, this.OldState);
        }
    }
}
