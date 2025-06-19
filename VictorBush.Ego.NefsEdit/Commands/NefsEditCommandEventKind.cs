// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Commands;

/// <summary>
/// Defines kinds of command events.
/// </summary>
internal enum NefsEditCommandEventKind
{
	/// <summary>
	/// A new command was executed.
	/// </summary>
	New,

	/// <summary>
	/// An undo was performed.
	/// </summary>
	Undo,

	/// <summary>
	/// A redo was performed.
	/// </summary>
	Redo,
}
