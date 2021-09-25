// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Services
{
    using System;
    using System.IO.Abstractions;
    using System.Windows.Forms;
    using System.Windows.Threading;
    using VictorBush.Ego.NefsEdit.UI;
    using VictorBush.Ego.NefsLib.ArchiveSource;
    using VictorBush.Ego.NefsLib.IO;

    /// <summary>
    /// UI service implementation.
    /// </summary>
    internal class UiService : IUiService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UiService"/> class.
        /// </summary>
        /// <param name="dispatcher">The dispatcher for the UI thread.</param>
        /// <param name="fileSystem">The file system.</param>
        public UiService(Dispatcher dispatcher, IFileSystem fileSystem)
        {
            this.Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <inheritdoc/>
        public Dispatcher Dispatcher { get; }

        private IFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public (DialogResult Result, string Path) ShowFolderBrowserDialog(string message)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = message;
                dialog.ShowNewFolderButton = true;
                var result = dialog.ShowDialog();
                return (result, dialog.SelectedPath);
            }
        }

        /// <inheritdoc/>
        public DialogResult ShowMessageBox(String message, String title = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return MessageBox.Show(message, title, buttons, icon);
        }

        /// <inheritdoc/>
        public (DialogResult Result, NefsArchiveSource Source) ShowNefsEditOpenFileDialog(
            ISettingsService settingsService,
            IProgressService progressService,
            INefsReader reader)
        {
            using (var dialog = new OpenFileForm(settingsService, this, progressService, reader, this.FileSystem))
            {
                var result = dialog.ShowDialog();
                var source = dialog.ArchiveSource;
                return (result, source);
            }
        }

        /// <inheritdoc/>
        public (DialogResult Result, String FileName) ShowOpenFileDialog(string filter = null)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Filter = filter;
                var result = dialog.ShowDialog();
                return (result, dialog.FileName);
            }
        }

        /// <inheritdoc/>
        public (DialogResult Result, string FileName) ShowSaveFileDialog(string defaultName, string filter = null)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.OverwritePrompt = true;
                dialog.FileName = defaultName;
                dialog.Filter = filter;
                var result = dialog.ShowDialog();
                return (result, dialog.FileName);
            }
        }

        /// <inheritdoc/>
        public DialogResult ShowSettingsDialog(ISettingsService settingsService)
        {
            using (var dialog = new SettingsForm(settingsService, this))
            {
                return dialog.ShowDialog();
            }
        }
    }
}
