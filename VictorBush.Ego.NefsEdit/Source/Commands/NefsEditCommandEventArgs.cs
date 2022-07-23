// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Commands;

/// <summary>
/// Event arguments for when a command is executed.
/// </summary>
internal class NefsEditCommandEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NefsEditCommandEventArgs"/> class.
	/// </summary>
	/// <param name="kind">The kind of event that was executed.</param>
	/// <param name="command">The command that was executed.</param>
	public NefsEditCommandEventArgs(NefsEditCommandEventKind kind, INefsEditCommand command)
	{
		Kind = kind;
		Command = command ?? throw new ArgumentNullException(nameof(command));
	}

	/// <summary>
	/// Gets the command that was executed.
	/// </summary>
	public INefsEditCommand Command { get; }

	/// <summary>
	/// Gets the kind of command that was executed.
	/// </summary>
	public NefsEditCommandEventKind Kind { get; }
}
