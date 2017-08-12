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
        NefsItem _selectedItem;

        public EditorForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Extracts the specified item to a location chosen by the user.
        /// </summary>
        /// <param name="item">The item to extract.</param>
        public async void ExtractItemTo(NefsItem item)
        {
            if (item == null)
            {
                MessageBox.Show("No item selected to extract.");
                return;
            }

            /*
             * Open a save file dialog and get the output location
             */
            var sfd = new SaveFileDialog();
            sfd.OverwritePrompt = true;
            sfd.FileName = item.Filename;
            var result = sfd.ShowDialog();

            if (result == DialogResult.OK)
            {
                /* Create a progress dialog form */
                var progressDialog = new ProgressDialogForm();
                progressDialog.SetStyle(ProgressBarStyle.Marquee);

                /* Show the loading dialog asnyc */
                var progressDialogTask = progressDialog.ShowDialogAsync();

                /* Extract the item */
                await Task.Run(() =>
                {
                    try
                    {
                        log.Info("----------------------------");
                        log.Info(String.Format("Extracting {0} to {1}...", item.Filename, sfd.FileName));
                        item.Extract(sfd.FileName, progressDialog.ProgressInfo);
                        log.Info("Item extracted successfully: " + sfd.FileName);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error extracting file.", ex);
                    }
                });

                /* Close the progress dialog */
                progressDialog.Close();
            }
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
        /// Saves the specified archive to a location chosen by the user.
        /// </summary>
        /// <param name="archive">The archive to save.</param>
        public async void SaveArchiveAsAsync(NefsArchive archive)
        {
            if (archive == null)
            {
                MessageBox.Show("No archive selected to save.");
                return;
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
                        log.Info(String.Format("Saving archive to {0}...", sfd.FileName));
                        archive.Save(sfd.FileName, progressDialog.ProgressInfo);
                        log.Info("Item successfully saved: " + sfd.FileName);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error saving archive.", ex);
                    }
                });

                /* Update editor */
                setArchive(archive);
                SelectNefsItem(_selectedItem);
                updateTitle();

                /* Close the progress dialog */
                progressDialog.Close();
            }
        }

        /// <summary>
        /// Indicate that an item has been selected and allow appropriate parties to take action.
        /// </summary>
        /// <param name="item">The item that has been selected.</param>
        public void SelectNefsItem(NefsItem item)
        {
            _selectedItem = item;
            _selectedFilePropertyForm.SetSelectedObject(item);
            
            if (item != null && item.Type == NefsItem.NefsItemType.File)
            {
                itemToolStripMenuItem.Visible = true;
            }
            else
            {
                itemToolStripMenuItem.Visible = false;
            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO : Startup items
            //  - verify write access / admin privellegeee?
            // - try to find location of DiRT 4 in steamapps folder? https://stackoverflow.com/questions/29036572/how-to-find-the-path-to-steams-sourcemods-folder

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
            SelectNefsItem(null);
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

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExtractItemTo(_selectedItem);
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
            ReplaceItem(_selectedItem);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveArchiveAsAsync(_archive);
        }

        private void quit(FormClosingEventArgs e)
        {
            // TODO : Cleanup this function

            if (_archive != null && _archive.Modified)
            {
                var result = MessageBox.Show(String.Format("Save archive {0}?", _archive.FilePath), 
                                            "Save?", 
                                            MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    SaveArchiveAsAsync(_archive);
                    if (e != null)
                    {
                        e.Cancel = true;
                    }
                    return; // TODO : Check if save was successful, then exit
                }
                else if (result == DialogResult.Cancel)
                {
                    if (e != null)
                    {
                        e.Cancel = true;
                    }
                    return;
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

        private void itemDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _selectedFilePropertyForm.Show();
            _selectedFilePropertyForm.Focus();
        }

        private void treeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _browseTreeForm.Show();
            _browseTreeForm.Focus();
        }

        private void debugViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _browseAllForm.Show();
            _browseAllForm.Focus();
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _consoleForm.Show();
            _consoleForm.Focus();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* Quit the application */
            Application.Exit();
        }

        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            quit(e);
        }
    }
}
