// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Utility;
    using VictorBush.Ego.NefsEdit.Workspace;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.Item;
    using WeifenLuo.WinFormsUI.Docking;

    /// <summary>
    /// Displays archive with tree browser.
    /// </summary>
    internal partial class BrowseTreeForm : DockContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowseTreeForm"/> class.
        /// </summary>
        /// <param name="workspace">The workspace to use.</param>
        /// <param name="editorForm">The editor form.</param>
        /// <param name="uiService">The UI service.</param>
        internal BrowseTreeForm(
            INefsEditWorkspace workspace,
            EditorForm editorForm,
            IUiService uiService)
        {
            this.InitializeComponent();
            this.EditorForm = editorForm ?? throw new ArgumentNullException(nameof(editorForm));
            this.UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            this.Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            this.Workspace.ArchiveOpened += this.OnWorkspaceArchiveOpened;

            // Create the columns we want
            var columns = new ColumnHeader[]
            {
                new ColumnHeader() { Name = "id", Text = "Id" },
                new ColumnHeader() { Name = "filename", Text = "Name", Width = 200 },
                new ColumnHeader() { Name = "compressedSize", Text = "Compressed Size" },
                new ColumnHeader() { Name = "extractedSize", Text = "Extracted Size" },
            };

            this.filesListView.Columns.AddRange(columns);
        }

        /// <summary>
        /// Gets the current directory. Will be null if the root directory.
        /// </summary>
        private NefsItem Directory { get; set; }

        private EditorForm EditorForm { get; }

        private IUiService UiService { get; }

        private INefsEditWorkspace Workspace { get; }

        private void AddSubItem(ListViewItem item, string name, string text)
        {
            item.SubItems.Add(new ListViewItem.ListViewSubItem()
            {
                Name = name,
                Text = text,
            });
        }

        private void DirectoryTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                this.OpenDirectory((NefsItem)e.Node.Tag);
            }
        }

        private void FilesListView_DoubleClick(object sender, EventArgs e)
        {
            if (this.filesListView.SelectedItems.Count > 0)
            {
                var item = this.filesListView.SelectedItems[0].Tag as NefsItem;
                if (item.Type == NefsItemType.Directory)
                {
                    this.OpenDirectory(item);
                }
            }
        }

        private void FilesListView_MouseUp(object sender, MouseEventArgs e)
        {
            // Show context menu if an item is right-clicked
            if (e.Button == MouseButtons.Right)
            {
                this.EditorForm.ShowItemContextMenu(Cursor.Position);
            }
        }

        private void FilesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItems = this.filesListView.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return;
            }

            /* Build a list of selected NefsItems */
            var selectedNefsItems = new List<NefsItem>();

            foreach (ListViewItem item in this.filesListView.SelectedItems)
            {
                selectedNefsItems.Add(item.Tag as NefsItem);
            }

            // Tell the editor what items are selected
            this.Workspace.SelectItems(selectedNefsItems);
        }

        private void LoadArchive(NefsArchive archive)
        {
            if (archive == null)
            {
                return;
            }

            this.directoryTreeView.Nodes.Clear();

            // TODO : Change the root node to the name of the archive?
            var root = this.directoryTreeView.Nodes.Add("root");

            foreach (var item in archive.Items)
            {
                if (item.Type == NefsItemType.Directory)
                {
                    if (item.Id == item.DirectoryId)
                    {
                        // This directory is at the root level
                        var newNode = root.Nodes.Add(item.FileName);
                        newNode.Tag = item;
                    }
                    else
                    {
                        /* Find this directory's parent directory */
                        var parent = (from n in root.DescendantNodes()
                                      where n.Tag != null && ((NefsItem)n.Tag).Id == item.DirectoryId
                                      select n).FirstOrDefault();

                        if (parent == null)
                        {
                            // TODO : FIX THIS
                            MessageBox.Show("LOL");
                        }

                        var newNode = parent.Nodes.Add(item.FileName);
                        newNode.Tag = item;
                    }
                }
            }

            root.Expand();

            this.OpenDirectory(null);
        }

        private void OnWorkspaceArchiveOpened(Object sender, EventArgs e)
        {
            // Update items list - must do on UI thread
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.LoadArchive(this.Workspace.Archive);
            });
        }

        /// <summary>
        /// Opens a directory in the archive for viewing.
        /// </summary>
        /// <param name="dir">The directory to view. Use null for root directory.</param>
        private void OpenDirectory(NefsItem dir)
        {
            List<NefsItem> itemsInDir;
            var archive = this.Workspace.Archive;

            this.Directory = dir;
            if (dir == null)
            {
                // Display contents of root
                itemsInDir = (from item in archive.Items
                              where item.Id == item.DirectoryId
                              select item).ToList();

                // Clear the directory contents list view
                this.filesListView.Items.Clear();

                // This is the root of the archive
                this.pathLabel.Text = @"\";
            }
            else
            {
                if (dir.Type != NefsItemType.Directory)
                {
                    // TODO : FIX
                    MessageBox.Show("TODO: Log this --- can't browse a file.");
                }

                // Display contents of specified directory
                itemsInDir = (from item in archive.Items
                              where item.DirectoryId == dir.Id && item.DirectoryId != item.Id
                              select item).ToList();

                this.pathLabel.Text = @"\" + dir.FilePathInArchive;
            }

            // Clear the directory contents list view
            this.filesListView.Items.Clear();

            // Load all items in the NeFS archive into the listview
            foreach (var item in itemsInDir)
            {
                var listItem = new ListViewItem();

                // The list item is actually the first column
                listItem.Text = item.Id.Value.ToString("X");

                // Save a reference to the item object
                listItem.Tag = item;

                this.AddSubItem(listItem, "filename", item.FileName);

                if (item.Type == NefsItemType.File)
                {
                    this.AddSubItem(listItem, "compressedSize", item.CompressedSize.ToString("X"));
                    this.AddSubItem(listItem, "extractedSize", item.ExtractedSize.ToString("X"));
                }

                if (item.Type == NefsItemType.Directory)
                {
                    listItem.BackColor = Color.LightBlue;
                }

                this.filesListView.Items.Add(listItem);
            }
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            if (this.Directory == null)
            {
                // Can't go up a directory
                return;
            }

            // Find the parent directory
            var parent = this.Workspace.Archive.Items
                .Where(i => i.Id == this.Directory.DirectoryId).FirstOrDefault();

            if (parent == this.Directory)
            {
                // If the parent == the current dir, then display root
                this.OpenDirectory(null);
            }
            else
            {
                this.OpenDirectory(parent);
            }
        }
    }
}
