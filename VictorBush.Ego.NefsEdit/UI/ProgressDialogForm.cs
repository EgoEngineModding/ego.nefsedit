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
using VictorBush.Ego.NefsLib;

namespace VictorBush.Ego.NefsEdit.UI
{
    /// <summary>
    /// Form that monitors progress reporting for the provided progress reporter.
    /// </summary>
    /// <remarks>
    /// Usage:
    ///     var progressReporter = new Progress<NefsProgress>();
    ///     var progressDialog = new ProgressDialogForm(p);
    ///     
    ///     // Show the modal dialog asynchronously (just using ShowDialog() would block
    ///     //  and prevent the async operation from executing)
    ///     var progressDialogTask = progressDialog.ShowDialogAsync();
    ///     
    ///     // Do the async operation
    ///     var task = await doSomethingAsync(progressReporter);
    ///     
    ///     // Close the dialog
    ///     progressDialog.Close();
    /// </remarks>
    public partial class ProgressDialogForm : Form
    {
        CancellationTokenSource _ctSource;
        Progress<NefsProgress> _progress;
        NefsProgressInfo _progressInfo;

        public ProgressDialogForm()
        {
            InitializeComponent();
            
            /* Create a progress reporter */
            _progress = new Progress<NefsProgress>();
            _progress.ProgressChanged += onProgress;

            /* Create a cancellation source */
            _ctSource = new CancellationTokenSource();

            /* Create the nefs progress info */
            _progressInfo = new NefsProgressInfo();
            _progressInfo.CancellationToken = _ctSource.Token;
            _progressInfo.Progress = _progress;
        }

        public NefsProgressInfo ProgressInfo
        {
            get { return _progressInfo; }
        }

        public void SetStyle(ProgressBarStyle style)
        {
            progressBar.Style = style;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            /* Set the cancellation token to cancel */
            _ctSource.Cancel();
        }

        private void onProgress(object sender, NefsProgress e)
        {
            /* Constrain the progress percentage to appropriate range */
            var value = Math.Min((int)(e.Progress * 100), progressBar.Maximum);
            value = Math.Max(value, 0);

            /* Update the form controls */
            progressBar.Value = value;
            statusLabel.Text = e.Message;
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
