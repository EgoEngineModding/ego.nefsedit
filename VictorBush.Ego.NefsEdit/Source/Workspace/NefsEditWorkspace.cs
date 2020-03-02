// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Workspace
{
    using System;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using log4net;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Utility;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.IO;

    internal class NefsEditWorkspace : INefsEditWorkspace
    {
        private static readonly ILog Log = LogHelper.GetLogger();

        public NefsEditWorkspace(
            IFileSystem fileSystem,
            IProgressService progressService,
            IUiService uiService,
            INefsReader nefsReader,
            INefsWriter nefsWriter)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            this.UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            this.NefsReader = nefsReader ?? throw new ArgumentNullException(nameof(nefsReader));
            this.NefsWriter = nefsWriter ?? throw new ArgumentNullException(nameof(nefsWriter));
        }

        public event EventHandler ArchiveClosed;

        public event EventHandler ArchiveOpened;

        public event EventHandler SelectedItemsChanged;

        /// <summary>
        /// Gets the current archive.
        /// </summary>
        public NefsArchive Archive { get; private set; }

        /// <summary>
        /// Gest the path to the archive file.
        /// </summary>
        public string ArchiveFilePath { get; private set; }

        // TODO ??
        public bool ArchiveIsModified { get; set; }

        /// <inheritdoc/>
        public IFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public INefsReader NefsReader { get; }

        /// <inheritdoc/>
        public INefsWriter NefsWriter { get; }

        /// <inheritdoc/>
        public IProgressService ProgressService { get; }

        /// <inheritdoc/>
        public IUiService UiService { get; }

        /// <inheritdoc/>
        public async Task<bool> CloseArchiveAsync()
        {
            if (this.Archive == null)
            {
                // Nothing to close
                return true;
            }

            // Check if there are pending changes
            if (this.ArchiveIsModified)
            {
                var fileName = this.FileSystem.Path.GetFileName(this.ArchiveFilePath);
                var result = this.UiService.ShowMessageBox($"Save changes to {fileName}?", null, MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel)
                {
                    // Cancel, don't close
                    return false;
                }
                else if (result == DialogResult.Yes)
                {
                    // Save archive before closing TODO
                    throw new NotImplementedException("TODO");
                }
            }

            // Close archive
            this.Archive = null;
            this.ArchiveIsModified = false;
            this.ArchiveFilePath = "";

            // Notify archive closed
            this.ArchiveClosed?.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> OpenArchiveAsync(string filePath)
        {
            var result = false;

            await this.ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
            {
                using (var tt = p.BeginTask(1.0f))
                {
                    // Verify file exists
                    if (!this.FileSystem.File.Exists(filePath))
                    {
                        Log.Error($"File not found: {filePath}.");
                        return;
                    }

                    // Open archive
                    try
                    {
                        this.Archive = await this.NefsReader.ReadArchiveAsync(filePath, p);
                        this.ArchiveFilePath = filePath;
                        this.ArchiveOpened?.Invoke(this, EventArgs.Empty);
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to open archive {filePath}.\r\n{ex.Message}");
                    }
                }
            }));

            return result;
        }

        public Task<bool> OpenArchiveByDialogAsync()
        {
            throw new NotImplementedException();
        }
    }
}
