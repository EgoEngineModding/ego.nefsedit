// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsLib.Progress;

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
}
