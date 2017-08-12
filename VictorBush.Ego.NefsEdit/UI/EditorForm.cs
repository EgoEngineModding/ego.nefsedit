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
using VictorBush.Ego.NefsLib;

namespace VictorBush.Ego.NefsEdit.UI
{
    public partial class EditorForm : Form
    {
        private static readonly ILog log = LogHelper.GetLogger();

        NefsArchive _archive;
        PropertyGridForm _archivePropertyForm;
        BrowseAllForm _browseAllForm;
        BrowseTreeForm _browseTreeForm;
        ConsoleForm _consoleForm;
        PropertyGridForm _selectedFilePropertyForm;
        List<NefsItem> _selectedItems = new List<NefsItem>();

        public EditorForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Extracts the specified items.
        /// </summary>
        /// <param name="items">The items to extract.</param>
        /// <param name="useQuickExtract">Whether or not to extract to the quick extract directory.</param>
        public async void ExtractItems(List<NefsItem> items, bool useQuickExtract)
        {
            if (items == null || items.Count == 0)
            {
                MessageBox.Show("No items selected to selected.");
                return;
            }

            var outputDir = "";
            var outputFile = "";

            if (useQuickExtract)
            {
                /*
                 * Use the quick extraction directory
                 */
                if (!Directory.Exists(Settings.QuickExtractDir))
                {
                    /* Quick extract dir not set, have user choose one */
                    if (!Settings.ChooseQuickExtractDir())
                    {
                        /* User cancelled the directory selection */
                        return;
                    }
                }

                outputDir = Settings.QuickExtractDir;
            }
            else
            {
                /*
                 * Have user choose where to save the items
                 */
                var result = DialogResult.Cancel;

                /* Show either a directory chooser or a save file dialog */
                if (items.Count > 1 || items[0].Type == NefsItem.NefsItemType.Directory)
                {
                    /* Extracting multiple files or a directory - show folder browser */
                    var fbd = new FolderBrowserDialog();
                    fbd.Description = "Choose where to extract the items to.";
                    fbd.ShowNewFolderButton = true;

                    result = fbd.ShowDialog();
                    outputDir = fbd.SelectedPath;
                }
                else
                {
                    /* Extracting a file - show a save file dialog*/
                    var sfd = new SaveFileDialog();
                    sfd.OverwritePrompt = true;
                    sfd.FileName = items[0].Filename;

                    result = sfd.ShowDialog();
                    outputDir = Path.GetDirectoryName(sfd.FileName);
                    outputFile = Path.GetFileName(sfd.FileName);
                }

                if (result != DialogResult.OK)
                {
                    /* Use canceled the dialog box */
                    return;
                }
            }

            /* Create a progress dialog form */
            var progressDialog = new ProgressDialogForm();

            /* Show the loading dialog asnyc */
            var progressDialogTask = progressDialog.ShowDialogAsync();

            /* Extract the item */
            await Task.Run(() =>
            {
                try
                {
                    var p = progressDialog.ProgressInfo;
                    var numItems = _selectedItems.Count;

                    log.Info("----------------------------");
                    p.BeginTask(1.0f);

                    /* Extract each item */
                    for (int i = 0; i < numItems; i++)
                    {
                        var item = _selectedItems[i];
                        var dir = outputDir;
                        var file = outputFile;

                        /* When extracting multiple items or using the quick extraction 
                         * directory, use the original filenames and directory structure
                         * of the archive */
                        if (numItems > 0 || useQuickExtract)
                        {
                            var dirInArchive = Path.GetDirectoryName(item.FilePathInArchive);
                            dir = Path.Combine(outputDir, dirInArchive);
                            file = Path.GetFileName(item.FilePathInArchive);
                        }

                        log.Info(String.Format("Extracting {0} to {1}...", item.FilePathInArchive, Path.Combine(dir, file)));
                        try
                        {
                            item.Extract(dir, file, p);
                        }
                        catch (Exception ex)
                        {
                            log.Error(String.Format("Error extracting item {0}.", item.FilePathInArchive));
                        }
                    }

                    p.EndTask();
                    log.Info("Extraction finished.");
                }
                catch (Exception ex)
                {
                    log.Error("Error extracting items.", ex);
                }
            });

            /* Close the progress dialog */
            progressDialog.Close();
        }
        
        /// <summary>
        /// Replaces the selected item with a new item.
        /// </summary>
        /// <param name="item">The item to replace.</param>
        public async void ReplaceItem(NefsItem item)
        {
            if (item == null)
            {
                MessageBox.Show("No item selected to replace.");
                return;
            }

            /*
             * Open an open file dialog and get file to inject
             */
            var ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            var result = ofd.ShowDialog();

            if (result == DialogResult.OK)
            {
                /* Create a progress dialog form */
                var progressDialog = new ProgressDialogForm();

                /* Show the progress dialog asnyc */
                var progressDialogTask = progressDialog.ShowDialogAsync();

                /* Replace the item */
                await Task.Run(() =>
                {
                    try
                    {
                        log.Info("----------------------------");
                        log.Info(String.Format("Replacing {0} with {1}...", item.Filename, ofd.FileName));
                        item.Inject(ofd.FileName, progressDialog.ProgressInfo);
                        log.Info("Item successfully replaced with: " + ofd.FileName);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error replacing file.", ex);
                    }
                });

                /* Post-replace activites */
                updateTitle();

                /* Close the progress dialog */
                progressDialog.Close();
            }
        }

