// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsEdit.Commands;

/// <summary>
/// Command to remove an item from the archive.
/// </summary>
internal class RemoveFileDuplicatesCommand : INefsEditCommand
{
	/// <summary>
	/// The individual commands that make up this command.
	/// </summary>
	public IReadOnlyList<RemoveFileCommand> Commands { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RemoveFileDuplicatesCommand"/> class.
	/// </summary>
	/// <param name="duplicates">The item and its duplicates to remove.</param>
	public RemoveFileDuplicatesCommand(IReadOnlyList<NefsItem> duplicates)
	{
		Commands = duplicates.Select(x => new RemoveFileCommand(x)).ToArray();
	}

	/// <inheritdoc />
	public void Do()
	{
		foreach (var command in Commands)
		{
			command.Do();
		}
	}

	/// <inheritdoc />
	public void Undo()
	{
		foreach (var command in Commands)
		{
			command.Undo();
		}
	}
}
