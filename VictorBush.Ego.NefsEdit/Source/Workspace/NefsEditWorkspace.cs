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

            // Build a list of files to extract (paired with the path where to extract them)
            var extractionList = new List<(NefsItem Item, string FilePath)>();

            // Show either a directory chooser or a save file dialog
            if (items.Count == 1 && items[0].Type == NefsItemType.File)
            {
                /*
                Extracting a single file
                */
                var item = items[0];

                var (result, outputFilePath) = this.UiService.ShowSaveFileDialog(item.FileName);
                if (result != DialogResult.OK)
                {
                    // User canceled the dialog box
                    return false;
                }

                extractionList.Add((item, outputFilePath));
            }
            else
            {
                /*
                Extracting multiple items or a single directory, show folder browser dialog
                */
                var (result, outputDir) = this.UiService.ShowFolderBrowserDialog("Choose where to extract the items to.");
                if (result != DialogResult.OK)
                {
                    // User canceled the dialog box
                    return false;
                }

                // Determine files to extract
                foreach (var item in items)
                {
                    extractionList.AddRange(this.GetExtractionList(item, this.Archive.Items, outputDir));
                }
            }

            // Extract the files
            return await this.ExtractFilesAsync(extractionList);
        }

        /// <inheritdoc/>
        public async Task<bool> ExtractItemsByQuickExtractAsync(IReadOnlyList<NefsItem> items)
        {
            // If the quick extract dir doesn't exist, have user choose one
            if (!this.FileSystem.Directory.Exists(this.SettingsService.QuickExtractDir))
            {
                if (!this.SettingsService.ChooseQuickExtractDir())
                {
                    // User cancelled the directory selection
                    return false;
                }
            }

            // Build a list of files to extract (paired with the path where to extract them)
            var extractionList = new List<(NefsItem Item, string FilePath)>();
            var baseDir = this.SettingsService.QuickExtractDir;

            foreach (var item in items)
            {
                // Use the full path in archive to determine where to extract within the quick
                // extract dir. The quick extract option preserves the structure of the nefs archive
                // within the quick extract dir.
                var fullPath = Path.Combine(baseDir, item.FilePathInArchive);
                var dir = Path.GetDirectoryName(fullPath);
                extractionList.AddRange(this.GetExtractionList(item, this.Archive.Items, dir));
            }

            // Extract the files
            return await this.ExtractFilesAsync(extractionList);
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

        private async Task<bool> ExtractFileAsync(NefsItem item, string outputFilePath, NefsProgress p)
        {
            try
            {
                // Create target directory if needed
                var dir = Path.GetDirectoryName(outputFilePath);
                if (!this.FileSystem.Directory.Exists(dir))
                {
                    this.FileSystem.Directory.CreateDirectory(dir);
                }

                // Extract the file
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

        private async Task<bool> ExtractFilesAsync(IList<(NefsItem Item, string FilePath)> extractionList)
        {
            // Create progress dialog
            await this.ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
            {
                var numToExtract = extractionList.Count;

                using (var t = p.BeginTask(1.0f))
                {
                    Log.Info("----------------------------");
                    Log.Info($"Extracting {numToExtract} items...");

                    for (var i = 0; i < extractionList.Count; ++i)
                    {
                        var (item, filePath) = extractionList[i];

                        using (var tt = p.BeginTask(1.0f / numToExtract, $"({i + 1}/{numToExtract}) Extracting '{item.FileName}'..."))
                        {
                            await this.ExtractFileAsync(item, filePath, p);
                        }
                    }

                    Log.Info("Extraction complete.");
                }
            }));

            return true;
        }

        /// <summary>
        /// Gets a list of files to be extracted. Each file is paired with an output path to where
        /// the content will be extracted. Directories are not included in the list, but they will
        /// be recursively processed for descendant files.
        /// </summary>
        /// <remarks>
        /// The purpose for this function is to provide a list of all files that will be extracted.
        /// This allows the progress reporting to be accurate (allows knowing the total number of
        /// items to be extracted before starting).
        /// </remarks>
        /// <param name="item">The item to get the extraction list for.</param>
        /// <param name="itemsList">The source <see cref="NefsItemList"/>.</param>
        /// <param name="outputDir">The output directory for this item.</param>
        /// <returns>The item and its output path.</returns>
        private IList<(NefsItem Item, string OutputPath)> GetExtractionList(
            NefsItem item,
            NefsItemList itemsList,
            string outputDir)
        {
            var items = new List<(NefsItem, string)>();
            var children = itemsList.Where(i => i.DirectoryId == item.Id && i != item);
            var path = Path.Combine(outputDir, item.FileName);

            if (item.Type == NefsItemType.Directory)
            {
                // Item is directory, add children
                foreach (var child in children)
                {
                    items.AddRange(this.GetExtractionList(child, itemsList, path));
                }
            }
            else
            {
                // Item is file, add self
                items.Add((item, path));
            }

            return items;
        }
    }
}
