using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using VictorBush.Ego.NefsEdit.Source;
using VictorBush.Ego.NefsEdit.Utility;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.IO;

namespace VictorBush.Ego.NefsEdit
{
    internal class NefsEditWorkspace : INefsEditWorkspace
    {
        private static readonly ILog Log = LogHelper.GetLogger();

        public NefsEditWorkspace(IProgressService progressService)
        {
            this.ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        }

        public IProgressService ProgressService { get; }

        public NefsWriter NefsWriter => throw new NotImplementedException();

        public NefsReader NefsReader => throw new NotImplementedException();
        public IUiService UiService { get; }
        public INefsCompressor NefsCompressor => throw new NotImplementedException();

        public IFileSystem FileSystem => throw new NotImplementedException();

        // TODO ??
        public bool ArchiveIsModified { get; set; }

        /// <summary>
        /// Gets the current archive.
        /// </summary>
        public NefsArchive Archive { get; private set; }

        public string ArchiveFileName { get; private set; }

        public event EventHandler ArchiveOpened;
        public event EventHandler ArchiveClosed;
        public event EventHandler SelectedItemsChanged;

        public bool CloseArchive()
        {
            if (this.Archive == null)
            {
                // Nothing to close
                return true;
            }

            // Check if there are pending changes
            if (this.ArchiveIsModified)
            {
                var fileName = this.FileSystem.Path.GetFileName(this.ArchiveFileName);
                var result = this.UiService.ShowMessageBox($"Save changes to {fileName}?", null, MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel)
                {
                    // Cancel, don't close
                    return false;
                }
                else if (result == DialogResult.Yes)
                {
                    // Save archive before closing
                    // TODO
                    throw new NotImplementedException("TODO");
                }
            }

            // Close archive
            this.Archive = null;
            this.ArchiveIsModified = false;
            this.ArchiveFileName = "";

            // Notify archive closed
            this.ArchiveClosed?.Invoke(this, EventArgs.Empty);

            return true;
        }

        public async Task OpenArchiveAsync(string filePath)
        {
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
                        this.ArchiveOpened?.Invoke(this, EventArgs.Empty);
                    }
                    catch(Exception ex)
                    {
                        Log.Error($"Failed to open archive {filePath}.\r\n{ex.Message}");
                    }
                }
            }));
        }

        public Task OpenArchiveByDialogAsync()
        {
            throw new NotImplementedException();
        }
    }
}
