// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Services
{
    using System;
    using System.Threading.Tasks;
    using log4net;
    using VictorBush.Ego.NefsEdit.UI;
    using VictorBush.Ego.NefsEdit.Utility;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// Progress service implementation.
    /// </summary>
    internal class ProgressService : IProgressService
    {
        private static readonly ILog Log = LogHelper.GetLogger();

        /// <inheritdoc/>
        public async Task RunModalTaskAsync(Func<NefsProgress, Task> task)
        {
            // Create a progress dialog
            var progressForm = new ProgressDialogForm();

            // Show the progress dialog. Don't await this call. Need to allow dialog to show
            // modally, but want to continue execution.
            var progressFormTask = progressForm.ShowDialogAsync();

            // Run the task
            try
            {
                await task(progressForm.ProgressInfo);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            // Close the progress dialog
            progressForm.Close();
        }
    }
}
