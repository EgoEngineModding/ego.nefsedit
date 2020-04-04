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

        private readonly OpenMode openModeCustom = new OpenMode("Custom");

        private readonly OpenMode openModeGameDatDirt4 = new OpenMode("game*.dat (DiRT 4)");

        private readonly OpenMode openModeGameDatDirtRally2 = new OpenMode("game*.dat (DiRT Rally 2)");

        private readonly OpenMode openModeNefs = new OpenMode("NeFS");

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

        private void CustomDataFileButton_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowOpenFileDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            this.customDataFileTextBox.Text = path;
            this.customDataFileTextBox.ScrollToEnd();
        }

        private void CustomHeaderFileButton_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowOpenFileDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            this.customHeaderFileTextBox.Text = path;
            this.customHeaderFileTextBox.ScrollToEnd();
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

            var headerOffsets = new Dictionary<string, ulong>();
            var gameDatFiles = new List<string>();

            // Read whole game exe into memory
            byte[] gameExeBuffer;
            using (var t = p.BeginTask(0.20f, "Reading game executable"))
            using (var reader = this.FileSystem.File.OpenRead(gameExePath))
            {
                gameExeBuffer = new byte[reader.Length];
                await reader.ReadAsync(gameExeBuffer, 0, (int)reader.Length, p.CancellationToken);
            }

            // Search for headers in the exe
            using (var t = p.BeginTask(0.50f, "Searching for headers"))
            {
                var searchOffset = 0UL;

                (string DataFileName, ulong Offset)? header;
                while ((header = await this.Reader.FindHeaderAsync(gameExeBuffer, searchOffset, p)) != null)
                {
                    headerOffsets.Add(header.Value.DataFileName, header.Value.Offset);
                    searchOffset = header.Value.Offset + 4;
                }
            }

            // Try to match offsets to game.dat files
            using (var t = p.BeginTask(0.30f, "Searching for game.dat files"))
            {
                foreach (var file in this.FileSystem.Directory.EnumerateFiles(gameDatDir))
                {
                    var fileName = Path.GetFileName(file);
                    if (headerOffsets.ContainsKey(fileName))
                    {
                        gameDatFiles.Add(file);
                    }
                }
            }

            // Match offsets and files
            if (gameDatFiles.Count != headerOffsets.Count)
            {
                Log.LogError($"Found {gameDatFiles.Count} game*.dat files, but found {headerOffsets.Count} headers in game exectuable.");
            }

            // Build data sources for the game.dat files
            var sources = new List<NefsArchiveSource>();
            for (var i = 0; i < gameDatFiles.Count; ++i)
            {
                var fileName = Path.GetFileName(gameDatFiles[i]);
                var isDataEncrypted = true;
                var source = new NefsArchiveSource(gameExePath, headerOffsets[fileName], gameDatFiles[i], isDataEncrypted);
                sources.Add(source);
            }

            return sources;
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

            if (this.typeComboBox.SelectedItem == this.openModeNefs)
            {
                source = this.ValidateNefs();
            }
            else if (this.typeComboBox.SelectedItem == this.openModeGameDatDirtRally2)
            {
                source = this.ValidateGameDat();
            }
            else if (this.typeComboBox.SelectedItem == this.openModeGameDatDirt4)
            {
                source = this.ValidateGameDat();
            }
            else if (this.typeComboBox.SelectedItem == this.openModeCustom)
            {
                source = this.ValidateCustom();
            }

            if (source != null)
            {
                this.DialogResult = DialogResult.OK;
                this.ArchiveSource = source;
                this.Close();
            }
        }

        private void OpenFileForm_Load(Object sender, EventArgs e)
        {
            // Setup combo box
            this.typeComboBox.Items.Add(this.openModeNefs);
            this.typeComboBox.Items.Add(this.openModeGameDatDirtRally2);
            this.typeComboBox.Items.Add(this.openModeGameDatDirt4);
            this.typeComboBox.Items.Add(this.openModeCustom);

            // Select default open mode
            this.typeComboBox.SelectedItem = this.openModeNefs;
        }

        private void TypeComboBox_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (this.typeComboBox.SelectedItem == this.openModeNefs)
            {
                // Open NeFS archive
                this.tablessControl1.SelectedTab = this.nefsTabPage;
            }
            else if (this.typeComboBox.SelectedItem == this.openModeGameDatDirtRally2)
            {
                // Open a game*.dat file (DiRT Rally 2)
                this.tablessControl1.SelectedTab = this.gameDatTabPage;
                this.gameExeFileTextBox.Text = this.SettingsService.DirtRally2Exe;
                this.gameExeFileTextBox.ScrollToEnd();
                this.gameDatDirTextBox.Text = this.SettingsService.DirtRally2GameDatDir;
                this.gameDatDirTextBox.ScrollToEnd();
                this.gameDatFilesListBox.Items.Clear();
            }
            else if (this.typeComboBox.SelectedItem == this.openModeGameDatDirt4)
            {
                // Open a game*.dat file (DiRT 4)
                this.tablessControl1.SelectedTab = this.gameDatTabPage;
                this.gameExeFileTextBox.Text = this.SettingsService.Dirt4Exe;
                this.gameExeFileTextBox.ScrollToEnd();
                this.gameDatDirTextBox.Text = this.SettingsService.Dirt4GameDatDir;
                this.gameDatDirTextBox.ScrollToEnd();
                this.gameDatFilesListBox.Items.Clear();
            }
            else if (this.typeComboBox.SelectedItem == this.openModeCustom)
            {
                // Custom
                this.tablessControl1.SelectedTab = this.customTabPage;
            }
        }

        private NefsArchiveSource ValidateCustom()
        {
            if (!this.FileSystem.File.Exists(this.customHeaderFileTextBox.Text))
            {
                this.UiService.ShowMessageBox($"Cannot find header file: {this.customHeaderFileTextBox.Text}.");
                return null;
            }

            if (!this.FileSystem.File.Exists(this.customDataFileTextBox.Text))
            {
                this.UiService.ShowMessageBox($"Cannot find data file: {this.customDataFileTextBox.Text}.");
                return null;
            }

            var offsetString = this.customHeaderOffsetTextBox.Text;
            var offsetNumStyle = NumberStyles.Integer;
            if (!ulong.TryParse(offsetString, offsetNumStyle, CultureInfo.InvariantCulture, out var offset))
            {
                this.UiService.ShowMessageBox($"Invalid header offset {this.customHeaderOffsetTextBox.Text}.");
                return null;
            }

            return new NefsArchiveSource(
                this.customHeaderFileTextBox.Text,
                offset,
                this.customDataFileTextBox.Text,
                this.customDataIsEncryptedCheckBox.Checked);
        }

        private NefsArchiveSource ValidateGameDat()
        {
            var selectedItem = this.gameDatFilesListBox.SelectedItem as GameDatFileItem;
            if (selectedItem == null || !this.FileSystem.File.Exists(selectedItem.Source.DataFilePath))
            {
                this.UiService.ShowMessageBox($"Cannot find game*.dat file: {selectedItem?.Source.DataFilePath}.");
                return null;
            }

            if (!this.FileSystem.File.Exists(this.gameExeFileTextBox.Text))
            {
                this.UiService.ShowMessageBox($"Cannot find game executable: {this.gameExeFileTextBox.Text}.");
                return null;
            }

            return selectedItem.Source;
        }

        private NefsArchiveSource ValidateNefs()
        {
            if (!this.FileSystem.File.Exists(this.nefsFileTextBox.Text))
            {
                this.UiService.ShowMessageBox($"Cannot find NeFS file: {this.nefsFileTextBox.Text}.");
                return null;
            }

            return new NefsArchiveSource(this.nefsFileTextBox.Text);
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
