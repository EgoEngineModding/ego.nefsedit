// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsEdit.Commands;

/// <summary>
/// Command to remove an item from the archive.
/// </summary>
internal class RemoveFileCommand : INefsEditCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RemoveFileCommand"/> class.
	/// </summary>
	/// <param name="item">The item to remove.</param>
	public RemoveFileCommand(NefsItem item)
	{
		Item = item ?? throw new ArgumentNullException(nameof(item));
		OldState = item.State;
		NewState = NefsItemState.Removed;
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
		Item.UpdateState(NewState);
	}

	/// <inheritdoc/>
	public void Undo()
	{
		Item.UpdateState(OldState);
	}
}
