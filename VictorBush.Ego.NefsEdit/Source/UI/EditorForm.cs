using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VictorBush.Ego.NefsEdit.Utility;
using VictorBush.Ego.NefsEdit.Workspace;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsEdit.UI
{
    internal partial class EditorForm : Form
    {
        private static readonly ILog Log = LogHelper.GetLogger();

        private PropertyGridForm archivePropertyForm;
        private BrowseAllForm browseAllForm;
        private BrowseTreeForm browseTreeForm;
        private ConsoleForm consoleForm;

        private PropertyGridForm selectedFilePropertyForm;

        /// <summary>
        /// Gets the workspace.
        /// </summary>
        private INefsEditWorkspace Workspace { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EditorForm"/> class.
        /// </summary>
        /// <param name="workspace">The workspace to use.</param>
        public EditorForm(INefsEditWorkspace workspace)
        {
            this.InitializeComponent();

            this.Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            this.Workspace.ArchiveOpened += this.OnWorkspaceArchiveOpened;
            this.Workspace.ArchiveClosed += this.OnWorkspaceArchiveClosed;
            this.Workspace.ArchiveSaved += this.OnWorkspaceArchiveSaved;
            this.Workspace.SelectedItemsChanged += this.OnWorkspaceSelectedItemsChanged;
        }

        private void OnWorkspaceArchiveSaved(Object sender, EventArgs e)
        {
            this.UpdateTitle();
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
            this.ItemToolStripMenuItem.Visible = items.Count > 0;

            if (items.Count == 0)
            {
                return;
            }

            // Set visibility of the Replace menu option
            if (items.Count > 1 || items[0].Type == NefsItemType.Directory)
            {
                // Can't replace directories or multiple files right now
                this.ReplaceContextMenuItem.Visible = false;
                this.ReplaceToolStripMenuItem.Enabled = false;
            }
            else
            {
                // Single file selected, can replace
                this.ReplaceContextMenuItem.Visible = true;
                this.ReplaceToolStripMenuItem.Enabled = true;
            }
        }

        private void OnWorkspaceArchiveClosed(Object sender, EventArgs e)
        {
            this.archivePropertyForm.SetSelectedObject(null);
            this.UpdateTitle();
        }

        private void OnWorkspaceArchiveOpened(Object sender, EventArgs e)
        {
            this.archivePropertyForm.SetSelectedObject(this.Workspace.Archive);
            this.UpdateTitle();
        }



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

        private void EditorForm_Load(object sender, EventArgs e)
        {
            // TODO : Startup items
            //  - verify write access / admin privellegeee?
            // - try to find location of DiRT 4 in steamapps folder? https://stackoverflow.com/questions/29036572/how-to-find-the-path-to-steams-sourcemods-folder

            // Load settings
            Settings.LoadSettings();

            // Set the dockpanel theme
            var theme = new WeifenLuo.WinFormsUI.Docking.VS2015LightTheme();
            this.browserDockPanel.Theme = theme;

            // Create the different forms for the editor
            this.browseAllForm = new BrowseAllForm(this.Workspace, this);
            this.browseTreeForm = new BrowseTreeForm(this.Workspace, this);
            this.selectedFilePropertyForm = new PropertyGridForm();
            this.archivePropertyForm = new PropertyGridForm();
            this.consoleForm = new ConsoleForm();

            // Redirect standard output to our console form
            this.consoleForm.SetupConsole();

            // Reset the form layout to the default layout
            this.ResetToDefaultLayout();
        }

        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //quit(e);
        }

        /// <summary>
        /// Resets the editor form to the default layout with the default windows open
        /// and docked in their default locations.
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

            this.selectedFilePropertyForm.Show(this.browserDockPanel);
            this.selectedFilePropertyForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
            this.selectedFilePropertyForm.Text = "Item Details";
            this.selectedFilePropertyForm.HideOnClose = true;

            this.archivePropertyForm.Show(this.browserDockPanel);
            this.archivePropertyForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
            this.archivePropertyForm.Text = "Archive Details";
            this.archivePropertyForm.HideOnClose = true;

            this.consoleForm.Show(this.browserDockPanel);
            this.consoleForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
            this.consoleForm.Text = "Console";
            this.consoleForm.HideOnClose = true;
        }


        private async void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await this.Workspace.OpenArchiveByDialogAsync();
        }


        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Workspace.ReplaceSeletedItemByDialog();
        }

        private async void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await this.Workspace.SaveArchiveByDialogAsync();
        }

        //private async void quit(FormClosingEventArgs e)
        //{
        //    if (_archive != null && _archive.Modified)
        //    {
        //        /* Archive has been modified; prompt to save before exit */
        //        var result = MessageBox.Show(String.Format("Save archive {0}?", _archive.FilePath), 
        //                                    "Save?", 
        //                                    MessageBoxButtons.YesNoCancel);

        //        if (result == DialogResult.Yes)
        //        {
        //            /* Cancel exiting the application - we need to wait for the save to finish */
        //            if (e != null)
        //            {
        //                e.Cancel = true;
        //            }

        //            /* Trigger the save */
        //            var saved = await SaveArchiveAsync(_archive, _archive.FilePath);

        //            if (saved)
        //            {
        //                /* Saved successfully, quit now */
        //                Application.Exit();
        //            }
        //        }
        //        else if (result == DialogResult.Cancel)
        //        {
        //            if (e != null)
        //            {
        //                e.Cancel = true;
        //            }
        //        }
        //    }
        //}

        private void UpdateTitle()
        {
            this.Text = "NeFS Edit";

            if (this.Workspace.Archive != null)
            {
                this.Text += " - ";

                if (this.Workspace.ArchiveIsModified)
                {
                    this.Text += "*";
                }

                this.Text += this.Workspace.ArchiveFilePath;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(String.Format("Version {0}", Application.ProductVersion));
        }

        private void archiveDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            archivePropertyForm.Show();
            archivePropertyForm.Focus();
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            consoleForm.Show();
            consoleForm.Focus();
        }

        private void debugViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            browseAllForm.Show();
            browseAllForm.Focus();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* Quit the application */
            Application.Exit();
        }
        
        private void quickExtractContextMenuItem_Click(object sender, EventArgs e)
        {
            //ExtractItems(_selectedItems, true);
        }

        private void quickExtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ExtractItems(_selectedItems, true);
        }

        private void extractToContextMenuItem_Click(object sender, EventArgs e)
        {
            //ExtractItems(_selectedItems, false);
        }

        private void extractToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ExtractItems(_selectedItems, false);
        }
        
        private void itemDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedFilePropertyForm.Show();
            selectedFilePropertyForm.Focus();
        }

        private void ReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Workspace.ReplaceSeletedItemByDialog();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Workspace.SaveArchiveAsync();
        }

        private void TreeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.browseTreeForm.Show();
            this.browseTreeForm.Focus();
        }

        private void setExtractionDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.ChooseQuickExtractDir();
        }
    }
}
