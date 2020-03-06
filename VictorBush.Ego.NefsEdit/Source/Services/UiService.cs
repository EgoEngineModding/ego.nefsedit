// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Services
{
    using System;
    using System.Windows.Forms;
    using System.Windows.Threading;

    /// <summary>
    /// UI service implementation.
    /// </summary>
    internal class UiService : IUiService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UiService"/> class.
        /// </summary>
        /// <param name="dispatcher">The dispatcher for the UI thread.</param>
        public UiService(Dispatcher dispatcher)
        {
            this.Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <inheritdoc/>
        public Dispatcher Dispatcher { get; }

        /// <inheritdoc/>
        public DialogResult ShowMessageBox(String message, String title = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return MessageBox.Show(message, title, buttons, icon);
        }

        /// <inheritdoc/>
        public (DialogResult Result, String FileName) ShowOpenFileDialog()
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            var result = dialog.ShowDialog();
            return (result, dialog.FileName);
        }
    }
}
