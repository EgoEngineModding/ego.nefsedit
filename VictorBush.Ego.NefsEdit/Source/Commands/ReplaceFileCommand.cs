// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsEdit.Commands;

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
		Item = item ?? throw new ArgumentNullException(nameof(item));
		OldDataSource = oldDataSource ?? throw new ArgumentNullException(nameof(oldDataSource));
		OldState = oldState;
		NewDataSource = newDataSource ?? throw new ArgumentNullException(nameof(newDataSource));
		NewState = NefsItemState.Replaced;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ReplaceFileCommand"/> class.
	/// </summary>
	/// <param name="item">The item to replace.</param>
	/// <param name="newDataSource">The new data source.</param>
	public ReplaceFileCommand(NefsItem item, INefsDataSource newDataSource)
	{
		Item = item;
		OldDataSource = item.DataSource;
		OldState = item.State;
		NewDataSource = newDataSource;
		NewState = NefsItemState.Replaced;
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
		Item.UpdateDataSource(NewDataSource, NewState);
	}

	/// <inheritdoc/>
	public void Undo()
	{
		Item.UpdateDataSource(OldDataSource, OldState);
	}
}
