using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsEdit.UI
{
    /// <summary>
    /// Form that monitors progress reporting for the provided progress reporter.
    /// </summary>
    /// <remarks>
    /// Usage:
    ///     var progressDialog = new ProgressDialogForm();
    ///     var progressInfo = progressDialog.ProgressInfo;    
    /// 
    ///     // Show the modal dialog asynchronously (just using ShowDialog() would block
    ///     //  and prevent the async operation from executing)
    ///     var progressDialogTask = progressDialog.ShowDialogAsync();
    ///     
    ///     // Do the async operation
    ///     var task = await doSomethingAsync(progressInfo);
    ///     
    ///     // Close the dialog
    ///     progressDialog.Close();
    /// </remarks>
    public partial class ProgressDialogForm : Form
    {
        public NefsProgress ProgressInfo { get; }

        private CancellationTokenSource cancelSource;

        private Dispatcher dispatcher;

        public ProgressDialogForm()
        {
            this.InitializeComponent();

            this.dispatcher = Dispatcher.CurrentDispatcher;

            /* Setup cancellation */
            this.cancelSource = new CancellationTokenSource();

            /* Create a progress reporter */
            this.ProgressInfo = new NefsProgress(this.cancelSource.Token);
            this.ProgressInfo.ProgressChanged += onProgress;
        }

        public void SetStyle(ProgressBarStyle style)
        {
            progressBar.Style = style;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            /* Set the cancellation token to cancel */
            this.cancelSource.Cancel();
        }

        private void onProgress(object sender, NefsProgressEventArgs e)
        {
            /* Constrain the progress percentage to appropriate range */
            var value = Math.Min((int)(e.Progress * 100), this.progressBar.Maximum);
            value = Math.Max(value, 0);

            /* Update the form controls - must do on UI thread */
            this.dispatcher.Invoke(() =>
            {
                this.progressBar.Value = value;
                this.statusLabel.Text = e.Message;
            });
        }
    }

    /// <summary>
    /// Extension to allow the form to be a modal dialog while perform async operations.
    /// See: https://stackoverflow.com/questions/33406939/async-showdialog
    /// </summary>
    public static class DialogExt
    {
        public static async Task<DialogResult> ShowDialogAsync(this Form @this)
        {
            await Task.Yield();
            if (@this.IsDisposed)
                return DialogResult.OK;
            return @this.ShowDialog();
        }
    }
}
