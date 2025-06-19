// See LICENSE.txt for license information.

using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.Utility;

namespace VictorBush.Ego.NefsEdit.UI;

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
		InitializeComponent();
		SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
		UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
	}

	private ISettingsService SettingsService { get; }

	private IUiService UiService { get; }

	private void BrowseDirtRally2Button_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowFolderBrowserDialog("Choose the DiRT Rally 2 directory.");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.dirtRally2TextBox.Text = path;
	}

	private void CancelButton_Click(Object sender, EventArgs e)
	{
		Close();
	}

	private void Dirt4Button_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowFolderBrowserDialog("Choose the DiRT 4 directory.");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.dirt4TextBox.Text = path;
	}

	private void DirtRallyButton_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowFolderBrowserDialog("Choose the DiRT Rally directory.");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.dirtRallyTextBox.Text = path;
	}

	private void QuickExtractButton_Click(Object sender, EventArgs e)
	{
		(var result, var path) = UiService.ShowFolderBrowserDialog("Choose a quick extract directory.");
		if (result != DialogResult.OK)
		{
			return;
		}

		this.quickExtractTextBox.Text = path;
	}

	private void SaveButton_Click(Object sender, EventArgs e)
	{
		SettingsService.QuickExtractDir = this.quickExtractTextBox.Text;
		SettingsService.DirtRally1Dir = this.dirtRallyTextBox.Text;
		SettingsService.DirtRally2Dir = this.dirtRally2TextBox.Text;
		SettingsService.Dirt4Dir = this.dirt4TextBox.Text;
		SettingsService.Save();
		Close();
	}

	private void SettingsForm_Load(Object sender, EventArgs e)
	{
		this.quickExtractTextBox.Text = SettingsService.QuickExtractDir;
		this.quickExtractTextBox.ScrollToEnd();

		this.dirtRallyTextBox.Text = SettingsService.DirtRally1Dir;
		this.dirtRallyTextBox.ScrollToEnd();

		this.dirtRally2TextBox.Text = SettingsService.DirtRally2Dir;
		this.dirtRally2TextBox.ScrollToEnd();

		this.dirt4TextBox.Text = SettingsService.Dirt4Dir;
		this.dirt4TextBox.ScrollToEnd();
	}
}
