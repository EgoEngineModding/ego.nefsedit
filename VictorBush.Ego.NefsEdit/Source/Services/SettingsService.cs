// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Win32;
    using VictorBush.Ego.NefsEdit.Settings;
    using VictorBush.Ego.NefsEdit.Utility;
    using VictorBush.Ego.NefsLib;

    /// <summary>
    /// Settings service implementation.
    /// </summary>
    internal class SettingsService : ISettingsService
    {
        private static readonly ILogger Log = LogHelper.GetLogger();

        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="uiService">The UI service.</param>
        public SettingsService(IFileSystem fileSystem, IUiService uiService)
        {
            this.FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            this.Settings = new Settings();
        }

        /// <inheritdoc/>
        public String Dirt4Dir
        {
            get => this.Settings.Dirt4Dir;
            set => this.Settings.Dirt4Dir = value;
        }

        /// <inheritdoc/>
        public String Dirt4Exe => Path.Combine(this.Dirt4Dir, Constants.Dirt4ExeName);

        /// <inheritdoc/>
        public String Dirt4GameDatDir => Path.Combine(this.Dirt4Dir, Constants.Dirt4GameDatPath);

        /// <inheritdoc/>
        public String DirtRally1Dir
        {
            get => this.Settings.DirtRally1Dir;
            set => this.Settings.DirtRally1Dir = value;
        }

        /// <inheritdoc/>
        public String DirtRally1Exe => Path.Combine(this.DirtRally1Dir, Constants.DirtRally1ExeName);

        /// <inheritdoc/>
        public String DirtRally1GameBinDir => Path.Combine(this.DirtRally1Dir, Constants.DirtRally1GameBinPath);

        /// <inheritdoc/>
        public String DirtRally2Dir
        {
            get => this.Settings.DirtRally2Dir;
            set => this.Settings.DirtRally2Dir = value;
        }

        /// <inheritdoc/>
        public String DirtRally2Exe => Path.Combine(this.DirtRally2Dir, Constants.DirtRally2ExeName);

        /// <inheritdoc/>
        public String DirtRally2GameDatDir => Path.Combine(this.DirtRally2Dir, Constants.DirtRally2GameDatPath);

        /// <inheritdoc/>
        public OpenFileDialogState OpenFileDialogState
        {
            get => this.Settings.OpenFileDialogState;
            set => this.Settings.OpenFileDialogState = value;
        }

        /// <inheritdoc/>
        public string QuickExtractDir
        {
            get => this.Settings.QuickExtractDir ?? "";
            set => this.Settings.QuickExtractDir = value;
        }

        /// <inheritdoc/>
        public List<NefsArchiveSource> RecentFiles
        {
            get => this.Settings.RecentFiles;
            set => this.Settings.RecentFiles = value;
        }

        private IFileSystem FileSystem { get; }

        private Settings Settings { get; set; }

        private IUiService UiService { get; }

        /// <inheritdoc/>
        public bool ChooseQuickExtractDir()
        {
            var (result, dir) = this.UiService.ShowFolderBrowserDialog("Choose a quick extract directory.");
            if (result != DialogResult.OK)
            {
                return false;
            }

            if (this.Settings == null)
            {
                this.ResetSettings();
            }

            this.Settings.QuickExtractDir = dir;
            this.Save();
            return true;
        }

        /// <inheritdoc/>
        public void Load()
        {
            if (!this.FileSystem.File.Exists(SettingsFilePath))
            {
                // Settings file doesn't exist, create it
                this.ResetSettings();
                return;
            }

            Log.LogInformation("----------------------------");
            Log.LogInformation($"Loading settings...");

            // Settings file exists, load it
            try
            {
                // Deserialize XML settings file
                using (var reader = this.FileSystem.File.OpenRead(SettingsFilePath))
                {
                    var xs = new XmlSerializer(typeof(Settings));
                    this.Settings = xs.Deserialize(reader) as Settings;
                }

                Log.LogInformation($"Settings loaded.");
                this.OnSettingsLoaded();
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to read settings file.\r\n{ex.Message}");
                this.ResetSettings();
            }
        }

        /// <inheritdoc/>
        public void Save()
        {
            // Use defaults if no settings loaded
            if (this.Settings == null)
            {
                this.ResetSettings();
            }

            Log.LogInformation("----------------------------");
            Log.LogInformation($"Saving settings...");

            try
            {
                // Serialize to XML file
                using (var writer = this.FileSystem.File.OpenWrite(SettingsFilePath))
                {
                    var xs = new XmlSerializer(typeof(Settings));
                    xs.Serialize(writer, this.Settings);
                }

                Log.LogInformation($"Settings saved.");
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to save settings file.\r\n{ex.Message}");
            }
        }

        private string FindSteamGameDir(string gameSubPath)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key?.GetValue("SteamPath") is string steamPath)
                    {
                        var path = Path.Combine(steamPath, gameSubPath);
                        if (this.FileSystem.Directory.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return "";
        }

        /// <summary>
        /// Performs checks when settings are loaded.
        /// </summary>
        private void OnSettingsLoaded()
        {
            var shouldSave = false;

            // Try to auto-find game directories if not set
            if (string.IsNullOrWhiteSpace(this.DirtRally2Dir))
            {
                this.DirtRally2Dir = this.FindSteamGameDir(Constants.DirtRally2SteamPath);
                if (!string.IsNullOrWhiteSpace(this.DirtRally2Dir))
                {
                    Log.LogInformation($"Found DiRT Rally 2 directory: {this.DirtRally2Dir}");
                    shouldSave = true;
                }
            }

            if (string.IsNullOrWhiteSpace(this.Dirt4Dir))
            {
                this.Dirt4Dir = this.FindSteamGameDir(Constants.Dirt4SteamPath);
                if (!string.IsNullOrWhiteSpace(this.Dirt4Dir))
                {
                    Log.LogInformation($"Found DiRT 4 directory: {this.Dirt4Dir}");
                    shouldSave = true;
                }
            }

            // Save updated settings if needed
            if (shouldSave)
            {
                this.Save();
            }
        }

        /// <summary>
        /// Resets application settings to default.
        /// </summary>
        private void ResetSettings()
        {
            this.Settings = new Settings
            {
                DirtRally1Dir = this.FindSteamGameDir(Constants.DirtRally1SteamPath),
                DirtRally2Dir = this.FindSteamGameDir(Constants.DirtRally2SteamPath),
                Dirt4Dir = this.FindSteamGameDir(Constants.Dirt4SteamPath),
            };

            this.Save();
        }
    }
}
