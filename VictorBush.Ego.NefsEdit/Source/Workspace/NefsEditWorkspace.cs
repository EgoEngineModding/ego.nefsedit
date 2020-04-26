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
    using Microsoft.Extensions.Logging;
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
        private static readonly ILogger Log = LogHelper.GetLogger();

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
        /// <param name="nefsTransformer">The nefs transformer to use.</param>
        public NefsEditWorkspace(
            IFileSystem fileSystem,
            IProgressService progressService,
            IUiService uiService,
            ISettingsService settingsService,
            INefsReader nefsReader,
            INefsWriter nefsWriter,
            INefsTransformer nefsTransformer)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            this.UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            this.SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.NefsReader = nefsReader ?? throw new ArgumentNullException(nameof(nefsReader));
            this.NefsWriter = nefsWriter ?? throw new ArgumentNullException(nameof(nefsWriter));
            this.NefsTransformer = nefsTransformer ?? throw new ArgumentNullException(nameof(nefsTransformer));

            this.Archive = null;
            this.ArchiveSource = null;

            this.UndoBuffer = new UndoBuffer();
            this.UndoBuffer.CommandExecuted += (o, e) => this.CommandExecuted?.Invoke(o, e);
        }

        /// <inheritdoc/>
        public event EventHandler ArchiveClosed;

        /// <inheritdoc/>
        public event EventHandler ArchiveOpened;

        /// <inheritdoc/>
        public event EventHandler ArchiveSaved;

        /// <inheritdoc/>
        public event EventHandler<NefsEditCommandEventArgs> CommandExecuted;

        /// <inheritdoc/>
        public event EventHandler SelectedItemsChanged;

        /// <inheritdoc/>
        public NefsArchive Archive { get; private set; }

        /// <inheritdoc/>
        public bool ArchiveIsModified => this.UndoBuffer.IsModified;

        /// <inheritdoc/>
        public NefsArchiveSource ArchiveSource { get; private set; }

        /// <inheritdoc/>
        public bool CanRedo => this.UndoBuffer.CanRedo;

        /// <inheritdoc/>
        public bool CanUndo => this.UndoBuffer.CanUndo;

        /// <inheritdoc/>
        public IFileSystem FileSystem { get; }

        /// <inheritdoc/>
        public INefsReader NefsReader { get; }

        /// <inheritdoc/>
        public INefsWriter NefsWriter { get; }

        /// <inheritdoc/>
        public IReadOnlyList<NefsItem> SelectedItems => this.selectedItems;

        private INefsTransformer NefsTransformer { get; }

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
                var fileName = this.FileSystem.Path.GetFileName(this.ArchiveSource.DataFilePath);
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

            Log.LogInformation("----------------------------");
            Log.LogInformation($"Closing archive: {this.ArchiveSource.DataFilePath}.");

            // Close archive
            this.Archive = null;
            this.ArchiveSource = null;
            this.UndoBuffer.Reset();
            this.SelectItems(Enumerable.Empty<NefsItem>());

            // Notify archive closed
            this.ArchiveClosed?.Invoke(this, EventArgs.Empty);

            Log.LogInformation($"Archive closed.");

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
                var path = this.Archive.Items.GetItemFilePath(item.Id);
                var fullPath = Path.Combine(baseDir, path);
                var dir = Path.GetDirectoryName(fullPath);
                extractionList.AddRange(this.GetExtractionList(item, this.Archive.Items, dir));
            }

            // Extract the files
            return await this.ExtractFilesAsync(extractionList);
        }

        /// <inheritdoc/>
        public async Task<bool> OpenArchiveAsync(NefsArchiveSource source)
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
                    Log.LogInformation("----------------------------");

                    // Check if header/data files are split
                    if (source.IsHeaderSeparate)
                    {
                        Log.LogInformation($"Opening archive:");
                        Log.LogInformation($"Header file: {source.HeaderFilePath}");
                        Log.LogInformation($"Header offset: {source.HeaderOffset}");
                        Log.LogInformation($"Data file: {source.DataFilePath}");
                    }
                    else
                    {
                        Log.LogInformation($"Opening archive: {source.DataFilePath}");
                    }

                    // Verify file exists
                    if (!this.FileSystem.File.Exists(source.HeaderFilePath))
                    {
                        Log.LogError($"File not found: {source.HeaderFilePath}.");
                        return;
                    }

                    if (!this.FileSystem.File.Exists(source.DataFilePath))
                    {
                        Log.LogError($"File not found: {source.DataFilePath}.");
                        return;
                    }

                    // Open archive
                    try
                    {
                        this.Archive = await this.NefsReader.ReadArchiveAsync(source, p);
                        this.ArchiveSource = source;

                        this.ArchiveOpened?.Invoke(this, EventArgs.Empty);
                        result = true;

                        Log.LogInformation($"Archive opened.");
                    }
                    catch (Exception ex)
                    {
                        Log.LogError($"Failed to open archive {source.DataFilePath}.\r\n{ex.Message}");
                    }
                }
            }));

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> OpenArchiveAsync(string filePath)
        {
            var source = new NefsArchiveSource(filePath);
            return await this.OpenArchiveAsync(source);
        }

        /// <inheritdoc/>
        public async Task<bool> OpenArchiveByDialogAsync()
        {
            (var result, var source) = this.UiService.ShowNefsEditOpenFileDialog(this.SettingsService, this.ProgressService, this.NefsReader);
            if (result != DialogResult.OK)
            {
                return false;
            }

            return await this.OpenArchiveAsync(source);
        }

        /// <inheritdoc/>
        public void Redo()
        {
            if (this.UndoBuffer.Redo())
            {
                Log.LogInformation("Redo executed.");
            }
        }

        /// <inheritdoc/>
        public bool ReplaceItemByDialog(NefsItem item)
        {
            if (item == null)
            {
                Log.LogError($"Cannot replace item. Item is null.");
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
                Log.LogError($"Cannot replace item. Replacement file does not exist: {fileName}.");
                return false;
            }

            var fileSize = this.FileSystem.FileInfo.FromFileName(fileName).Length;
            var itemSize = new NefsItemSize((uint)fileSize);
            var newDataSource = new NefsFileDataSource(fileName, 0, itemSize, false);
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
            return await this.DoSaveArchiveAsync(this.ArchiveSource);
        }

        /// <inheritdoc/>
        public async Task<bool> SaveArchiveAsync(string destFilePath)
        {
            var source = new NefsArchiveSource(destFilePath);
            return await this.DoSaveArchiveAsync(source);
        }

        /// <inheritdoc/>
        public async Task<bool> SaveArchiveByDialogAsync()
        {
            if (this.Archive == null)
            {
                Log.LogError("Failed to save archive: no archive open.");
                return false;
            }

            var fileName = Path.GetFileName(this.Archive.Items.DataFilePath);

            var (result, path) = this.UiService.ShowSaveFileDialog(fileName);
            if (result != DialogResult.OK)
            {
                return false;
            }

            var source = new NefsArchiveSource(path);
            return await this.DoSaveArchiveAsync(source);
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
            if (this.UndoBuffer.Undo())
            {
                Log.LogInformation("Undo executed.");
            }
        }

        private async Task<bool> DoSaveArchiveAsync(NefsArchiveSource source)
        {
            if (this.Archive == null)
            {
                Log.LogError("Failed to save archive: no archive open.");
                return false;
            }

            // Currently don't support saving separated header/data archives (i.e., game.dat)
            if (source.IsHeaderSeparate)
            {
                var msg = "Saving archive with a separated header is not supported.";
                this.UiService.ShowMessageBox(msg);
                Log.LogError(msg);
                return false;
            }

            // Currently don't support offset headers
            if (source.HeaderOffset != 0)
            {
                var msg = "Saving archive with an offset header is not supported.";
                this.UiService.ShowMessageBox(msg);
                Log.LogError(msg);
                return false;
            }

            // Currently don't support saving encrypted archives
            if (this.Archive.Header.IsEncrypted)
            {
                this.UiService.ShowMessageBox("Saving encrypted archives is not supported.", icon: MessageBoxIcon.Error);
                return false;
            }

            var destFilePath = source.DataFilePath;
            Log.LogInformation("----------------------------");
            Log.LogInformation($"Writing archive: {destFilePath}.");
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
                        this.ArchiveSource = source;
                        this.UndoBuffer.MarkAsSaved();

                        this.ArchiveSaved?.Invoke(this, EventArgs.Empty);
                        result = true;

                        Log.LogInformation($"Archive saved.");
                    }
                    catch (Exception ex)
                    {
                        Log.LogError($"Failed to saved archive {destFilePath}.\r\n{ex.Message}");
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
                await this.NefsTransformer.DetransformFileAsync(
                    item.DataSource.FilePath,
                    (Int64)item.DataSource.Offset,
                    outputFilePath,
                    0,
                    item.ExtractedSize,
                    item.DataSource.Size.Chunks,
                    p);

                return true;
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"Failed to extract item {item.FileName}.");
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
                    Log.LogInformation("----------------------------");
                    Log.LogInformation($"Extracting {numToExtract} items...");

                    for (var i = 0; i < extractionList.Count; ++i)
                    {
                        var (item, filePath) = extractionList[i];

                        using (var tt = p.BeginTask(1.0f / numToExtract, $"({i + 1}/{numToExtract}) Extracting '{item.FileName}'..."))
                        {
                            await this.ExtractFileAsync(item, filePath, p);
                        }
                    }

                    Log.LogInformation("Extraction complete.");
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
            var children = itemsList.EnumerateItemChildren(item.Id);
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
