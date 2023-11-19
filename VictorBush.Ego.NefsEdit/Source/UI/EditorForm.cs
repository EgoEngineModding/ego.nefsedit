// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsCommon.InjectionDatabase;
using VictorBush.Ego.NefsEdit.Commands;
using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.Workspace;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsEdit.UI;

/// <summary>
/// Main application form.
/// </summary>
internal partial class EditorForm : Form
{
	private readonly ILogger<EditorForm> logger;
	private readonly IInjectionDatabaseService injectionDatabaseService;
	private readonly IProgressService progressService;
	private ArchiveDebugForm archiveDebugForm;
	private BrowseAllForm browseAllForm;
	private BrowseTreeForm browseTreeForm;
	private ConsoleForm consoleForm;
	private ItemDebugForm itemDebugForm;
	private PropertyGridForm selectedFilePropertyForm;

	/// <summary>
	/// Initializes a new instance of the <see cref="EditorForm"/> class.
	/// </summary>
	public EditorForm(
		ILogger<EditorForm> logger,
		INefsEditWorkspace workspace,
		IUiService uiService,
		ISettingsService settingsService,
		IInjectionDatabaseService injectionDatabaseService,
		IProgressService progressService)
	{
		InitializeComponent();
		UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
		SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
		this.injectionDatabaseService = injectionDatabaseService ?? throw new ArgumentNullException(nameof(injectionDatabaseService));
		this.progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
		Workspace.ArchiveOpened += OnWorkspaceArchiveOpened;
		Workspace.ArchiveClosed += OnWorkspaceArchiveClosed;
		Workspace.ArchiveSaved += OnWorkspaceArchiveSaved;
		Workspace.CommandExecuted += OnWorkspaceCommandExecuted;
		Workspace.SelectedItemsChanged += OnWorkspaceSelectedItemsChanged;
	}

	private ISettingsService SettingsService { get; }

	private IUiService UiService { get; }

	/// <summary>
	/// Gets the workspace.
	/// </summary>
	private INefsEditWorkspace Workspace { get; }

	/// <summary>
	/// Opens the item context menu if there is an item selected.
	/// </summary>
	/// <param name="position">Where to open the menu at.</param>
	public void ShowItemContextMenu(Point position)
	{
		if (Workspace.SelectedItems.Count == 0)
		{
			return;
		}

		/* Show the context menu */
		this.itemContextMenuStrip.Show(position);
	}

	private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
	{
		UiService.ShowMessageBox($"Version {Application.ProductVersion}", title: "About");
	}

	private void ArchiveDebugMainMenuItem_Click(Object sender, EventArgs e)
	{
		this.archiveDebugForm.Show();
		this.archiveDebugForm.Focus();
	}

	private async void CloseMainMenuItem_Click(Object sender, EventArgs e)
	{
		await Workspace.CloseArchiveAsync();
	}