        /// <summary>
        /// Replaces the selected item with a file the user chooses.
        /// </summary>
        public void ReplaceSelectedItem()
        {
            if (_selectedItems.Count == 0)
            {
                MessageBox.Show("No item selected to replace.");
                return;
            }

            if (_selectedItems.Count > 1 )
            {
                MessageBox.Show("Replacing multiple files not supported.");
                return;
            }

            if (_selectedItems[0].Type == NefsItem.NefsItemType.Directory)
            {
                MessageBox.Show("Replacing directories not supported.");
                return;
            }

            ReplaceItem(_selectedItems[0]);
        }

        /// <summary>
        /// Saves the specified archive to a location chosen by the user.
        /// </summary>
        /// <param name="archive">The archive to save.</param>
        public async Task<bool> SaveArchiveAsAsync(NefsArchive archive)
        {
            if (archive == null)
            {
                MessageBox.Show("No archive selected to save.");
                return false;
            }

            /*
             * Open an open file dialog and get file to inject
             */
            var sfd = new SaveFileDialog();
            sfd.OverwritePrompt = true;
            sfd.FileName = Path.GetFileName(archive.FilePath);
            var result = sfd.ShowDialog();

            if (result == DialogResult.OK)
            {
                var success = await SaveArchiveAsync(archive, sfd.FileName);
                return success;
            }

            return false;
        }

        /// <summary>
        /// Saves the specified archive to the specified location.
        /// </summary>
        /// <param name="archive">The archive to save.</param>
        /// <param name="filename">Location to save to.</param>
        public async Task<bool> SaveArchiveAsync(NefsArchive archive, string filename)
        {
            var success = false;

            /* Create a progress dialog form */
            var progressDialog = new ProgressDialogForm();

            /* Show the progress dialog asnyc */
            var progressDialogTask = progressDialog.ShowDialogAsync();

            /* Replace the item */
            await Task.Run(() =>
            {
                try
                {
                    log.Info("----------------------------");
                    log.Info(String.Format("Saving archive to {0}...", filename));
                    archive.Save(filename, progressDialog.ProgressInfo);
                    log.Info("Item successfully saved: " + filename);
                    success = true;
                }
                catch (Exception ex)
                {
                    log.Error("Error saving archive.", ex);
                }
            });

            /* Update editor */
            setArchive(archive);
            SelectNefsItem(_selectedItems);
            updateTitle();

            /* Close the progress dialog */
            progressDialog.Close();

            return success;
        }

        /// <summary>
        /// Saves the current archive if one is open.
        /// </summary>
        public void SaveCurrentArchive()
        {
            if (_archive == null)
            {
                return;
            }

            SaveArchiveAsync(_archive, _archive.FilePath);
        }

        /// <summary>
        /// Indicate that an item has been selected and allow appropriate parties to take action.
        /// </summary>
        /// <param name="item">List of selected items.</param>
        public void SelectNefsItem(List<NefsItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("Cannot provide a null list of items to select.");
            }

            _selectedItems = items;

            /* Set which item shows up in the Item's property window */
            if (_selectedItems.Count == 1)
            {
                _selectedFilePropertyForm.SetSelectedObject(_selectedItems[0]);
            }
            else
            {
                _selectedFilePropertyForm.SetSelectedObject(null);
            }

            /* Set "Item" menu visibility */
            itemToolStripMenuItem.Visible = (items.Count > 0);

            if (items.Count == 0)
            {
                return;
            }

            /* Set visibility of the Replace menu option */
            if (_selectedItems.Count > 1
             || _selectedItems[0].Type == NefsItem.NefsItemType.Directory)
            {
                /* Can't replace directories or multiple files right now */
                replaceContextMenuItem.Visible = false;
                replaceToolStripMenuItem.Enabled = false;
            }
            else
            {
                /* Single file selected, can replace */
                replaceContextMenuItem.Visible = true;
                replaceToolStripMenuItem.Enabled = true;
            }
        }

        /// <summary>
        /// Opens the item context menu if there is an item selected.
        /// </summary>
        /// <param name="position">Where to open the menu at.</param>
        public void ShowItemContextMenu(Point position)
        {
            if (_selectedItems.Count == 0)
            {
                return;
            }

            /* Show the context menu */
            itemContextMenuStrip.Show(position);
        }

