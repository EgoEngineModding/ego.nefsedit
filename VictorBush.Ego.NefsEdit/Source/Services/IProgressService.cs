// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsEdit.Services;

/// <summary>
/// Provides task execution and progress display services.
/// </summary>
internal interface IProgressService
{
	/// <summary>
	/// Opens a modal progress dialog and executes a foreground task that reports progress to the progress popup.
	/// </summary>
	/// <param name="task">The task to execute.</param>
	/// <returns>The <see cref="Task"/>.</returns>
	Task RunModalTaskAsync(Func<NefsProgress, Task> task);
}
