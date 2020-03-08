// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using log4net;
    using VictorBush.Ego.NefsEdit.Commands;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Utility;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Handles archive and item operations.
    /// </summary>
    internal class NefsEditWorkspace : INefsEditWorkspace
    {
        private static readonly ILog Log = LogHelper.GetLogger();

        private List<NefsItem> selectedItems = new List<NefsItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsEditWorkspace"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system to use.</param>
        /// <param name="progressService">The progress service to use.</param>
        /// <param name="uiService">The UI service to use.</param>
        /// <param name="settingsService">The settings service to use.</param>
        /// <param name="nefsReader">The nefs reader to use.</param>
        /// <param name="nefsWriter">The nefs wrtier to use.</param>
        public NefsEditWorkspace(
            IFileSystem fileSystem,
            IProgressService progressService,
            IUiService uiService,
            ISettingsService settingsService,
            INefsReader nefsReader,
            INefsWriter nefsWriter)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            this.UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            this.SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.NefsReader = nefsReader ?? throw new ArgumentNullException(nameof(nefsReader));
            this.NefsWriter = nefsWriter ?? throw new ArgumentNullException(nameof(nefsWriter));

            this.Archive = null;
            this.ArchiveFilePath = "";
            this.UndoBuffer = new UndoBuffer();
        }

        private ISettingsService SettingsService { get; }

        /// <inheritdoc/>
        public event EventHandler ArchiveClosed;

        /// <inheritdoc/>
        public event EventHandler ArchiveOpened;

        /// <inheritdoc/>
        public event EventHandler SelectedItemsChanged;

        /// <inheritdoc/>
        public event EventHandler ArchiveSaved;

        /// <inheritdoc/>
        public NefsArchive Archive { get; private set; }

        /// <inheritdoc/>
        public string ArchiveFilePath { get; private set; }

        /// <inheritdoc/>
        public bool ArchiveIsModified => this.UndoBuffer.IsModified;

        /// <inheritdoc/>
        public IFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public INefsReader NefsReader { get; }

        /// <inheritdoc/>
        public INefsWriter NefsWriter { get; }

        /// <inheritdoc/>
        public IProgressService ProgressService { get; }

        /// <inheritdoc/>
        public IReadOnlyList<NefsItem> SelectedItems => this.selectedItems;

        /// <inheritdoc/>
        public IUiService UiService { get; }

        /// <summary>
        /// Gets the undo buffer.
        /// </summary>
        private UndoBuffer UndoBuffer { get; }

        /// <summary>
        /// Executes a command and adds to the undo buffer.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void Execute(INefsEditCommand command)
        {
            this.UndoBuffer.Execute(command);
        }

        /// <summary>
        /// Performs an undo operation if available.
        /// </summary>
        public void Undo()
        {
            this.UndoBuffer.Undo();
        }

        /// <summary>
        /// Performs a redo operation if available.
        /// </summary>
        public void Redo()
        {
            this.UndoBuffer.Redo();
        }

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
                    // Save archive before closing
                    if (!await this.SaveArchiveAsync())
                    {
                        // User canceled saving or save failed, so don't close
                        return false;
                    }
                }
            }

            // Close archive
            this.Archive = null;
            this.ArchiveFilePath = "";
            this.UndoBuffer.Reset();

            // Notify archive closed
            this.ArchiveClosed?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //public async Task<bool> ExtractItemsByQuickExtractAsync(IReadOnlyList<NefsItem> items)
        //{
        //    throw new NotImplementedException();

        //    // If the quick extract dir doesn't exist, have user choose one
        //    if (!this.FileSystem.Directory.Exists(this.SettingsService.QuickExtractDir))
        //    {
        //        if (!this.SettingsService.ChooseQuickExtractDir())
        //        {
        //            // User cancelled the directory selection
        //            return false;
        //        }
        //    }
        //}

        private async Task<bool> ExtractDirectoryAsync(NefsItem item, string outputDirPath)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> ExtractFileAsync(NefsItem item, string outputFilePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<bool> ExtractItemsByDialogAsync(IReadOnlyList<NefsItem> items)
        {
            throw new NotImplementedException();

            //if (items == null || items.Count == 0)
            //{
            //    MessageBox.Show("No items selected to selected.");
            //    return false;
            //}

            //var outputDir = "";
            //var outputFile = "";

            //if (useQuickExtract)
            //{
            //    /*
            //     * Use the quick extraction directory
            //     */
            //    if (!Directory.Exists(Settings.QuickExtractDir))
            //    {
            //        /* Quick extract dir not set, have user choose one */
            //        if (!Settings.ChooseQuickExtractDir())
            //        {
            //            /* User cancelled the directory selection */
            //            return;
            //        }
            //    }

            //    outputDir = Settings.QuickExtractDir;
            //}
            //else
            //{
            //    /*
            //     * Have user choose where to save the items
            //     */
            //    var result = DialogResult.Cancel;

            //    /* Show either a directory chooser or a save file dialog */
            //    if (items.Count > 1 || items[0].Type == NefsItemType.Directory)
            //    {
            //        /* Extracting multiple files or a directory - show folder browser */
            //        var fbd = new FolderBrowserDialog();
            //        fbd.Description = "Choose where to extract the items to.";
            //        fbd.ShowNewFolderButton = true;

            //        result = fbd.ShowDialog();
            //        outputDir = fbd.SelectedPath;
            //    }
            //    else
            //    {
            //        /* Extracting a file - show a save file dialog*/
            //        var sfd = new SaveFileDialog();
            //        sfd.OverwritePrompt = true;
            //        sfd.FileName = items[0].FileName;

            //        result = sfd.ShowDialog();
            //        outputDir = Path.GetDirectoryName(sfd.FileName);
            //        outputFile = Path.GetFileName(sfd.FileName);
            //    }

            //    if (result != DialogResult.OK)
            //    {
            //        /* Use canceled the dialog box */
            //        return;
            //    }
            //}

            ///* Create a progress dialog form */
            //var progressDialog = new ProgressDialogForm(this.Workspace.UiService);

            ///* Show the loading dialog asnyc */
            //var progressDialogTask = progressDialog.ShowDialogAsync();

            /* Extract the item */
            //await Task.Run(() =>
            //{
            //    try
            //    {
            //        var p = progressDialog.ProgressInfo;
            //        var numItems = _selectedItems.Count;

            //        log.Info("----------------------------");
            //        p.BeginTask(1.0f);

            //        /* Extract each item */
            //        for (int i = 0; i < numItems; i++)
            //        {
            //            var item = _selectedItems[i];
            //            var dir = outputDir;
            //            var file = outputFile;

            //            /* When extracting multiple items or using the quick extraction 
            //             * directory, use the original filenames and directory structure
            //             * of the archive */
            //            if (numItems > 0 || useQuickExtract)
            //            {
            //                var dirInArchive = Path.GetDirectoryName(item.FilePathInArchive);
            //                dir = Path.Combine(outputDir, dirInArchive);
            //                file = Path.GetFileName(item.FilePathInArchive);
            //            }

            //            log.Info(String.Format("Extracting {0} to {1}...", item.FilePathInArchive, Path.Combine(dir, file)));
            //            try
            //            {
            //                item.Extract(dir, file, p);
            //            }
            //            catch (Exception ex)
            //            {
            //                log.Error(String.Format("Error extracting item {0}.", item.FilePathInArchive), ex);
            //            }
            //        }

            //        p.EndTask();
            //        log.Info("Extraction finished.");
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Error("Error extracting items.", ex);
            //    }
            //});

            /* Close the progress dialog */
            //progressDialog.Close();
        }

        /// <inheritdoc/>
        public async Task<bool> OpenArchiveAsync(string filePath)
        {
            var result = false;

            // Close existing archive if needed
            if (this.Archive != null)
            {
                if (!await this.CloseArchiveAsync())
                {
                    // User canceled close or close failed
                    return false;
                }
            }

            // Open archive
            await this.ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
            {
                using (var tt = p.BeginTask(1.0f))
                {
                    Log.Info($"Opening archive: {filePath}.");

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

                        Log.Info($"Archive opened.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to open archive {filePath}.\r\n{ex.Message}");
                    }
                }
            }));

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> OpenArchiveByDialogAsync()
        {
            (var result, var fileName) = this.UiService.ShowOpenFileDialog();
            if (result != DialogResult.OK)
            {
                return false;
            }

            return await this.OpenArchiveAsync(fileName);
        }

        /// <inheritdoc/>
        public bool ReplaceItemByDialog(NefsItem item)
        {
            if (item == null)
            {
                Log.Error($"Cannot replace item. Item is null.");
                return false;
            }

            // Have user pick file
            (var result, var fileName) = this.UiService.ShowOpenFileDialog();
            if (result != DialogResult.OK)
            {
                return false;
            }

            // Check file exists
            if (!this.FileSystem.File.Exists(fileName))
            {
                Log.Error($"Cannot replace item. Replacement file does not exist: {fileName}.");
                return false;
            }

            var fileSize = this.FileSystem.FileInfo.FromFileName(fileName).Length;
            var itemSize = new NefsItemSize((uint)fileSize);
            var newDataSource = new NefsFileDataSource(fileName, 0, itemSize, item.DataSource.Size.IsCompressed);
            var cmd = new ReplaceFileCommand(item, item.DataSource, item.State, newDataSource);
            this.UndoBuffer.Execute(cmd);
            return true;
        }

        /// <inheritdoc/>
        public bool ReplaceSeletedItemByDialog()
        {
            if (this.SelectedItems.Count == 0)
            {
                this.UiService.ShowMessageBox("No item selected to replace.");
                return false;
            }

            if (this.SelectedItems.Count > 1)
            {
                this.UiService.ShowMessageBox("Replacing multiple files not supported.");
                return false;
            }

            if (this.SelectedItems[0].Type == NefsItemType.Directory)
            {
                this.UiService.ShowMessageBox("Replacing directories not supported.");
                return false;
            }

            return this.ReplaceItemByDialog(this.SelectedItems[0]);
        }

        /// <inheritdoc/>
        public async Task<bool> SaveArchiveAsync()
        {
            return await this.DoSaveArchiveAsync(this.ArchiveFilePath);
        }

        /// <inheritdoc/>
        public async Task<bool> SaveArchiveAsync(string destFilePath)
        {
            return await this.DoSaveArchiveAsync(destFilePath);
        }

        /// <inheritdoc/>
        public async Task<bool> SaveArchiveByDialogAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<bool> DoSaveArchiveAsync(string destFilePath)
        {
            if (this.Archive == null)
            {
                Log.Error("Failed to save archive: no archive open.");
                return false;
            }

            // TODO : Don't allow saving encrypted archives

            Log.Info($"Writing archive: {destFilePath}.");
            var result = false;

            // Save archive
            await this.ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
            {
                using (var tt = p.BeginTask(1.0f))
                {
                    // Save archive
                    try
                    {
                        this.Archive = await this.NefsWriter.WriteArchiveAsync(destFilePath, this.Archive, p);
                        this.ArchiveFilePath = destFilePath;

                        this.ArchiveOpened?.Invoke(this, EventArgs.Empty);
                        result = true;

                        Log.Info($"Archive opened.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to saved archive {destFilePath}.\r\n{ex.Message}");
                    }
                }
            }));

            return result;
        }

        /// <inheritdoc/>
        public void SelectItem(NefsItem item)
        {
            this.SelectItems(new List<NefsItem> { item });
        }

        /// <inheritdoc/>
        public void SelectItems(IEnumerable<NefsItem> items)
        {
            this.selectedItems = new List<NefsItem>(items);
            this.SelectedItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
