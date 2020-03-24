// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Services
{
    using System.Windows.Forms;
    using System.Windows.Threading;

    /// <summary>
    /// Provides user interface dialogs and other services.
    /// </summary>
    internal interface IUiService
    {
        /// <summary>
        /// Gets the dispatcher for the UI thread.
        /// </summary>
        Dispatcher Dispatcher { get; }

        /// <summary>
        /// Shows a folder browser dialog.
        /// </summary>
        /// <param name="message">A message to show in the dialog.</param>
        /// <returns>The dialog result and the folder path (if applicable).</returns>
        (DialogResult Result, string Path) ShowFolderBrowserDialog(string message);

        /// <summary>
        /// Shows a message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="buttons">Buttons to show.</param>
        /// <param name="icon">Icon to display.</param>
        /// <returns>The dialog result.</returns>
        DialogResult ShowMessageBox(
            string message,
            string title = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None);

        /// <summary>
        /// Shows an open file dialog.
        /// </summary>
        /// <returns>The dialog result and the file name (if applicable).</returns>
        (DialogResult Result, string FileName) ShowOpenFileDialog();

        /// <summary>
        /// Shows a save file dialog.
        /// </summary>
        /// <param name="defaultName">The default file name.</param>
        /// <returns>The dialog result and the file name (if applicable).</returns>
        (DialogResult Result, string FileName) ShowSaveFileDialog(string defaultName);
    }
}
