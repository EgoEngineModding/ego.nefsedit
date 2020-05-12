// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Microsoft.Extensions.Logging;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Settings;
    using VictorBush.Ego.NefsEdit.Utility;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.IO;
    using VictorBush.Ego.NefsLib.Progress;

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
        private readonly OpenMode openModeRecent = new OpenMode("Recent");

        private string gameDatCustomDirPath = "";
        private string gameDatCustomExePath = "";

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
            this.InitializeComponent();
            this.SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            this.ProgressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            this.Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
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

        private void AdvancedCheckBox_CheckedChanged(Object sender, EventArgs e)
        {
            this.advancedGroupBox.Enabled = this.advancedCheckBox.Checked;
        }

        private void DataFileBrowseButton_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowOpenFileDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            this.dataFileTextBox.Text = path;
            this.dataFileTextBox.ScrollToEnd();
        }

        /// <summary>
        /// Looks through the game executable to find header offsets for game.dat files.
        /// </summary>
        /// <returns>A list of game.dat archive sources.</returns>
        private async Task<List<NefsArchiveSource>> FindGameDatHeaderOffsetsAsync(
            string gameDatDir,
            string gameExePath,
            NefsProgress p)
        {
            if (!this.FileSystem.File.Exists(gameExePath))
            {
                this.UiService.ShowMessageBox($"Cannot find executable file: {gameExePath}.");
                return new List<NefsArchiveSource>();
            }

            // Search for headers in the exe
            using (var t = p.BeginTask(1.0f, "Searching for headers"))
            {
                return await this.Reader.FindHeadersAsync(gameExePath, gameDatDir, p);
            }
        }

        private void GameDatDirButton_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowFolderBrowserDialog("Choose directory where game.dat files are stored.");
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

            await this.ProgressService.RunModalTaskAsync(p => Task.Run(async () =>
            {
                var files = await this.FindGameDatHeaderOffsetsAsync(
                    this.gameDatDirTextBox.Text,
                    this.gameExeFileTextBox.Text,
                    p);

                // Update on UI thread
                this.UiService.Dispatcher.Invoke(() =>
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
            (var result, var path) = this.UiService.ShowOpenFileDialog("Executable (*.exe)|*.exe");
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

            foreach (var recent in this.SettingsService.RecentFiles)
            {
                this.recentListBox.Items.Add(recent);
            }

            var modeIdx = this.SettingsService.OpenFileDialogState.LastMode;
            this.modeListBox.SelectedIndex = modeIdx < this.modeListBox.Items.Count ? modeIdx : 0;
            this.nefsFileTextBox.Text = this.SettingsService.OpenFileDialogState.HeaderPath;
            this.dataFileTextBox.Text = this.SettingsService.OpenFileDialogState.DataFilePath;
            this.headerOffsetTextBox.Text = this.SettingsService.OpenFileDialogState.HeaderOffset;
            this.headerPart6OffsetTextBox.Text = this.SettingsService.OpenFileDialogState.HeaderPart6Offset;
            this.advancedCheckBox.Checked = this.SettingsService.OpenFileDialogState.IsAdvanced;
            this.advancedGroupBox.Enabled = this.advancedCheckBox.Checked;
            this.gameDatCustomExePath = this.SettingsService.OpenFileDialogState.GameDatCustomExePath;
            this.gameDatCustomDirPath = this.SettingsService.OpenFileDialogState.GameDatCustomDatDirPath;
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
            else if (this.modeListBox.SelectedItem == this.openModeGameBinDirtRally1)
            {
                // Open a game*.bin file (DiRT Rally 1)
                this.tablessControl1.SelectedTab = this.gameDatTabPage;
                this.gameExeFileTextBox.Text = this.SettingsService.DirtRally1Exe;
                this.gameExeFileTextBox.ScrollToEnd();
                this.gameDatDirTextBox.Text = this.SettingsService.DirtRally1GameBinDir;
                this.gameDatDirTextBox.ScrollToEnd();
                this.gameDatFilesListBox.Items.Clear();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameDatDirtRally2)
            {
                // Open a game*.dat file (DiRT Rally 2)
                this.tablessControl1.SelectedTab = this.gameDatTabPage;
                this.gameExeFileTextBox.Text = this.SettingsService.DirtRally2Exe;
                this.gameExeFileTextBox.ScrollToEnd();
                this.gameDatDirTextBox.Text = this.SettingsService.DirtRally2GameDatDir;
                this.gameDatDirTextBox.ScrollToEnd();
                this.gameDatFilesListBox.Items.Clear();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameDatDirt4)
            {
                // Open a game*.dat file (DiRT 4)
                this.tablessControl1.SelectedTab = this.gameDatTabPage;
                this.gameExeFileTextBox.Text = this.SettingsService.Dirt4Exe;
                this.gameExeFileTextBox.ScrollToEnd();
                this.gameDatDirTextBox.Text = this.SettingsService.Dirt4GameDatDir;
                this.gameDatDirTextBox.ScrollToEnd();
                this.gameDatFilesListBox.Items.Clear();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameDatCustom)
            {
                // Search an executable for game.bin/game.dat files
                this.tablessControl1.SelectedTab = this.gameDatTabPage;
                this.gameExeFileTextBox.Text = this.gameDatCustomExePath;
                this.gameExeFileTextBox.ScrollToEnd();
                this.gameDatDirTextBox.Text = this.gameDatCustomDirPath;
                this.gameDatDirTextBox.ScrollToEnd();
                this.gameDatFilesListBox.Items.Clear();
            }
        }

        private void NefsFileButton_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowOpenFileDialog("NeFS Archive (*.nefs)|*.nefs");
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
                source = this.ValidateNefs();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameDatDirtRally2)
            {
                source = this.ValidateGameDat();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameDatDirt4)
            {
                source = this.ValidateGameDat();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameBinDirtRally1)
            {
                source = this.ValidateGameDat();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameDatCustom)
            {
                source = this.ValidateGameDat();
            }
            else if (this.modeListBox.SelectedItem == this.openModeRecent)
            {
                source = this.ValidateRecent();
            }

            if (source != null)
            {
                this.DialogResult = DialogResult.OK;
                this.ArchiveSource = source;

                var recentFile = new RecentFile(source);
                this.SettingsService.RecentFiles.Insert(0, recentFile);
                this.Close();
            }
        }

        private void OpenFileForm_FormClosing(Object sender, FormClosingEventArgs e)
        {
            this.SaveSettings();
        }

        private void OpenFileForm_Load(Object sender, EventArgs e)
        {
            // Setup combo box
            this.modeListBox.Items.Add(this.openModeNefs);
            this.modeListBox.Items.Add(this.openModeRecent);
            this.modeListBox.Items.Add(this.openModeGameBinDirtRally1);
            this.modeListBox.Items.Add(this.openModeGameDatDirtRally2);
            this.modeListBox.Items.Add(this.openModeGameDatDirt4);
            this.modeListBox.Items.Add(this.openModeGameDatCustom);

            // Select default open mode
            this.modeListBox.SelectedItem = this.openModeNefs;

            // Load settings
            this.LoadSettings();
        }

        /// <summary>
        /// Saves the current open file dialog state and recently opened files.
        /// </summary>
        private void SaveSettings()
        {
            // Only keep 10 recent items
            if (this.SettingsService.RecentFiles.Count > 10)
            {
                this.SettingsService.RecentFiles.RemoveAt(this.SettingsService.RecentFiles.Count - 1);
            }

            this.SettingsService.OpenFileDialogState.LastMode = this.modeListBox.SelectedIndex;
            this.SettingsService.OpenFileDialogState.DataFilePath = this.dataFileTextBox.Text;
            this.SettingsService.OpenFileDialogState.HeaderOffset = this.headerOffsetTextBox.Text;
            this.SettingsService.OpenFileDialogState.HeaderPart6Offset = this.headerPart6OffsetTextBox.Text;
            this.SettingsService.OpenFileDialogState.HeaderPath = this.nefsFileTextBox.Text;
            this.SettingsService.OpenFileDialogState.IsAdvanced = this.advancedCheckBox.Checked;
            this.SettingsService.OpenFileDialogState.GameDatCustomDatDirPath = this.gameDatCustomDirPath;
            this.SettingsService.OpenFileDialogState.GameDatCustomExePath = this.gameDatCustomExePath;

            this.SettingsService.Save();
        }

        private NefsArchiveSource ValidateGameDat()
        {
            var selectedItem = this.gameDatFilesListBox.SelectedItem as GameDatFileItem;
            if (selectedItem == null)
            {
                return null;
            }

            return this.ValidateSource(selectedItem.Source) ? selectedItem.Source : null;
        }

        private NefsArchiveSource ValidateNefs()
        {
            var isAdvanced = this.advancedCheckBox.Checked;

            var offsetString = isAdvanced ? this.headerOffsetTextBox.Text : "0";
            var offsetNumStyle = NumberStyles.Integer;
            if (!ulong.TryParse(offsetString, offsetNumStyle, CultureInfo.InvariantCulture, out var offset))
            {
                this.UiService.ShowMessageBox($"Invalid header offset {offsetString}.");
                return null;
            }

            var offsetPart6String = isAdvanced ? this.headerPart6OffsetTextBox.Text : "0";
            if (!ulong.TryParse(offsetString, offsetNumStyle, CultureInfo.InvariantCulture, out var offsetPart6))
            {
                this.UiService.ShowMessageBox($"Invalid header part 6 offset {offsetPart6String}.");
                return null;
            }

            var headerFile = this.nefsFileTextBox.Text;
            var dataFile = isAdvanced ? this.dataFileTextBox.Text : headerFile;
            var source = new NefsArchiveSource(headerFile, offset, offsetPart6, dataFile);

            return this.ValidateSource(source) ? source : null;
        }

        private NefsArchiveSource ValidateRecent()
        {
            var recent = this.recentListBox.SelectedItem as RecentFile;
            if (recent == null)
            {
                return null;
            }

            var source = new NefsArchiveSource(recent.HeaderFilePath, recent.HeaderOffset, recent.HeaderPart6Offset, recent.DataFilePath);
            return this.ValidateSource(source) ? source : null;
        }

        private bool ValidateSource(NefsArchiveSource source)
        {
            if (!this.FileSystem.File.Exists(source.HeaderFilePath))
            {
                this.UiService.ShowMessageBox($"Cannot find file: {source.HeaderFilePath}.");
                return false;
            }

            if (!this.FileSystem.File.Exists(source.DataFilePath))
            {
                this.UiService.ShowMessageBox($"Cannot find file: {source.DataFilePath}.");
                return false;
            }

            return true;
        }

        private class GameDatFileItem
        {
            public GameDatFileItem(NefsArchiveSource source)
            {
                this.Source = source ?? throw new ArgumentNullException(nameof(source));
            }

            public NefsArchiveSource Source { get; }

            public override String ToString()
            {
                var fileName = Path.GetFileName(this.Source.DataFilePath);
                return $"{fileName} (header offset: {this.Source.HeaderOffset})";
            }
        }

        private class OpenMode
        {
            public OpenMode(string mode)
            {
                this.Mode = mode;
            }

            public string Mode { get; set; }

            public override String ToString()
            {
                return this.Mode;
            }
        }
    }
}
