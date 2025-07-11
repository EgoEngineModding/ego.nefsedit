// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using System.Globalization;
using System.IO.Abstractions;
using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.Settings;
using VictorBush.Ego.NefsEdit.Utility;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsEdit.UI;

/// <summary>
/// Dialog for opening archives.
/// </summary>
internal partial class OpenFileForm : Form
{
	private static readonly ILogger Log = LogHelper.GetLogger();

	private readonly OpenMode openModeHeadless = new OpenMode("Headless");
	private readonly OpenMode openModeHeadlessCustom = new OpenMode("Headless (Custom)");
	private readonly OpenMode openModeNefs = new OpenMode("NeFS");
	private readonly OpenMode openModeRecent = new OpenMode("Recent");

	/// <summary>
	/// Initializes a new instance of the <see cref="OpenFileForm"/> class.
	/// </summary>
	/// <param name="settingsService">The settings service.</param>
	/// <param name="uiService">The UI service.</param>
	/// <param name="progressService">Progress service.</param>
	/// <param name="reader">Nefs reader.</param>
	/// <param name="fileSystem">The file system.</param>
	/// <param name="exeHeaderFinder">The exe header finder.</param>
	public OpenFileForm(
		ISettingsService settingsService,
		IUiService uiService,
		IProgressService progressService,
		INefsReader reader,
		IFileSystem fileSystem,
		INefsExeHeaderFinder exeHeaderFinder)
	{
		InitializeComponent();
		SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
		UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
		ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
		Reader = reader ?? throw new ArgumentNullException(nameof(reader));
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		ExeHeaderFinder = exeHeaderFinder;
	}

	/// <summary>
	/// Source of the archive to open.
	/// </summary>
	public NefsArchiveSource? ArchiveSource { get; private set; }

	private IFileSystem FileSystem { get; }

	private INefsExeHeaderFinder ExeHeaderFinder { get; }

	private IProgressService ProgressService { get; }

	private INefsReader Reader { get; }

	private ISettingsService SettingsService { get; }

	private IUiService UiService { get; }

	private static bool ParseHexNumberStringToInt(string input, out int result)
	{
		var str = input.Replace("0x", "").Replace("&h", "").Trim();
		return int.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
	}

	private static bool ParseHexNumberStringToLong(string input, out long result)
	{
		var str = input.Replace("0x", "").Replace("&h", "").Trim();
		return long.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
	}

	private async Task<List<HeadlessSource>> FindExeHeadersAsync(string exePath, string dataDirectory, NefsProgress p)
	{
		if (!FileSystem.File.Exists(exePath))
		{
			UiService.ShowMessageBox($"Cannot find executable file: {exePath}.");
			return [];
		}

		// Search for headers in the exe
		using var _ = p.BeginTask(1.0f, "Searching for headers");
		return await ExeHeaderFinder.FindHeadersAsync(exePath, dataDirectory, p);
	}

