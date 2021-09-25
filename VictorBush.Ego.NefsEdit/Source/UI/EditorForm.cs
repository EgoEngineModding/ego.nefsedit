﻿// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using VictorBush.Ego.NefsEdit.Commands;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Workspace;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Main application form.
    /// </summary>
    internal partial class EditorForm : Form
    {
        private ArchiveDebugForm archiveDebugForm;
        private BrowseAllForm browseAllForm;
        private BrowseTreeForm browseTreeForm;
        private ConsoleForm consoleForm;
        private ItemDebugForm itemDebugForm;
        private PropertyGridForm selectedFilePropertyForm;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorForm"/> class.
        /// </summary>
        /// <param name="workspace">The workspace to use.</param>
        /// <param name="uiService">The UI service to use.</param>
        /// <param name="settingsService">The settings service to use.</param>
        public EditorForm(
            INefsEditWorkspace workspace,
            IUiService uiService,
            ISettingsService settingsService)
        {
            this.InitializeComponent();
            this.UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            this.SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            this.Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            this.Workspace.ArchiveOpened += this.OnWorkspaceArchiveOpened;
            this.Workspace.ArchiveClosed += this.OnWorkspaceArchiveClosed;
            this.Workspace.ArchiveSaved += this.OnWorkspaceArchiveSaved;
            this.Workspace.CommandExecuted += this.OnWorkspaceCommandExecuted;
            this.Workspace.SelectedItemsChanged += this.OnWorkspaceSelectedItemsChanged;
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
            if (this.Workspace.SelectedItems.Count == 0)
            {
                return;
            }

            /* Show the context menu */
            this.itemContextMenuStrip.Show(position);
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.UiService.ShowMessageBox($"Version {Application.ProductVersion}", title: "About");
        }

        private void ArchiveDebugMainMenuItem_Click(Object sender, EventArgs e)
        {
            this.archiveDebugForm.Show();
            this.archiveDebugForm.Focus();
        }

        private async void CloseMainMenuItem_Click(Object sender, EventArgs e)
        {
            await this.Workspace.CloseArchiveAsync();
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
            this.Quit(e);
        }

        private void EditorForm_Load(object sender, EventArgs e)
        {
            // Set the dockpanel theme
            var theme = new WeifenLuo.WinFormsUI.Docking.VS2015LightTheme();
            this.browserDockPanel.Theme = theme;

            // Create the different forms for the editor
            this.browseAllForm = new BrowseAllForm(this.Workspace, this, this.UiService);
            this.browseTreeForm = new BrowseTreeForm(this.Workspace, this, this.UiService);
            this.selectedFilePropertyForm = new PropertyGridForm();
            this.consoleForm = new ConsoleForm();
            this.archiveDebugForm = new ArchiveDebugForm(this.Workspace, this.UiService);
            this.itemDebugForm = new ItemDebugForm(this.Workspace, this.UiService);

            // Redirect standard output to our console form
            this.consoleForm.SetupConsole();

            // Reset the form layout to the default layout
            this.ResetToDefaultLayout();

            // Setup menu item initial state
            this.UpdateMenuItems();

            // Load settings
            this.SettingsService.Load();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Quit the application
            Application.Exit();
        }

        private async void ExtractToContextMenuItem_Click(object sender, EventArgs e)
        {
            await this.Workspace.ExtractItemsByDialogAsync(this.Workspace.SelectedItems);
        }

        private async void ExtractToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await this.Workspace.ExtractItemsByDialogAsync(this.Workspace.SelectedItems);
        }

        private void ItemDebugViewMainMenuItem_Click(Object sender, EventArgs e)
        {
            this.itemDebugForm.Show();
            this.itemDebugForm.Focus();
        }

        private void ItemDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.selectedFilePropertyForm.Show();
            this.selectedFilePropertyForm.Focus();
        }

        private void OnWorkspaceArchiveClosed(Object sender, EventArgs e)
        {
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.UpdateTitle();
                this.UpdateMenuItems();
            });
        }

        private void OnWorkspaceArchiveOpened(Object sender, EventArgs e)
        {
            // Update - must do on UI thread
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.UpdateTitle();
                this.UpdateMenuItems();
            });
        }

        private void OnWorkspaceArchiveSaved(Object sender, EventArgs e)
        {
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.UpdateTitle();
                this.UpdateMenuItems();
            });
        }

        private void OnWorkspaceCommandExecuted(object sender, NefsEditCommandEventArgs e)
        {
            this.UpdateTitle();
            this.UpdateMenuItems();
            this.selectedFilePropertyForm.RefreshGrid();
        }

        private void OnWorkspaceSelectedItemsChanged(Object sender, EventArgs e)
        {
            var items = this.Workspace.SelectedItems;

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
            this.UpdateMenuItems();

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
            await this.Workspace.OpenArchiveByDialogAsync();
        }

        private void OptionsMainMenuItem_Click(Object sender, EventArgs e)
        {
            this.UiService.ShowSettingsDialog(this.SettingsService);
        }

        private async void QuickExtractContextMenuItem_Click(object sender, EventArgs e)
        {
            await this.Workspace.ExtractItemsByQuickExtractAsync(this.Workspace.SelectedItems);
        }

        private async void QuickExtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await this.Workspace.ExtractItemsByQuickExtractAsync(this.Workspace.SelectedItems);
        }

        private async void Quit(FormClosingEventArgs e)
        {
            if (this.Workspace.Archive == null || !this.Workspace.ArchiveIsModified)
            {
                return;
            }

            // Archive has been modified; prompt to save before exit
            var result = this.UiService.ShowMessageBox(
                $"Save archive {this.Workspace.ArchiveSource.FilePath}?", "Save?", MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Yes)
            {
                // Cancel exiting the application - we need to wait for the save to finish
                if (e != null)
                {
                    e.Cancel = true;
                }

                /* Trigger the save */
                var saved = await this.Workspace.SaveArchiveAsync();
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

        private void RedoMainMenuItem_Click(Object sender, EventArgs e)
        {
            this.Workspace.Redo();
        }

        private void ReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Workspace.ReplaceSeletedItemByDialog();
        }

        /// <summary>
        /// Resets the editor form to the default layout with the default windows open and docked in
        /// their default locations.
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
            await this.Workspace.SaveArchiveByDialogAsync();
        }

        private async void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await this.Workspace.SaveArchiveAsync();
        }

        private void SetExtractionDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SettingsService.ChooseQuickExtractDir();
        }

        private void TreeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.browseTreeForm.Show();
            this.browseTreeForm.Focus();
        }

        private void UndoMainMenuItem_Click(Object sender, EventArgs e)
        {
            this.Workspace.Undo();
        }

        private void UpdateMenuItems()
        {
            this.itemMainMenuItem.Visible = this.Workspace.SelectedItems.Count > 0;
            this.undoMainMenuItem.Enabled = this.Workspace.CanUndo;
            this.redoMainMenuItem.Enabled = this.Workspace.CanRedo;
            this.saveAsMainMenuItem.Enabled = this.Workspace.Archive != null;
            this.saveMainMenuItem.Enabled = this.Workspace.ArchiveIsModified;
            this.closeMainMenuItem.Enabled = this.Workspace.Archive != null;
        }

        private void UpdateTitle()
        {
            var archive = this.Workspace.Archive;
            var archivePath = archive != null ? this.Workspace.ArchiveSource.FilePath : "";
            var modifiedStar = this.Workspace.ArchiveIsModified ? "*" : "";
            var separator = archive != null ? " - " : "";

            this.Text = $"{archivePath}{modifiedStar}{separator}NeFS Edit";
        }
    }
}
