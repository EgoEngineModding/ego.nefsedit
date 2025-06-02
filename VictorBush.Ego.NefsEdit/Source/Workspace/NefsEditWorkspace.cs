// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Abstractions;
using VictorBush.Ego.NefsEdit.Commands;
using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.Utility;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsEdit.Workspace;

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
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
		UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
		SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
		NefsReader = nefsReader ?? throw new ArgumentNullException(nameof(nefsReader));
		NefsWriter = nefsWriter ?? throw new ArgumentNullException(nameof(nefsWriter));
		NefsTransformer = nefsTransformer ?? throw new ArgumentNullException(nameof(nefsTransformer));

		Archive = null;
		ArchiveSource = null;

		UndoBuffer = new UndoBuffer();
		UndoBuffer.CommandExecuted += (o, e) => CommandExecuted?.Invoke(o, e);
	}

	/// <inheritdoc/>
	public event EventHandler? ArchiveClosed;

	/// <inheritdoc/>
	public event EventHandler? ArchiveOpened;

	/// <inheritdoc/>
	public event EventHandler? ArchiveSaved;

	/// <inheritdoc/>
	public event EventHandler<NefsEditCommandEventArgs>? CommandExecuted;

	/// <inheritdoc/>
	public event EventHandler? SelectedItemsChanged;

	/// <inheritdoc/>
	public NefsArchive? Archive { get; private set; }

	/// <inheritdoc/>
	public bool ArchiveIsModified => UndoBuffer.IsModified;

	/// <inheritdoc/>
	public NefsArchiveSource? ArchiveSource { get; private set; }

	/// <inheritdoc/>
	public bool CanRedo => UndoBuffer.CanRedo;

	/// <inheritdoc/>
	public bool CanUndo => UndoBuffer.CanUndo;

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
		if (Archive is null)
		{
			// Nothing to close
			return true;
		}

		// Check if there are pending changes
		if (ArchiveIsModified)
		{
			var fileName = ArchiveSource!.FileName;
			var result = UiService.ShowMessageBox($"Save changes to {fileName}?", null, MessageBoxButtons.YesNoCancel);
			if (result == DialogResult.Cancel)
			{
				// Cancel, don't close
				return false;
			}
			else if (result == DialogResult.Yes)
			{
				// Save archive before closing
				if (!await SaveArchiveAsync())
				{
					// User canceled saving or save failed, so don't close
					return false;
				}
			}
		}

		Log.LogInformation("----------------------------");
		Log.LogInformation($"Closing archive: {ArchiveSource!.FilePath}.");

		// Close archive
		Archive = null;
		ArchiveSource = null;
		UndoBuffer.Reset();
		SelectItems(Enumerable.Empty<NefsItem>());

		// Notify archive closed
		ArchiveClosed?.Invoke(this, EventArgs.Empty);

		Log.LogInformation($"Archive closed.");

		return true;
	}

	/// <inheritdoc/>
	public void Execute(INefsEditCommand command)
	{
		UndoBuffer.Execute(command);
	}

	/// <inheritdoc/>
	public async Task<bool> ExtractItemsByDialogAsync(IReadOnlyList<NefsItem> items)
	{
		if (items == null || items.Count == 0)
		{
			UiService.ShowMessageBox("No items selected to selected.");
			return false;
		}

		if (Archive == null)
		{
			UiService.ShowMessageBox("No archive opened.");
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

			var (result, outputFilePath) = UiService.ShowSaveFileDialog(item.FileName);
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
			var (result, outputDir) = UiService.ShowFolderBrowserDialog("Choose where to extract the items to.");
			if (result != DialogResult.OK)
			{
				// User canceled the dialog box
				return false;
			}

			// Determine files to extract
			foreach (var item in items)
			{
				extractionList.AddRange(GetExtractionList(item, Archive.Items, outputDir));
			}
		}

		// Extract the files
		return await ExtractFilesAsync(extractionList);
	}

	/// <inheritdoc/>
	public async Task<bool> ExtractItemsByQuickExtractAsync(IReadOnlyList<NefsItem> items)
	{
		if (Archive is null)
		{
			Log.LogError("Cannot extract items because no archive is open.");
			return false;
		}

		// If the quick extract dir doesn't exist, have user choose one
		if (!FileSystem.Directory.Exists(SettingsService.QuickExtractDir))
		{
			if (!SettingsService.ChooseQuickExtractDir())
			{
				// User cancelled the directory selection
				return false;
			}
		}

		// Build a list of files to extract (paired with the path where to extract them)
		var extractionList = new List<(NefsItem Item, string FilePath)>();
		var baseDir = SettingsService.QuickExtractDir;

		foreach (var item in items)
		{
			// Use the full path in archive to determine where to extract within the quick extract dir. The quick
			// extract option preserves the structure of the nefs archive within the quick extract dir.
			var path = Archive.Items.GetItemFilePath(item.Id);
			var fullPath = Path.Combine(baseDir, path);
			var dir = Path.GetDirectoryName(fullPath) ?? "";
			extractionList.AddRange(GetExtractionList(item, Archive.Items, dir));
		}

		// Extract the files
		return await ExtractFilesAsync(extractionList);
	}

	/// <inheritdoc/>
	public async Task<bool> OpenArchiveAsync(NefsArchiveSource source)
	{
		// Close existing archive if needed
		if (Archive != null)
		{
			if (!await CloseArchiveAsync())
			{
				// User canceled close or close failed
				return false;
			}
		}

		switch (source)
		{
			case StandardSource standard:
				return await OpenStandardArchiveAsync(standard);

			case HeadlessSource gameDatSource:
				return await OpenGameDatArchiveAsync(gameDatSource);

			case NefsInjectSource nefsInjectSource:
				return await OpenNefsInjectArchiveAsync(nefsInjectSource);

			default:
				throw new ArgumentException("Unknown archive source type.");
		}
	}

	/// <inheritdoc/>
	public async Task<bool> OpenArchiveAsync(string filePath)
	{
		var source = NefsArchiveSource.Standard(filePath);
		return await OpenArchiveAsync(source);
	}

	/// <inheritdoc/>
	public async Task<bool> OpenArchiveByDialogAsync()
	{
		(var result, var source) = UiService.ShowNefsEditOpenFileDialog(SettingsService, ProgressService, NefsReader);
		if (result != DialogResult.OK || source is null)
		{
			return false;
		}

		return await OpenArchiveAsync(source);
	}

	/// <inheritdoc/>
	public void Redo()
	{
		if (UndoBuffer.Redo())
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
		(var result, var fileName) = UiService.ShowOpenFileDialog();
		if (result != DialogResult.OK)
		{
			return false;
		}

		// Check file exists
		if (!FileSystem.File.Exists(fileName))
		{
			Log.LogError($"Cannot replace item. Replacement file does not exist: {fileName}.");
			return false;
		}

		var fileSize = FileSystem.FileInfo.New(fileName).Length;
		var itemSize = new NefsItemSize((uint)fileSize);
		var newDataSource = new NefsFileDataSource(fileName, 0, itemSize, false);
		var cmd = new ReplaceFileCommand(item, item.DataSource, item.State, newDataSource);
		UndoBuffer.Execute(cmd);
		return true;
	}

	/// <inheritdoc/>
	public bool ReplaceSeletedItemByDialog()
	{
		if (SelectedItems.Count == 0)
		{
			UiService.ShowMessageBox("No item selected to replace.");
			return false;
		}

		if (SelectedItems.Count > 1)
		{
			UiService.ShowMessageBox("Replacing multiple files not supported.");
			return false;
		}

		if (SelectedItems[0].Type == NefsItemType.Directory)
		{
			UiService.ShowMessageBox("Replacing directories not supported.");
			return false;
		}

		return ReplaceItemByDialog(SelectedItems[0]);
	}

	/// <inheritdoc/>
	public async Task<bool> SaveArchiveAsync()
	{
		// TODO : Forcing save-as for now.
		return await SaveArchiveByDialogAsync();
	}

	/// <inheritdoc/>
	public async Task<bool> SaveArchiveAsync(string destFilePath)
	{
		return await DoSaveStandardArchiveAsync(destFilePath);
	}

	/// <inheritdoc/>
	public async Task<bool> SaveArchiveByDialogAsync()
	{
		if (Archive == null)
		{
			Log.LogError("Failed to save archive: no archive open.");
			return false;
		}

		var fileName = Path.GetFileName(Archive.Items.DataFilePath);
		var fileExt = Path.GetExtension(fileName);
		var filter = $"*{fileExt}|*{fileExt}";

		var (result, path) = UiService.ShowSaveFileDialog(fileName, filter);
		if (result != DialogResult.OK)
		{
			return false;
		}

		switch (ArchiveSource)
		{
			case StandardSource _:
				return await DoSaveStandardArchiveAsync(path);

			case NefsInjectSource _:
			case HeadlessSource _:
				var nefsInjectFilePath = path + ".nefsinject";
				return await DoSaveNefsInjectArchiveAsync(path, nefsInjectFilePath);

			default:
				throw new ArgumentException("Unknown archive source type.");
		}
	}

	/// <inheritdoc/>
	public void SelectItem(NefsItem item)
	{
		SelectItems(new List<NefsItem> { item });
	}

	/// <inheritdoc/>
	public void SelectItems(IEnumerable<NefsItem> items)
	{
		this.selectedItems = new List<NefsItem>(items);
		SelectedItemsChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <inheritdoc/>
	public void Undo()
	{
		if (UndoBuffer.Undo())
		{
			Log.LogInformation("Undo executed.");
		}
	}

	private async Task<bool> DoOpenArchiveAsync(NefsArchiveSource source)
	{
		var result = false;

		// Open archive
		await ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
		{
			try
			{
				using (p.BeginTask(0.9f, "Reading archive"))
				{
					Archive = await NefsReader.ReadArchiveAsync(source, p);
					ArchiveSource = source;
				}

				using (p.BeginTask(0.1f, "Preparing UI"))
				{
					ArchiveOpened?.Invoke(this, EventArgs.Empty);
				}

				result = true;
				Log.LogInformation("Archive opened.");
			}
			catch (Exception ex)
			{
				Log.LogError(ex, $"Failed to open archive {source.FilePath}.");
			}
		}));

		return result;
	}

	private async Task<bool> DoSaveNefsInjectArchiveAsync(string dataFilePath, string nefsInjectFilePath)
	{
		if (Archive == null)
		{
			Log.LogError("Failed to save archive: no archive open.");
			return false;
		}

		// Currently don't support saving encrypted archives
		if (Archive.Header.IsEncrypted)
		{
			UiService.ShowMessageBox("Saving encrypted archives is not supported.", icon: MessageBoxIcon.Error);
			return false;
		}

		Log.LogInformation("----------------------------");
		Log.LogInformation($"Writing NefsInject archive.");
		Log.LogInformation($"Data file: {dataFilePath}.");
		Log.LogInformation($"NefsInject file: {nefsInjectFilePath}.");
		var result = false;

		// Save archive
		await ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
		{
			using (var tt = p.BeginTask(1.0f))
			{
				// Save archive
				try
				{
					var (archive, source) = await NefsWriter.WriteNefsInjectArchiveAsync(dataFilePath, nefsInjectFilePath, Archive, p);
					Archive = archive;
					ArchiveSource = source;
					UndoBuffer.MarkAsSaved();

					ArchiveSaved?.Invoke(this, EventArgs.Empty);
					result = true;

					Log.LogInformation($"Archive saved.");
				}
				catch (Exception ex)
				{
					Log.LogError($"Failed to saved archive {dataFilePath}.\r\n{ex.Message}");
				}
			}
		}));

		return result;
	}

	private async Task<bool> DoSaveStandardArchiveAsync(string filePath)
	{
		if (Archive == null)
		{
			Log.LogError("Failed to save archive: no archive open.");
			return false;
		}

		// Currently don't support saving encrypted archives
		if (Archive.Header.IsEncrypted)
		{
			UiService.ShowMessageBox("Saving encrypted archives is not supported.", icon: MessageBoxIcon.Error);
			return false;
		}

		Log.LogInformation("----------------------------");
		Log.LogInformation($"Writing archive: {filePath}.");
		var result = false;

		// Save archive
		await ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
		{
			using (var tt = p.BeginTask(1.0f))
			{
				// Save archive
				try
				{
					var (archive, source) = await NefsWriter.WriteArchiveAsync(filePath, Archive, p);
					Archive = archive;
					ArchiveSource = source;
					UndoBuffer.MarkAsSaved();

					ArchiveSaved?.Invoke(this, EventArgs.Empty);
					result = true;

					Log.LogInformation($"Archive saved.");
				}
				catch (Exception ex)
				{
					Log.LogError($"Failed to saved archive {filePath}.\r\n{ex.Message}");
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
			if (!FileSystem.Directory.Exists(dir))
			{
				FileSystem.Directory.CreateDirectory(dir);
			}

			// Extract the file
			await NefsTransformer.DetransformFileAsync(
				item.DataSource.FilePath,
				item.DataSource.Offset,
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
		await ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
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
						await ExtractFileAsync(item, filePath, p);
					}
				}

				Log.LogInformation("Extraction complete.");
			}
		}));

		return true;
	}

	/// <summary>
	/// Gets a list of files to be extracted. Each file is paired with an output path to where the content will be
	/// extracted. Directories are not included in the list, but they will be recursively processed for descendant files.
	/// </summary>
	/// <remarks>
	/// The purpose for this function is to provide a list of all files that will be extracted. This allows the progress
	/// reporting to be accurate (allows knowing the total number of items to be extracted before starting).
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
				items.AddRange(GetExtractionList(child, itemsList, path));
			}
		}
		else
		{
			// Item is file, add self
			items.Add((item, path));
		}

		return items;
	}

	private async Task<bool> OpenGameDatArchiveAsync(HeadlessSource source)
	{
		Log.LogInformation("----------------------------");
		Log.LogInformation($"Opening archive:");
		Log.LogInformation($"Data file: {source.FilePath}");
		Log.LogInformation($"Header file: {source.HeaderFilePath}");
		Log.LogInformation($"Primary offset: {source.PrimaryOffset:X}");
		Log.LogInformation($"Primary size: {source.PrimarySize:X}");
		Log.LogInformation($"Secondary offset: {source.SecondaryOffset:X}");
		Log.LogInformation($"Secondary size: {source.SecondarySize:X}");

		if (!FileSystem.File.Exists(source.FilePath))
		{
			Log.LogError($"Data file not found: {source.FilePath}.");
			return false;
		}

		if (!FileSystem.File.Exists(source.HeaderFilePath))
		{
			Log.LogError($"Header file not found: {source.HeaderFilePath}.");
			return false;
		}

		return await DoOpenArchiveAsync(source);
	}

	private async Task<bool> OpenNefsInjectArchiveAsync(NefsInjectSource source)
	{
		Log.LogInformation("----------------------------");
		Log.LogInformation($"Opening archive: {source.DataFilePath}");
		Log.LogInformation($"NefsInject file: {source.NefsInjectFilePath}");

		if (!FileSystem.File.Exists(source.DataFilePath))
		{
			Log.LogError($"File not found: {source.DataFilePath}.");
			return false;
		}

		if (!FileSystem.File.Exists(source.NefsInjectFilePath))
		{
			Log.LogError($"File not found: {source.NefsInjectFilePath}.");
			return false;
		}

		return await DoOpenArchiveAsync(source);
	}

	private async Task<bool> OpenStandardArchiveAsync(StandardSource source)
	{
		Log.LogInformation("----------------------------");
		Log.LogInformation($"Opening archive: {source.FilePath}");

		if (!FileSystem.File.Exists(source.FilePath))
		{
			Log.LogError($"File not found: {source.FilePath}.");
			return false;
		}

		return await DoOpenArchiveAsync(source);
	}
}
