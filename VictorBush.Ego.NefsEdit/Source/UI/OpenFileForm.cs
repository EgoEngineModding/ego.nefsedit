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
    using VictorBush.Ego.NefsLib.ArchiveSource;
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

        /// <summary>
        /// Looks through the game executable to find header offsets for game.dat files.
        /// </summary>
        /// <returns>A list of game.dat archive sources.</returns>
        private async Task<List<GameDatSource>> FindGameDatHeaderOffsetsAsync(
            string gameDatDir,
            string gameExePath,
            NefsProgress p)
        {
            // TODO : FIX ME
            this.UiService.ShowMessageBox("Disabled.");
            return new List<GameDatSource>();

            if (!this.FileSystem.File.Exists(gameExePath))
            {
                this.UiService.ShowMessageBox($"Cannot find executable file: {gameExePath}.");
                return new List<GameDatSource>();
            }

            // Search for headers in the exe
            using (var t = p.BeginTask(1.0f, "Searching for headers"))
            {
                // TODO : FIX ME
                // return await this.Reader.FindHeadersAsync(gameExePath, gameDatDir, p);
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
            this.nefsFileTextBox.Text = this.SettingsService.OpenFileDialogState.NefsFilePath;
            this.splitDataFileTextBox.Text = this.SettingsService.OpenFileDialogState.GameDatDataFilePath;
            this.splitHeaderFileTextBox.Text = this.SettingsService.OpenFileDialogState.GameDatHeaderFilePath;
            this.splitPrimaryOffsetTextBox.Text = this.SettingsService.OpenFileDialogState.GameDatPrimaryOffset;
            this.splitPrimarySizeTextBox.Text = this.SettingsService.OpenFileDialogState.GameDatPrimarySize;
            this.splitSecondaryOffsetTextBox.Text = this.SettingsService.OpenFileDialogState.GameDatSecondaryOffset;
            this.splitSecondarySizeTextBox.Text = this.SettingsService.OpenFileDialogState.GameDatSecondarySize;
            this.nefsInjectDataFileTextBox.Text = this.SettingsService.OpenFileDialogState.NefsInjectDataFilePath;
            this.nefsInjectFileTextBox.Text = this.SettingsService.OpenFileDialogState.NefsInjectFilePath;
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
                source = this.ValidateStandardSource();
            }
            else if (this.modeListBox.SelectedItem == this.openModeNefsInject)
            {
                source = this.ValidateNefsInjectSource();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameDatDirtRally2)
            {
                source = this.ValidateGameDatSearchSource();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameDatDirt4)
            {
                source = this.ValidateGameDatSearchSource();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameBinDirtRally1)
            {
                source = this.ValidateGameDatSearchSource();
            }
            else if (this.modeListBox.SelectedItem == this.openModeGameDatCustom)
            {
                source = this.ValidateGameDatCustomSource();
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
            this.modeListBox.Items.Add(this.openModeNefsInject);
            this.modeListBox.Items.Add(this.openModeGameDatCustom);
            this.modeListBox.Items.Add(this.openModeRecent);
            this.modeListBox.Items.Add(this.openModeGameBinDirtRally1);
            this.modeListBox.Items.Add(this.openModeGameDatDirtRally2);
            this.modeListBox.Items.Add(this.openModeGameDatDirt4);

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
            this.SettingsService.OpenFileDialogState.NefsFilePath = this.nefsFileTextBox.Text;
            this.SettingsService.OpenFileDialogState.GameDatDataFilePath = this.splitDataFileTextBox.Text;
            this.SettingsService.OpenFileDialogState.GameDatHeaderFilePath = this.splitHeaderFileTextBox.Text;
            this.SettingsService.OpenFileDialogState.GameDatPrimaryOffset = this.splitPrimaryOffsetTextBox.Text;
            this.SettingsService.OpenFileDialogState.GameDatPrimarySize = this.splitPrimarySizeTextBox.Text;
            this.SettingsService.OpenFileDialogState.GameDatSecondaryOffset = this.splitSecondaryOffsetTextBox.Text;
            this.SettingsService.OpenFileDialogState.GameDatSecondarySize = this.splitSecondarySizeTextBox.Text;
            this.SettingsService.OpenFileDialogState.NefsInjectDataFilePath = this.nefsInjectDataFileTextBox.Text;
            this.SettingsService.OpenFileDialogState.NefsInjectFilePath = this.nefsInjectFileTextBox.Text;
            //this.SettingsService.OpenFileDialogState.GameDatCustomDatDirPath = this.gameDatCustomDirPath;
            //this.SettingsService.OpenFileDialogState.GameDatCustomExePath = this.gameDatCustomExePath;

            this.SettingsService.Save();
        }

        private NefsArchiveSource ValidateNefsInjectSource()
        {
            var dataFilePath = this.nefsInjectDataFileTextBox.Text;
            var headerFilePath = this.nefsInjectFileTextBox.Text;
            var source = NefsArchiveSource.NefsInject(dataFilePath, headerFilePath);

            if (!this.ValidateFileExists(source.DataFilePath))
                return null;

            if (!this.ValidateFileExists(source.NefsInjectFilePath))
                return null;

            return source;
        }

        private NefsArchiveSource ValidateGameDatSearchSource()
        {
            var selectedItem = this.gameDatFilesListBox.SelectedItem as GameDatFileItem;
            if (selectedItem == null)
                return null;

            var source = selectedItem.Source;
            if (!this.ValidateFileExists(source.DataFilePath))
                return null;

            if (!this.ValidateFileExists(source.HeaderFilePath))
                return null;

            return source;
        }


        private NefsArchiveSource ValidateStandardSource()
        {
            var headerFile = this.nefsFileTextBox.Text;
            var source = NefsArchiveSource.Standard(headerFile);

            if (!this.FileSystem.File.Exists(source.FilePath))
            {
                this.UiService.ShowMessageBox($"Cannot find file: {source.FilePath}.");
                return null;
            }

            return source;
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
                this.UiService.ShowMessageBox($"Invalid primary offset {primaryOffsetString}.");
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
                this.UiService.ShowMessageBox($"Invalid secondary offset {secondaryOffsetString}.");
                return null;
            }

            // Primary size
            int? primarySize = null;
            var primarySizeString = this.splitPrimarySizeTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(primarySizeString))
            {
                if (!ParseHexNumberStringToInt(primarySizeString, out var size))
                {
                    this.UiService.ShowMessageBox($"Invalid primary size {primarySizeString}.");
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
                    this.UiService.ShowMessageBox($"Invalid secondary size {secondarySizeString}.");
                    return null;
                }

                secondarySize = size;
            }

            var headerFile = this.splitHeaderFileTextBox.Text.Trim();
            var dataFile = this.splitDataFileTextBox.Text.Trim();
            var source = NefsArchiveSource.GameDat(dataFile, headerFile, primaryOffset, primarySize, secondaryOffset, secondarySize);

            if (!this.ValidateFileExists(source.DataFilePath))
            {
                return null;
            }

            if (!this.ValidateFileExists(source.HeaderFilePath))
            {
                return null;
            }

            return source;
        }

        private static bool ParseHexNumberStringToLong(string input, out long result)
        {
            var str = input.Replace("0x", "").Replace("&h", "").Trim();
            return long.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }
        private static bool ParseHexNumberStringToInt(string input, out int result)
        {
            var str = input.Replace("0x", "").Replace("&h", "").Trim();
            return int.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
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
                    if (!this.ValidateFileExists(standardSource.FilePath))
                        return null;
                    return standardSource;

                case GameDatSource gameDatSource:
                    if (!this.ValidateFileExists(gameDatSource.DataFilePath))
                        return null;
                    if (!this.ValidateFileExists(gameDatSource.HeaderFilePath))
                        return null;
                    return gameDatSource;

                case NefsInjectSource nefsInjectSource:
                    if (!this.ValidateFileExists(nefsInjectSource.NefsInjectFilePath))
                        return null;
                    if (!this.ValidateFileExists(nefsInjectSource.DataFilePath))
                        return null;
                    return nefsInjectSource;

                default:
                    this.UiService.ShowMessageBox($"Unknown archive source type.");
                    return null;
            }
        }

        private bool ValidateFileExists(string file)
        {
            if (!this.FileSystem.File.Exists(file))
            {
                this.UiService.ShowMessageBox($"Cannot find file: {file}.");
                return false;
            }

            return true;
        }

        //private bool ValidateSource(NefsArchiveSource source)
        //{
        //    if (!this.FileSystem.File.Exists(source.HeaderFilePath))
        //    {
        //        this.UiService.ShowMessageBox($"Cannot find file: {source.HeaderFilePath}.");
        //        return false;
        //    }

        //    if (!this.FileSystem.File.Exists(source.FilePath))
        //    {
        //        this.UiService.ShowMessageBox($"Cannot find file: {source.FilePath}.");
        //        return false;
        //    }

        //    return true;
        //}

        private class GameDatFileItem
        {
            public GameDatFileItem(GameDatSource source)
            {
                this.Source = source ?? throw new ArgumentNullException(nameof(source));
            }

            public GameDatSource Source { get; }

            public override String ToString()
            {
                return $"{this.Source.FileName} (header offset: {this.Source.PrimaryOffset})";
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

        private void splitDataFileBrowseButton_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowOpenFileDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            this.splitDataFileTextBox.Text = path;
            this.splitDataFileTextBox.ScrollToEnd();
        }

        private void splitHeaderFileBrowseButton_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowOpenFileDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            this.splitHeaderFileTextBox.Text = path;
            this.splitHeaderFileTextBox.ScrollToEnd();
        }
    }
}
