using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VictorBush.Ego.NefsEdit.Utility
{
    class Settings
    {
        public static string QuickExtractDir = "";

        private static string _settingsFileName = Path.Combine(Application.StartupPath, "settings.xml");

        /// <summary>
        /// Prompts user to select a directory to use for quick extraction.
        /// </summary>
        /// <returns>True if user chose succesfully, False otherwise.</returns>
        public static bool ChooseQuickExtractDir()
        {
            var ofd = new FolderBrowserDialog();
            ofd.Description = "Pick a directory to use for quick extraction.";
            ofd.ShowNewFolderButton = true;
            ofd.SelectedPath = QuickExtractDir;

            var result = ofd.ShowDialog();

            if (result == DialogResult.OK)
            {
                QuickExtractDir = ofd.SelectedPath;
                SaveSettings();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Loads settings from the settings file.
        /// </summary>
        public static void LoadSettings()
        {
            var settingsFileNeedsCleaned = false;

            if (!File.Exists(_settingsFileName))
            {
                /* Settings file doesn't exist, create it */
                ResetSettings();
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(_settingsFileName);

            try
            {
                QuickExtractDir = xmlDoc.SelectSingleNode("//settings/quickExtractDir").InnerText;
            }
            catch (Exception)
            {
                settingsFileNeedsCleaned = true;
            }

            /* Re-write the settings file if detected missing entries */
            if (settingsFileNeedsCleaned)
            {
                SaveSettings();
            }
        }

        /// <summary>
        /// Saves the current settings to the settings file.
        /// </summary>
        public static void SaveSettings()
        {
            var xmlDoc = new XmlDocument();
            var settingsElement = xmlDoc.CreateElement("settings");

            /* Extraction Directory */
            var extractionDirectoryNode = xmlDoc.CreateElement("quickExtractDir");
            extractionDirectoryNode.InnerText = QuickExtractDir;
            settingsElement.AppendChild(extractionDirectoryNode);

            /* Save */
            xmlDoc.AppendChild(settingsElement);
            xmlDoc.Save(_settingsFileName);
        }

        /// <summary>
        /// Resets application settings to default.
        /// </summary>
        public static void ResetSettings()
        {
            QuickExtractDir = "";
            SaveSettings();
        }
    }
}
