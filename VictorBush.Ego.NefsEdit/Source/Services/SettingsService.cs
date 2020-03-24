// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Services
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using log4net;
    using VictorBush.Ego.NefsEdit.Utility;

    /// <summary>
    /// Settings service implementation.
    /// </summary>
    internal class SettingsService : ISettingsService
    {
        private static readonly ILog Log = LogHelper.GetLogger();

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
        }

        /// <inheritdoc/>
        public string QuickExtractDir => this.Settings?.QuickExtractDir ?? "";

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

            Log.Info("----------------------------");
            Log.Info($"Loading settings...");

            // Settings file exists, load it
            try
            {
                // Deserialize XML settings file
                using (var reader = this.FileSystem.File.OpenRead(SettingsFilePath))
                {
                    var xs = new XmlSerializer(typeof(Settings));
                    this.Settings = xs.Deserialize(reader) as Settings;
                }

                Log.Info($"Settings loaded.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to read settings file.\r\n{ex.Message}");
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

            Log.Info("----------------------------");
            Log.Info($"Saving settings...");

            try
            {
                // Serialize to XML file
                using (var writer = this.FileSystem.File.OpenWrite(SettingsFilePath))
                {
                    var xs = new XmlSerializer(typeof(Settings));
                    xs.Serialize(writer, this.Settings);
                }

                Log.Info($"Settings saved.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save settings file.\r\n{ex.Message}");
            }
        }

        /// <summary>
        /// Resets application settings to default.
        /// </summary>
        private void ResetSettings()
        {
            this.Settings = new Settings();
            this.Save();
        }
    }
}
