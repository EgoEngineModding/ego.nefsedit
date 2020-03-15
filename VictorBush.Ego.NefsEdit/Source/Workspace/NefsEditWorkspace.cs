// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
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
    using VictorBush.Ego.NefsLib.Progress;

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
        /// <param name="nefsCompressor">The nefs compressor to use.</param>
        public NefsEditWorkspace(
            IFileSystem fileSystem,
            IProgressService progressService,
            IUiService uiService,
            ISettingsService settingsService,
            INefsReader nefsReader,
            INefsWriter nefsWriter,
            INefsCompressor nefsCompressor)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            this.UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            this.SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.NefsReader = nefsReader ?? throw new ArgumentNullException(nameof(nefsReader));
            this.NefsWriter = nefsWriter ?? throw new ArgumentNullException(nameof(nefsWriter));
            this.NefsCompressor = nefsCompressor ?? throw new ArgumentNullException(nameof(nefsCompressor));

            this.Archive = null;
            this.ArchiveFilePath = "";
            this.UndoBuffer = new UndoBuffer();
        }

        /// <inheritdoc/>
        public event EventHandler ArchiveClosed;

        /// <inheritdoc/>
        public event EventHandler ArchiveOpened;

        /// <inheritdoc/>
        public event EventHandler ArchiveSaved;

        /// <inheritdoc/>
        public event EventHandler SelectedItemsChanged;

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
        public IReadOnlyList<NefsItem> SelectedItems => this.selectedItems;

        private INefsCompressor NefsCompressor { get; }

        private IProgressService ProgressService { get; }

        private ISettingsService SettingsService { get; }

        private IUiService UiService { get; }

        /// <summary>
        /// Gets the undo buffer.
        /// </summary>
        private UndoBuffer UndoBuffer { get; }

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

        /// <inheritdoc/>
        public void Execute(INefsEditCommand command)
        {
            this.UndoBuffer.Execute(command);
        }

        /// <inheritdoc/>
        public async Task<bool> ExtractItemsByDialogAsync(IReadOnlyList<NefsItem> items)
        {
            if (items == null || items.Count == 0)
            {
                this.UiService.ShowMessageBox("No items selected to selected.");
                return false;
            }

            // Show either a directory chooser or a save file dialog
            if (items.Count > 1)
            {
                /*
                Extracting multiple items, show folder browser dialog
                */
                var (result, outputDir) = this.UiService.ShowFolderBrowserDialog("Choose where to extract the items to.");

                if (result != DialogResult.OK)
                {
                    // User canceled the dialog box
                    return false;
                }

                // Create progress dialog
                await this.ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
                {
                    using (var t = p.BeginTask(1.0f))
                    {
                        Log.Info("----------------------------");
                        Log.Info($"Extracting {items.Count} items to {outputDir}...");

                        for (var i = 0; i < items.Count; ++i)
                        {
                            using (var tt = p.BeginSubTask(1.0f / items.Count, $"Extracting item {i + 1}/{items.Count}..."))
                            {
                                await this.ExtractItemAsync(items[i], this.Archive.Items, outputDir, p);
                            }
                        }

                        Log.Info("Extracted successfully.");
                    }
                }));
            }
            else if (items.Count == 1 && items[0].Type == NefsItemType.Directory)
            {
                /*
                Extracting a single directory, show folder browser dialog
                */
                var (result, outputDir) = this.UiService.ShowFolderBrowserDialog("Choose where to extract the items to.");

                if (result != DialogResult.OK)
                {
                    // User canceled the dialog box
                    return false;
                }

                // Create progress dialog
                await this.ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
                {
                    var item = items[0];

                    using (var t = p.BeginTask(1.0f, $"Extracting directory '{item.FileName}'"))
                    {
                        Log.Info("----------------------------");
                        Log.Info($"Extracting {item.FileName} to {outputDir}...");

                        outputDir = Path.Combine(outputDir, item.FileName);
                        await this.ExtractDirectoryAsync(item, this.Archive.Items, outputDir, p);

                        Log.Info("Extracted successfully.");
                    }
                }));
            }
            else if (items.Count == 1 && items[0].Type == NefsItemType.File)
            {
                /*
                Extracting a single file
                */
                var item = items[0];
                var (result, outputFile) = this.UiService.ShowSaveFileDialog(item.FileName);

                if (result != DialogResult.OK)
                {
                    // User canceled the dialog box
                    return false;
                }

                // Create progress dialog
                await this.ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
                {
                    using (var t = p.BeginTask(1.0f))
                    {
                        Log.Info("----------------------------");
                        Log.Info($"Extracting {item.FileName} to {outputFile}...");

                        await this.ExtractFileAsync(item, outputFile, p);

                        Log.Info("Extracted successfully.");
                    }
                }));
            }
            else
            {
                Log.Error("Nothing selected to extract.");
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ExtractItemsByQuickExtractAsync(IReadOnlyList<NefsItem> items)
        {
            throw new NotImplementedException();

            // If the quick extract dir doesn't exist, have user choose one
            if (!this.FileSystem.Directory.Exists(this.SettingsService.QuickExtractDir))
            {
                if (!this.SettingsService.ChooseQuickExtractDir())
                {
                    // User cancelled the directory selection
                    return false;
                }
            }
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
                    Log.Info("----------------------------");
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
        public void Redo()
        {
            this.UndoBuffer.Redo();
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

        /// <inheritdoc/>
        public void Undo()
        {
            this.UndoBuffer.Undo();
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

                        this.ArchiveSaved?.Invoke(this, EventArgs.Empty);
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

        private async Task<bool> ExtractDirectoryAsync(NefsItem item, NefsItemList itemList, string outputDirPath, NefsProgress p)
        {
            if (item.Type != NefsItemType.Directory)
            {
                Log.Error($"Attempted to extract directory {item.FileName}, but it is not a directory.");
                return false;
            }

            // Create output directory if needed
            if (!this.FileSystem.Directory.Exists(outputDirPath))
            {
                this.FileSystem.Directory.CreateDirectory(outputDirPath);
            }

            var result = false;
            var children = itemList.Where(i => i.DirectoryId == item.Id && i != item).ToList();

            for (var i = 0; i < children.Count; ++i)
            {
                var child = children[i];

                using (var tt = p.BeginTask(1.0f / children.Count, $"Extracting {i + 1}/{children.Count}."))
                {
                    var childPath = Path.Combine(outputDirPath, child.FileName);
                    result = await this.ExtractItemAsync(child, itemList, childPath, p);
                }
            }

            return result;
        }

        private async Task<bool> ExtractFileAsync(NefsItem item, string outputFilePath, NefsProgress p)
        {
            try
            {
                await this.NefsCompressor.DecompressFileAsync(
                    item.DataSource.FilePath,
                    (Int64)item.DataSource.Offset,
                    item.DataSource.Size.ChunkSizes,
                    outputFilePath,
                    0,
                    p);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to extract item {item.FileName}.\r\n{ex.Message}");
                return false;
            }
        }

        private async Task<bool> ExtractItemAsync(NefsItem item, NefsItemList itemsList, string outputDir, NefsProgress p)
        {
            if (item.Type == NefsItemType.Directory)
            {
                // Extracting a directory
                return await this.ExtractDirectoryAsync(item, itemsList, outputDir, p);
            }
            else
            {
                // Extracting a file
                return await this.ExtractFileAsync(item, outputDir, p);
            }
        }
    }
}