	private void ConsoleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.consoleForm.Show();
		this.consoleForm.Focus();
	}

	private void DebugViewToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.browseAllForm.Show();
		this.browseAllForm.Focus();
	}

	private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		Quit(e);
	}

	private void EditorForm_Load(object sender, EventArgs e)
	{
		// Set the dockpanel theme
		var theme = new WeifenLuo.WinFormsUI.Docking.VS2015LightTheme();
		this.browserDockPanel.Theme = theme;

		// Create the different forms for the editor
		this.browseAllForm = new BrowseAllForm(Workspace, this, UiService);
		this.browseTreeForm = new BrowseTreeForm(Workspace, this, UiService);
		this.selectedFilePropertyForm = new PropertyGridForm();
		this.consoleForm = new ConsoleForm();
		this.archiveDebugForm = new ArchiveDebugForm(Workspace, UiService);
		this.itemDebugForm = new ItemDebugForm(Workspace, UiService);

		// Redirect standard output to our console form
		this.consoleForm.SetupConsole();

		// Reset the form layout to the default layout
		ResetToDefaultLayout();

		// Setup menu item initial state
		UpdateMenuItems();

		// Load settings
		SettingsService.Load();

		// Check for database update
		if (SettingsService.CheckForDatabaseUpdatesOnStartup)
		{
			this.logger.LogInformation("Checking for injection database update.");

			UiService.Dispatcher.InvokeAsync(async () =>
			{
				try
				{
					var update = await this.injectionDatabaseService.CheckForDatabaseUpdateAsync();
					if (update is null)
					{
						this.logger.LogInformation("No injection database update found.");
						return;
					}

					var result = UiService.ShowMessageBox("A new injection database is available. Download now?", "Database Update Available", MessageBoxButtons.YesNo);
					if (result != DialogResult.Yes)
					{
						return;
					}

					await this.progressService.RunModalTaskAsync(progress => this.injectionDatabaseService.UpdateDatabaseAsync(update));
					this.logger.LogInformation($"Injection database updates to version {update.DbVersion}.");
				}
				catch (Exception ex)
				{
					this.logger.LogError(ex, "Failed to check for injection database update.");
				}
			});
		}
	}

	private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
	{
		// Quit the application
		Application.Exit();
	}

	private void extractRawToToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private async void ExtractToContextMenuItem_Click(object sender, EventArgs e)
	{
		await Workspace.ExtractItemsByDialogAsync(Workspace.SelectedItems);
	}

	private async void ExtractToToolStripMenuItem_Click(object sender, EventArgs e)
	{
		await Workspace.ExtractItemsByDialogAsync(Workspace.SelectedItems);
	}

	private void ItemDebugViewMainMenuItem_Click(object sender, EventArgs e)
	{
		this.itemDebugForm.Show();
		this.itemDebugForm.Focus();
	}

	private void ItemDetailsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.selectedFilePropertyForm.Show();
		this.selectedFilePropertyForm.Focus();
	}

	private void OnWorkspaceArchiveClosed(object? sender, EventArgs e)
	{
		UiService.Dispatcher.Invoke(() =>
		{
			UpdateTitle();
			UpdateMenuItems();
		});
	}

	private void OnWorkspaceArchiveOpened(object? sender, EventArgs e)
	{
		// Update - must do on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			UpdateTitle();
			UpdateMenuItems();
		});
	}

	private void OnWorkspaceArchiveSaved(object? sender, EventArgs e)
	{
		UiService.Dispatcher.Invoke(() =>
		{
			UpdateTitle();
			UpdateMenuItems();
		});
	}

	private void OnWorkspaceCommandExecuted(object? sender, NefsEditCommandEventArgs e)
	{
		UpdateTitle();
		UpdateMenuItems();
		this.selectedFilePropertyForm.RefreshGrid();
	}

	private void OnWorkspaceSelectedItemsChanged(object? sender, EventArgs e)
	{
		var items = Workspace.SelectedItems;

		// Set which item shows up in the Item's property window
		if (items.Count == 1)
		{
			this.selectedFilePropertyForm.SetSelectedObject(items[0]);
		}
		else
		{
			this.selectedFilePropertyForm.SetSelectedObject(null);
		}

		// Set "Item" menu visibility
		UpdateMenuItems();

		if (items.Count == 0)
		{
			return;
		}

		// Set visibility of the Replace menu option
		if (items.Count > 1 || items[0].Type == NefsItemType.Directory)
		{
			// Can't replace directories or multiple files right now
			this.ReplaceContextMenuItem.Visible = false;
			this.replaceMainMenuItem.Enabled = false;
		}
		else
		{
			// Single file selected, can replace
			this.ReplaceContextMenuItem.Visible = true;
			this.replaceMainMenuItem.Enabled = true;
		}
	}

	private async void OpenToolStripMenuItem_Click(object sender, EventArgs e)
	{
		await Workspace.OpenArchiveByDialogAsync();
	}

	private void OptionsMainMenuItem_Click(object sender, EventArgs e)
	{
		UiService.ShowSettingsDialog(SettingsService);
	}

	private async void QuickExtractContextMenuItem_Click(object sender, EventArgs e)
	{
		await Workspace.ExtractItemsByQuickExtractAsync(Workspace.SelectedItems);
	}

	private async void QuickExtractToolStripMenuItem_Click(object sender, EventArgs e)
	{
		await Workspace.ExtractItemsByQuickExtractAsync(Workspace.SelectedItems);
	}

	private async void Quit(FormClosingEventArgs e)
	{
		if (Workspace.Archive is null || !Workspace.ArchiveIsModified)
		{
			return;
		}

		// Archive has been modified; prompt to save before exit
		var result = UiService.ShowMessageBox(
			$"Save archive {Workspace.ArchiveSource!.FilePath}?", "Save?", MessageBoxButtons.YesNoCancel);

		if (result == DialogResult.Yes)
		{
			// Cancel exiting the application - we need to wait for the save to finish
			if (e != null)
			{
				e.Cancel = true;
			}

			/* Trigger the save */
			var saved = await Workspace.SaveArchiveAsync();
			if (saved)
			{
				/* Saved successfully, quit now */
				Application.Exit();
			}
		}
		else if (result == DialogResult.Cancel)
		{
			if (e != null)
			{
				e.Cancel = true;
			}
		}
	}

	private void RedoMainMenuItem_Click(object sender, EventArgs e)
	{
		Workspace.Redo();
	}

	private void ReplaceToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Workspace.ReplaceSeletedItemByDialog();
	}

	/// <summary>
	/// Resets the editor form to the default layout with the default windows open and docked in their default locations.
	/// </summary>
	private void ResetToDefaultLayout()
	{
		this.browseTreeForm.Show(this.browserDockPanel);
		this.browseTreeForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document;
		this.browseTreeForm.CloseButton = false;
		this.browseTreeForm.CloseButtonVisible = false;
		this.browseTreeForm.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document | WeifenLuo.WinFormsUI.Docking.DockAreas.Float;
		this.browseTreeForm.HideOnClose = true;

		this.browseAllForm.Show(this.browserDockPanel);
		this.browseAllForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document;
		this.browseAllForm.CloseButton = false;
		this.browseAllForm.CloseButtonVisible = false;
		this.browseAllForm.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document | WeifenLuo.WinFormsUI.Docking.DockAreas.Float;
		this.browseAllForm.HideOnClose = true;

		this.archiveDebugForm.Show(this.browserDockPanel);
		this.archiveDebugForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document;
		this.archiveDebugForm.CloseButton = false;
		this.archiveDebugForm.CloseButtonVisible = false;
		this.archiveDebugForm.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document | WeifenLuo.WinFormsUI.Docking.DockAreas.Float;
		this.archiveDebugForm.HideOnClose = true;

		this.itemDebugForm.Show(this.browserDockPanel);
		this.itemDebugForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document;
		this.itemDebugForm.CloseButton = false;
		this.itemDebugForm.CloseButtonVisible = false;
		this.itemDebugForm.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document | WeifenLuo.WinFormsUI.Docking.DockAreas.Float;
		this.itemDebugForm.HideOnClose = true;

		this.selectedFilePropertyForm.Show(this.browserDockPanel);
		this.selectedFilePropertyForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
		this.selectedFilePropertyForm.Text = "Item Details";
		this.selectedFilePropertyForm.HideOnClose = true;

		this.consoleForm.Show(this.browserDockPanel);
		this.consoleForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
		this.consoleForm.Text = "Console";
		this.consoleForm.HideOnClose = true;
	}

	private async void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		await Workspace.SaveArchiveByDialogAsync();
	}

	private async void SaveToolStripMenuItem_Click(object sender, EventArgs e)
	{
		await Workspace.SaveArchiveAsync();
	}

	private void SetExtractionDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
	{
		SettingsService.ChooseQuickExtractDir();
	}

	private void TreeViewToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.browseTreeForm.Show();
		this.browseTreeForm.Focus();
	}

	private void UndoMainMenuItem_Click(object sender, EventArgs e)
	{
		Workspace.Undo();
	}

	private void UpdateMenuItems()
	{
		this.itemMainMenuItem.Visible = Workspace.SelectedItems.Count > 0;
		this.undoMainMenuItem.Enabled = Workspace.CanUndo;
		this.redoMainMenuItem.Enabled = Workspace.CanRedo;
		this.saveAsMainMenuItem.Enabled = Workspace.Archive != null;
		this.saveMainMenuItem.Enabled = Workspace.ArchiveIsModified;
		this.closeMainMenuItem.Enabled = Workspace.Archive != null;
	}

	private void UpdateTitle()
	{
		var archive = Workspace.Archive;
		var archivePath = archive != null ? Workspace.ArchiveSource!.FilePath : "";
		var modifiedStar = Workspace.ArchiveIsModified ? "*" : "";
		var separator = archive != null ? " - " : "";

		Text = $"{archivePath}{modifiedStar}{separator}NeFS Edit";
	}
}
