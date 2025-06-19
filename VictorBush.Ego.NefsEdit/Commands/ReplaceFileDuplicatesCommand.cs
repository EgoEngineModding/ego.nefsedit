// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsEdit.Commands;

/// <summary>
/// Command to replace an item's data source.
/// </summary>
internal class ReplaceFileDuplicatesCommand : INefsEditCommand
{
	/// <summary>
	/// The individual commands that make up this command.
	/// </summary>
	public IReadOnlyList<ReplaceFileCommand> Commands { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ReplaceFileDuplicatesCommand"/> class.
	/// </summary>
	/// <param name="duplicates">The item and its duplicates to replace.</param>
	/// <param name="newDataSource">The new data source.</param>
	public ReplaceFileDuplicatesCommand(IReadOnlyList<NefsItem> duplicates, INefsDataSource newDataSource)
	{
		Commands = duplicates.Select(x => new ReplaceFileCommand(x, newDataSource)).ToArray();
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