        private void EditorForm_Load(object sender, EventArgs e)
        {
            // TODO : Startup items
            //  - verify write access / admin privellegeee?
            // - try to find location of DiRT 4 in steamapps folder? https://stackoverflow.com/questions/29036572/how-to-find-the-path-to-steams-sourcemods-folder

            /* Load settings */
            Settings.LoadSettings();

            /* Set the dockpanel theme */
            var theme = new WeifenLuo.WinFormsUI.Docking.VS2015LightTheme();
            browserDockPanel.Theme = theme;

            /* Create the different forms for the editor */
            _browseAllForm = new BrowseAllForm(this);
            _browseTreeForm = new BrowseTreeForm(this);
            _selectedFilePropertyForm = new PropertyGridForm();
            _archivePropertyForm = new UI.PropertyGridForm();
            _consoleForm = new ConsoleForm();
            
            /* Redirect standard output to our console form */
            _consoleForm.SetupConsole();

            /* Reset the form layout to the default layout */
            resetToDefaultLayout();

            /* Clear the selected nefs archive */
            _selectedItems = new List<NefsItem>();
            SelectNefsItem(_selectedItems);
        }
        
        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            quit(e);
        }

        /// <summary>
        /// Resets the editor form to the default layout with the default windows open
        /// and docked in their default locations.
        /// </summary>
        private void resetToDefaultLayout()
        {
            _browseTreeForm.Show(browserDockPanel);
            _browseTreeForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            _browseTreeForm.CloseButton = false;
            _browseTreeForm.CloseButtonVisible = false;
            _browseTreeForm.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document | WeifenLuo.WinFormsUI.Docking.DockAreas.Float;
            _browseTreeForm.HideOnClose = true;

            _browseAllForm.Show(browserDockPanel);
            _browseAllForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            _browseAllForm.CloseButton = false;
            _browseAllForm.CloseButtonVisible = false;
            _browseAllForm.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document | WeifenLuo.WinFormsUI.Docking.DockAreas.Float;
            _browseAllForm.HideOnClose = true;

            _selectedFilePropertyForm.Show(browserDockPanel);
            _selectedFilePropertyForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
            _selectedFilePropertyForm.Text = "Item Details";
            _selectedFilePropertyForm.HideOnClose = true;

            _archivePropertyForm.Show(browserDockPanel);
            _archivePropertyForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
            _archivePropertyForm.Text = "Archive Details";
            _archivePropertyForm.HideOnClose = true;

            _consoleForm.Show(browserDockPanel);
            _consoleForm.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
            _consoleForm.Text = "Console";
            _consoleForm.HideOnClose = true;
        }



        private async void loadNefsAsync(string filePath)
        {
            /* Create a progress dialog form */
            var progressDialog = new ProgressDialogForm();

            /* Show the loading dialog asnyc */
            var progressDialogTask = progressDialog.ShowDialogAsync();
            
            /* Load the archive */
            var archive = await NefsArchive.LoadAsync(filePath, progressDialog.ProgressInfo);

            /* Close the progress dialog */
            progressDialog.Close();

            /* After loading activities */
            setArchive(archive);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            
            var result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                loadNefsAsync(ofd.FileName);
            }
        }

        private void setArchive(NefsArchive archive)
        {
            _archive = archive;
            _archivePropertyForm.SetSelectedObject(archive);
            _browseAllForm.LoadArchive(archive);
            _browseTreeForm.LoadArchive(archive);

            updateTitle();
        }    

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReplaceSelectedItem();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveArchiveAsAsync(_archive);
        }

        private async void quit(FormClosingEventArgs e)
        {
            if (_archive != null && _archive.Modified)
            {
                /* Archive has been modified; prompt to save before exit */
                var result = MessageBox.Show(String.Format("Save archive {0}?", _archive.FilePath), 
                                            "Save?", 
                                            MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    /* Cancel exiting the application - we need to wait for the save to finish */
                    if (e != null)
                    {
                        e.Cancel = true;
                    }

                    /* Trigger the save */
                    var saved = await SaveArchiveAsync(_archive, _archive.FilePath);
                    
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
        }

        private void updateTitle()
        {
            this.Text = "NeFS Edit";

            if (_archive != null)
            {
                this.Text += " - ";

                if (_archive.Modified)
                    this.Text += "*";

                this.Text += _archive.FilePath;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(String.Format("Version {0}", Application.ProductVersion));
        }

        private void archiveDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _archivePropertyForm.Show();
            _archivePropertyForm.Focus();
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _consoleForm.Show();
            _consoleForm.Focus();
        }

        private void debugViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _browseAllForm.Show();
            _browseAllForm.Focus();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* Quit the application */
            Application.Exit();
        }
        
        private void quickExtractContextMenuItem_Click(object sender, EventArgs e)
        {
            ExtractItems(_selectedItems, true);
        }

        private void quickExtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExtractItems(_selectedItems, true);
        }

        private void extractToContextMenuItem_Click(object sender, EventArgs e)
        {
            ExtractItems(_selectedItems, false);
        }

        private void extractToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExtractItems(_selectedItems, false);
        }
        
        private void itemDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _selectedFilePropertyForm.Show();
            _selectedFilePropertyForm.Focus();
        }

        private void replaceToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReplaceSelectedItem();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveCurrentArchive();
        }

        private void treeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _browseTreeForm.Show();
            _browseTreeForm.Focus();
        }

        private void setExtractionDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.ChooseQuickExtractDir();
        }
    }
}
