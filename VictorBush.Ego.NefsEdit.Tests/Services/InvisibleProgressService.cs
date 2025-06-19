// See LICENSE.txt for license information.

using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsEdit.Tests.Services;

/// <summary>
/// Progress service that does not display any progress dialog. Used for testing.
/// </summary>
internal class InvisibleProgressService : IProgressService
{
	public async Task RunModalTaskAsync(Func<NefsProgress, Task> task)
	{
		await task(new NefsProgress());
	}
}