	private async void GameDatRefreshButton_Click(Object sender, EventArgs e)
	{
		this.gameDatFilesListBox.Items.Clear();

		await ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
		{
			var files = await FindExeHeadersAsync(this.headlessGameExeFileTextBox.Text,
				this.headlessDataDirTextBox.Text, p).ConfigureAwait(false);

			// Update on UI thread
			UiService.Dispatcher.Invoke(() =>
			{
				foreach (var file in files)
				{
					var item = new HeadlessFileItem(file);
					this.gameDatFilesListBox.Items.Add(item);
				}
			});
		}));
	}

	private void GameExeFileButton_Click(Object sender, EventArgs e)
	{
		var (result, path) = UiService.ShowOpenFileDialog("Executable (*.exe)|*.exe;*.elf;*.bin;*");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.headlessGameExeFileTextBox.Text = path;
		this.headlessGameExeFileTextBox.ScrollToEnd();
	}

	private void HeadlessDataDirButton_Click(Object sender, EventArgs e)
	{
		var (result, path) = UiService.ShowFolderBrowserDialog("Choose directory where data files are stored.");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.headlessDataDirTextBox.Text = path;
		this.headlessDataDirTextBox.ScrollToEnd();
	}

	private void LoadSettings()
	{
		this.recentListBox.Items.Clear();

		foreach (var recent in SettingsService.RecentFiles)
		{
			this.recentListBox.Items.Add(recent);
		}

		var modeIdx = SettingsService.OpenFileDialogState.LastMode;
		this.modeListBox.SelectedIndex = modeIdx < this.modeListBox.Items.Count ? modeIdx : 0;
		this.nefsFileTextBox.Text = SettingsService.OpenFileDialogState.NefsFilePath;
		this.splitDataFileTextBox.Text = SettingsService.OpenFileDialogState.GameDatDataFilePath;
		this.splitHeaderFileTextBox.Text = SettingsService.OpenFileDialogState.GameDatHeaderFilePath;
		this.splitPrimaryOffsetTextBox.Text = SettingsService.OpenFileDialogState.GameDatPrimaryOffset;
		this.splitPrimarySizeTextBox.Text = SettingsService.OpenFileDialogState.GameDatPrimarySize;
		this.splitSecondaryOffsetTextBox.Text = SettingsService.OpenFileDialogState.GameDatSecondaryOffset;
		this.splitSecondarySizeTextBox.Text = SettingsService.OpenFileDialogState.GameDatSecondarySize;
		this.headlessGameExeFileTextBox.Text = SettingsService.OpenFileDialogState.HeadlessExePath;
		this.headlessDataDirTextBox.Text = SettingsService.OpenFileDialogState.HeadlessDataDirPath;
	}

	private void ModeListBox_SelectedIndexChanged(Object sender, EventArgs e)
	{
		if (this.modeListBox.SelectedItem == this.openModeNefs)
		{
			// Open NeFS archive
			this.tablessControl1.SelectedTab = this.nefsTabPage;
		}
		else if (this.modeListBox.SelectedItem == this.openModeRecent)
		{
			// Open recent
			this.tablessControl1.SelectedTab = this.recentTabPage;
		}
		else if (this.modeListBox.SelectedItem == this.openModeHeadless)
		{
			this.tablessControl1.SelectedTab = this.headlessTabPage;
		}
		else if (this.modeListBox.SelectedItem == this.openModeHeadlessCustom)
		{
			this.tablessControl1.SelectedTab = this.headlessCustomTabPage;
		}
	}

	private void NefsFileButton_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowOpenFileDialog("NeFS Archive|*.nefs;*.nfs");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.nefsFileTextBox.Text = path;
		this.nefsFileTextBox.ScrollToEnd();
	}

	private void OpenButton_Click(Object sender, EventArgs e)
	{
		NefsArchiveSource? source = null;

		if (this.modeListBox.SelectedItem == this.openModeNefs)
		{
			source = ValidateStandardSource();
		}
		else if (this.modeListBox.SelectedItem == this.openModeHeadless)
		{
			source = ValidateHeadlessSource();
		}
		else if (this.modeListBox.SelectedItem == this.openModeHeadlessCustom)
		{
			source = ValidateHeadlessCustomSource();
		}
		else if (this.modeListBox.SelectedItem == this.openModeRecent)
		{
			source = ValidateRecent();
		}

		if (source != null)
		{
			DialogResult = DialogResult.OK;
			ArchiveSource = source;

			var recentFile = new RecentFile(source);

			// Remove recent file from the list if it exists, then add to the top of the list
			SettingsService.RecentFiles.Remove(recentFile);
			SettingsService.RecentFiles.Insert(0, recentFile);

			Close();
		}
	}

	private void OpenFileForm_FormClosing(Object sender, FormClosingEventArgs e)
	{
		SaveSettings();
	}

	private void OpenFileForm_Load(Object sender, EventArgs e)
	{
		// Setup combo box
		this.modeListBox.Items.Add(this.openModeNefs);
		this.modeListBox.Items.Add(this.openModeHeadless);
		this.modeListBox.Items.Add(this.openModeHeadlessCustom);
		this.modeListBox.Items.Add(this.openModeRecent);

		// Select default open mode
		this.modeListBox.SelectedItem = this.openModeNefs;

		// Load settings
		LoadSettings();
	}

	/// <summary>
	/// Saves the current open file dialog state and recently opened files.
	/// </summary>
	private void SaveSettings()
	{
		// Only keep 10 recent items
		if (SettingsService.RecentFiles.Count > 10)
		{
			SettingsService.RecentFiles.RemoveAt(SettingsService.RecentFiles.Count - 1);
		}

		SettingsService.OpenFileDialogState.LastMode = this.modeListBox.SelectedIndex;
		SettingsService.OpenFileDialogState.NefsFilePath = this.nefsFileTextBox.Text;
		SettingsService.OpenFileDialogState.GameDatDataFilePath = this.splitDataFileTextBox.Text;
		SettingsService.OpenFileDialogState.GameDatHeaderFilePath = this.splitHeaderFileTextBox.Text;
		SettingsService.OpenFileDialogState.GameDatPrimaryOffset = this.splitPrimaryOffsetTextBox.Text;
		SettingsService.OpenFileDialogState.GameDatPrimarySize = this.splitPrimarySizeTextBox.Text;
		SettingsService.OpenFileDialogState.GameDatSecondaryOffset = this.splitSecondaryOffsetTextBox.Text;
		SettingsService.OpenFileDialogState.GameDatSecondarySize = this.splitSecondarySizeTextBox.Text;
		SettingsService.OpenFileDialogState.HeadlessExePath = this.headlessGameExeFileTextBox.Text;
		SettingsService.OpenFileDialogState.HeadlessDataDirPath = this.headlessDataDirTextBox.Text;

		SettingsService.Save();
	}

	private void splitDataFileBrowseButton_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowOpenFileDialog();
		if (result != DialogResult.OK)
		{
			return;
		}

		this.splitDataFileTextBox.Text = path;
		this.splitDataFileTextBox.ScrollToEnd();
	}

	private void splitHeaderFileBrowseButton_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowOpenFileDialog();
		if (result != DialogResult.OK)
		{
			return;
		}

		this.splitHeaderFileTextBox.Text = path;
		this.splitHeaderFileTextBox.ScrollToEnd();
	}

	private bool ValidateFileExists(string file)
	{
		if (!FileSystem.File.Exists(file))
		{
			UiService.ShowMessageBox($"Cannot find file: {file}.");
			return false;
		}

		return true;
	}

	private NefsArchiveSource? ValidateHeadlessSource()
	{
		var selectedItem = this.gameDatFilesListBox.SelectedItem as HeadlessFileItem;
		if (selectedItem == null)
			return null;

		var source = selectedItem.Source;
		if (!ValidateFileExists(source.DataFilePath))
			return null;

		if (!ValidateFileExists(source.HeaderFilePath))
			return null;

		return source;
	}

	private NefsArchiveSource? ValidateHeadlessCustomSource()
	{
		// Primary offset
		var primaryOffsetString = this.splitPrimaryOffsetTextBox.Text.Trim();
		if (string.IsNullOrWhiteSpace(primaryOffsetString))
		{
			primaryOffsetString = "0";
		}

		if (!ParseHexNumberStringToLong(primaryOffsetString, out var primaryOffset))
		{
			UiService.ShowMessageBox($"Invalid primary offset {primaryOffsetString}.");
			return null;
		}

		// Secondary offset
		var secondaryOffsetString = this.splitSecondaryOffsetTextBox.Text.Trim();
		if (string.IsNullOrWhiteSpace(secondaryOffsetString))
		{
			secondaryOffsetString = "0";
		}

		if (!ParseHexNumberStringToLong(secondaryOffsetString, out var secondaryOffset))
		{
			UiService.ShowMessageBox($"Invalid secondary offset {secondaryOffsetString}.");
			return null;
		}

		// Primary size
		int? primarySize = null;
		var primarySizeString = this.splitPrimarySizeTextBox.Text.Trim();
		if (!string.IsNullOrWhiteSpace(primarySizeString))
		{
			if (!ParseHexNumberStringToInt(primarySizeString, out var size))
			{
				UiService.ShowMessageBox($"Invalid primary size {primarySizeString}.");
				return null;
			}

			primarySize = size;
		}

		// Secondary size
		int? secondarySize = null;
		var secondarySizeString = this.splitSecondarySizeTextBox.Text.Trim();
		if (!string.IsNullOrWhiteSpace(secondarySizeString))
		{
			if (!ParseHexNumberStringToInt(secondarySizeString, out var size))
			{
				UiService.ShowMessageBox($"Invalid secondary size {secondarySizeString}.");
				return null;
			}

			secondarySize = size;
		}

		var headerFile = this.splitHeaderFileTextBox.Text.Trim();
		var dataFile = this.splitDataFileTextBox.Text.Trim();
		var source = NefsArchiveSource.Headless(dataFile, headerFile, primaryOffset, primarySize, secondaryOffset, secondarySize);

		if (!ValidateFileExists(source.DataFilePath))
		{
			return null;
		}

		if (!ValidateFileExists(source.HeaderFilePath))
		{
			return null;
		}

		return source;
	}

	private NefsArchiveSource? ValidateRecent()
	{
		var recent = this.recentListBox.SelectedItem as RecentFile;
		if (recent == null)
		{
			return null;
		}

		var source = recent.ToArchiveSource();
		switch (source)
		{
			case StandardSource standardSource:
				if (!ValidateFileExists(standardSource.FilePath))
					return null;
				return standardSource;

			case HeadlessSource gameDatSource:
				if (!ValidateFileExists(gameDatSource.DataFilePath))
					return null;
				if (!ValidateFileExists(gameDatSource.HeaderFilePath))
					return null;
				return gameDatSource;

			default:
				UiService.ShowMessageBox($"Unknown archive source type.");
				return null;
		}
	}

	private NefsArchiveSource? ValidateStandardSource()
	{
		var headerFile = this.nefsFileTextBox.Text;
		var source = NefsArchiveSource.Standard(headerFile);

		if (!FileSystem.File.Exists(source.FilePath))
		{
			UiService.ShowMessageBox($"Cannot find file: {source.FilePath}.");
			return null;
		}

		return source;
	}

	private class HeadlessFileItem
	{
		public HeadlessFileItem(HeadlessSource source)
		{
			Source = source ?? throw new ArgumentNullException(nameof(source));
		}

		public HeadlessSource Source { get; }

		public override String ToString()
		{
			return $"{Source.FileName} (header offset: {Source.PrimaryOffset})";
		}
	}

	private class OpenMode
	{
		public OpenMode(string mode)
		{
			Mode = mode;
		}

		public string Mode { get; set; }

		public override String ToString()
		{
			return Mode;
		}
	}
}
