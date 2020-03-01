using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using VictorBush.Ego.NefsEdit.UI;
using VictorBush.Ego.NefsEdit.Utility;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsEdit
{
    internal class ProgressService : IProgressService
    {
        private static readonly ILog Log = LogHelper.GetLogger();


        public async Task RunModalTaskAsync(Func<NefsProgress, Task> task)
        {
            // Create a progress dialog
            var progressForm = new ProgressDialogForm();

            // Show the progress dialog. Don't await this call. Need to allow
            // dialog to show modally, but want to continue execution.
            var _ = progressForm.ShowDialogAsync();

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
