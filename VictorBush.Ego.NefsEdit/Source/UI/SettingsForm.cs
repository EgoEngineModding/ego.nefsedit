// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using System.Windows.Forms;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Utility;

    /// <summary>
    /// Settings dialog box.
    /// </summary>
    internal partial class SettingsForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsForm"/> class.
        /// </summary>
        /// <param name="settingsService">The settings service to use.</param>
        /// <param name="uiService">The UI service to use.</param>
        public SettingsForm(ISettingsService settingsService, IUiService uiService)
        {
            this.InitializeComponent();
            this.SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
        }

        private ISettingsService SettingsService { get; }

        private IUiService UiService { get; }

        private void BrowseDirtRally2Button_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowFolderBrowserDialog("Choose the DiRT Rally 2 directory.");
            if (result != DialogResult.OK)
            {
                return;
            }

            this.dirtRally2TextBox.Text = path;
        }

        private void CancelButton_Click(Object sender, EventArgs e)
        {
            this.Close();
        }

        private void Dirt4Button_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowFolderBrowserDialog("Choose the DiRT 4 directory.");
            if (result != DialogResult.OK)
            {
                return;
            }

            this.dirt4TextBox.Text = path;
        }

        private void DirtRallyButton_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowFolderBrowserDialog("Choose the DiRT Rally directory.");
            if (result != DialogResult.OK)
            {
                return;
            }

            this.dirtRallyTextBox.Text = path;
        }

        private void QuickExtractButton_Click(Object sender, EventArgs e)
        {
            (var result, var path) = this.UiService.ShowFolderBrowserDialog("Choose a quick extract directory.");
            if (result != DialogResult.OK)
            {
                return;
            }

            this.quickExtractTextBox.Text = path;
        }

        private void SaveButton_Click(Object sender, EventArgs e)
        {
            this.SettingsService.QuickExtractDir = this.quickExtractTextBox.Text;
            this.SettingsService.DirtRally1Dir = this.dirtRallyTextBox.Text;
            this.SettingsService.DirtRally2Dir = this.dirtRally2TextBox.Text;
            this.SettingsService.Dirt4Dir = this.dirt4TextBox.Text;
            this.SettingsService.Save();
            this.Close();
        }

        private void SettingsForm_Load(Object sender, EventArgs e)
        {
            this.quickExtractTextBox.Text = this.SettingsService.QuickExtractDir;
            this.quickExtractTextBox.ScrollToEnd();

            this.dirtRallyTextBox.Text = this.SettingsService.DirtRally1Dir;
            this.dirtRallyTextBox.ScrollToEnd();

            this.dirtRally2TextBox.Text = this.SettingsService.DirtRally2Dir;
            this.dirtRally2TextBox.ScrollToEnd();

            this.dirt4TextBox.Text = this.SettingsService.Dirt4Dir;
            this.dirt4TextBox.ScrollToEnd();
        }
    }
}
