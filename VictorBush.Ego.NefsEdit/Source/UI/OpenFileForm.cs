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

	private readonly OpenMode openModeGameBinDirtRally1 = new OpenMode("game*.bin (DiRT Rally)");
	private readonly OpenMode openModeGameDatCustom = new OpenMode("game*.dat/bin (Custom)");
	private readonly OpenMode openModeGameDatDirt4 = new OpenMode("game*.dat (DiRT 4)");
	private readonly OpenMode openModeGameDatDirtRally2 = new OpenMode("game*.dat (DiRT Rally 2)");
	private readonly OpenMode openModeNefs = new OpenMode("NeFS");
	private readonly OpenMode openModeNefsInject = new OpenMode("NefsInject");
	private readonly OpenMode openModeRecent = new OpenMode("Recent");

	//private string gameDatCustomDirPath = "";
	//private string gameDatCustomExePath = "";

	/// <summary>
	/// Initializes a new instance of the <see cref="OpenFileForm"/> class.
	/// </summary>
	/// <param name="settingsService">The settings service.</param>
	/// <param name="uiService">The UI service.</param>
	/// <param name="progressService">Progress service.</param>
	/// <param name="reader">Nefs reader.</param>
	/// <param name="fileSystem">The file system.</param>
	public OpenFileForm(
		ISettingsService settingsService,
		IUiService uiService,
		IProgressService progressService,
		INefsReader reader,
		IFileSystem fileSystem)
	{
		InitializeComponent();
		SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
		UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
		ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
		Reader = reader ?? throw new ArgumentNullException(nameof(reader));
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
	}

	/// <summary>
	/// Source of the archive to open.
	/// </summary>
	public NefsArchiveSource ArchiveSource { get; private set; }

	private IFileSystem FileSystem { get; }

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

	/// <summary>
	/// Looks through the game executable to find header offsets for game.dat files.
	/// </summary>
	/// <returns>A list of game.dat archive sources.</returns>
	private async Task<List<HeadlessSource>> FindGameDatHeaderOffsetsAsync(
		string gameDatDir,
		string gameExePath,
		NefsProgress p)
	{
		// TODO : FIX ME
		UiService.ShowMessageBox("Disabled.");
		return new List<HeadlessSource>();

		if (!FileSystem.File.Exists(gameExePath))
		{
			UiService.ShowMessageBox($"Cannot find executable file: {gameExePath}.");
			return new List<HeadlessSource>();
		}

		// Search for headers in the exe
		using (var t = p.BeginTask(1.0f, "Searching for headers"))
		{
			// TODO : FIX ME return await this.Reader.FindHeadersAsync(gameExePath, gameDatDir, p);
		}
	}

	private void GameDatDirButton_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowFolderBrowserDialog("Choose directory where game.dat files are stored.");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.gameDatDirTextBox.Text = path;
		this.gameDatDirTextBox.ScrollToEnd();
	}

	private async void GameDatRefreshButton_Click(Object sender, EventArgs e)
	{
		this.gameDatFilesListBox.Items.Clear();

		await ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
		{
			var files = await FindGameDatHeaderOffsetsAsync(
				this.gameDatDirTextBox.Text,
				this.gameExeFileTextBox.Text,
				p);

			// Update on UI thread
			UiService.Dispatcher.Invoke(() =>
			{
				foreach (var file in files)
				{
					var item = new GameDatFileItem(file);
					this.gameDatFilesListBox.Items.Add(item);
				}
			});
		}));
	}

	private void GameExeFileButton_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowOpenFileDialog("Executable (*.exe)|*.exe");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.gameExeFileTextBox.Text = path;
		this.gameExeFileTextBox.ScrollToEnd();
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
		this.nefsInjectDataFileTextBox.Text = SettingsService.OpenFileDialogState.NefsInjectDataFilePath;
		this.nefsInjectFileTextBox.Text = SettingsService.OpenFileDialogState.NefsInjectFilePath;
		//this.gameDatCustomExePath = this.SettingsService.OpenFileDialogState.GameDatCustomExePath;
		//this.gameDatCustomDirPath = this.SettingsService.OpenFileDialogState.GameDatCustomDatDirPath;
	}

	private void ModeListBox_SelectedIndexChanged(Object sender, EventArgs e)
	{
		if (this.modeListBox.SelectedItem == this.openModeNefs)
		{
			// Open NeFS archive
			this.tablessControl1.SelectedTab = this.nefsTabPage;
		}
		else if (this.modeListBox.SelectedItem == this.openModeNefsInject)
		{
			this.tablessControl1.SelectedTab = this.nefsInjectTabPage;
		}
		else if (this.modeListBox.SelectedItem == this.openModeRecent)
		{
			// Open recent
			this.tablessControl1.SelectedTab = this.recentTabPage;
		}
		else if (this.modeListBox.SelectedItem == this.openModeGameBinDirtRally1)
		{
			// Open a game*.bin file (DiRT Rally 1)
			this.tablessControl1.SelectedTab = this.gameDatTabPage;
			this.gameExeFileTextBox.Text = SettingsService.DirtRally1Exe;
			this.gameExeFileTextBox.ScrollToEnd();
			this.gameDatDirTextBox.Text = SettingsService.DirtRally1GameBinDir;
			this.gameDatDirTextBox.ScrollToEnd();
			this.gameDatFilesListBox.Items.Clear();
		}
		else if (this.modeListBox.SelectedItem == this.openModeGameDatDirtRally2)
		{
			// Open a game*.dat file (DiRT Rally 2)
			this.tablessControl1.SelectedTab = this.gameDatTabPage;
			this.gameExeFileTextBox.Text = SettingsService.DirtRally2Exe;
			this.gameExeFileTextBox.ScrollToEnd();
			this.gameDatDirTextBox.Text = SettingsService.DirtRally2GameDatDir;
			this.gameDatDirTextBox.ScrollToEnd();
			this.gameDatFilesListBox.Items.Clear();
		}
		else if (this.modeListBox.SelectedItem == this.openModeGameDatDirt4)
		{
			// Open a game*.dat file (DiRT 4)
			this.tablessControl1.SelectedTab = this.gameDatTabPage;
			this.gameExeFileTextBox.Text = SettingsService.Dirt4Exe;
			this.gameExeFileTextBox.ScrollToEnd();
			this.gameDatDirTextBox.Text = SettingsService.Dirt4GameDatDir;
			this.gameDatDirTextBox.ScrollToEnd();
			this.gameDatFilesListBox.Items.Clear();
		}
		else if (this.modeListBox.SelectedItem == this.openModeGameDatCustom)
		{
			// Search an executable for game.bin/game.dat files
			//this.tablessControl1.SelectedTab = this.gameDatTabPage;
			//this.gameExeFileTextBox.Text = this.gameDatCustomExePath;
			//this.gameExeFileTextBox.ScrollToEnd();
			//this.gameDatDirTextBox.Text = this.gameDatCustomDirPath;
			//this.gameDatDirTextBox.ScrollToEnd();
			//this.gameDatFilesListBox.Items.Clear();

			this.tablessControl1.SelectedTab = this.gameDatCustomTabPage;
		}
	}

	private void NefsFileButton_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowOpenFileDialog("NeFS Archive (*.nefs)|*.nefs");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.nefsFileTextBox.Text = path;
		this.nefsFileTextBox.ScrollToEnd();
	}

	private void OpenButton_Click(Object sender, EventArgs e)
	{
		NefsArchiveSource source = null;

		if (this.modeListBox.SelectedItem == this.openModeNefs)
		{
			source = ValidateStandardSource();
		}
		else if (this.modeListBox.SelectedItem == this.openModeNefsInject)
		{
			source = ValidateNefsInjectSource();
		}
		else if (this.modeListBox.SelectedItem == this.openModeGameDatDirtRally2)
		{
			source = ValidateGameDatSearchSource();
		}
		else if (this.modeListBox.SelectedItem == this.openModeGameDatDirt4)
		{
			source = ValidateGameDatSearchSource();
		}
		else if (this.modeListBox.SelectedItem == this.openModeGameBinDirtRally1)
		{
			source = ValidateGameDatSearchSource();
		}
		else if (this.modeListBox.SelectedItem == this.openModeGameDatCustom)
		{
			source = ValidateGameDatCustomSource();
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
		this.modeListBox.Items.Add(this.openModeNefsInject);
		this.modeListBox.Items.Add(this.openModeGameDatCustom);
		this.modeListBox.Items.Add(this.openModeRecent);
		this.modeListBox.Items.Add(this.openModeGameBinDirtRally1);
		this.modeListBox.Items.Add(this.openModeGameDatDirtRally2);
		this.modeListBox.Items.Add(this.openModeGameDatDirt4);

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
		SettingsService.OpenFileDialogState.NefsInjectDataFilePath = this.nefsInjectDataFileTextBox.Text;
		SettingsService.OpenFileDialogState.NefsInjectFilePath = this.nefsInjectFileTextBox.Text;
		//this.SettingsService.OpenFileDialogState.GameDatCustomDatDirPath = this.gameDatCustomDirPath;
		//this.SettingsService.OpenFileDialogState.GameDatCustomExePath = this.gameDatCustomExePath;

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

	private NefsArchiveSource ValidateGameDatCustomSource()
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

	private NefsArchiveSource ValidateGameDatSearchSource()
	{
		var selectedItem = this.gameDatFilesListBox.SelectedItem as GameDatFileItem;
		if (selectedItem == null)
			return null;

		var source = selectedItem.Source;
		if (!ValidateFileExists(source.DataFilePath))
			return null;

		if (!ValidateFileExists(source.HeaderFilePath))
			return null;

		return source;
	}

	private NefsArchiveSource ValidateNefsInjectSource()
	{
		var dataFilePath = this.nefsInjectDataFileTextBox.Text;
		var headerFilePath = this.nefsInjectFileTextBox.Text;
		var source = NefsArchiveSource.NefsInject(dataFilePath, headerFilePath);

		if (!ValidateFileExists(source.DataFilePath))
			return null;

		if (!ValidateFileExists(source.NefsInjectFilePath))
			return null;

		return source;
	}

	private NefsArchiveSource ValidateRecent()
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

			case NefsInjectSource nefsInjectSource:
				if (!ValidateFileExists(nefsInjectSource.NefsInjectFilePath))
					return null;
				if (!ValidateFileExists(nefsInjectSource.DataFilePath))
					return null;
				return nefsInjectSource;

			default:
				UiService.ShowMessageBox($"Unknown archive source type.");
				return null;
		}
	}

	private NefsArchiveSource ValidateStandardSource()
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

	//private bool ValidateSource(NefsArchiveSource source)
	//{
	//    if (!this.FileSystem.File.Exists(source.HeaderFilePath))
	//    {
	//        this.UiService.ShowMessageBox($"Cannot find file: {source.HeaderFilePath}.");
	//        return false;
	//    }

	// if (!this.FileSystem.File.Exists(source.FilePath)) { this.UiService.ShowMessageBox($"Cannot find file:
	// {source.FilePath}."); return false; }

	//    return true;
	//}

	private class GameDatFileItem
	{
		public GameDatFileItem(HeadlessSource source)
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
