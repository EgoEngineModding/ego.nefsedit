// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Extensions.Logging;
    using VictorBush.Ego.NefsEdit.Commands;
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
        private static readonly ILogger Log = LogHelper.GetLogger();

        private readonly Dictionary<NefsItem, ListViewItem> filesListItems = new Dictionary<NefsItem, ListViewItem>();

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
            this.Workspace.ArchiveClosed += this.OnWorkspaceArchiveClosed;
            this.Workspace.ArchiveSaved += this.OnWorkspaceArchiveSaved;
            this.Workspace.CommandExecuted += this.OnWorkspaceCommandExecuted;

            // Create the columns we want
            var columns = new ColumnHeader[]
            {
                new ColumnHeader() { Name = "id", Text = "Id" },
                new ColumnHeader() { Name = "filename", Text = "Name", Width = 200 },
                new ColumnHeader() { Name = "compressedSize", Text = "Compressed Size" },
                new ColumnHeader() { Name = "extractedSize", Text = "Extracted Size" },
            };

            this.filesListView.ShowItemToolTips = true;
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
                this.filesListView.Items.Clear();
                this.filesListItems.Clear();
                this.directoryTreeView.Nodes.Clear();
                return;
            }

            this.directoryTreeView.Nodes.Clear();

            var fileName = Path.GetFileName(this.Workspace.ArchiveSource.DataFilePath);
            var root = this.directoryTreeView.Nodes.Add(fileName);

            foreach (var item in archive.Items.EnumerateDepthFirst())
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
                        // Find this directory's parent directory
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

        private void OnWorkspaceArchiveClosed(Object sender, EventArgs e)
        {
            // Clear items list - must do on UI thread
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.LoadArchive(null);
            });
        }

        private void OnWorkspaceArchiveOpened(Object sender, EventArgs e)
        {
            // Update items list - must do on UI thread
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.LoadArchive(this.Workspace.Archive);
            });
        }

        private void OnWorkspaceArchiveSaved(Object sender, EventArgs e)
        {
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.LoadArchive(this.Workspace.Archive);
            });
        }

        private void OnWorkspaceCommandExecuted(Object sender, Commands.NefsEditCommandEventArgs e)
        {
            if (e.Command is ReplaceFileCommand replaceCommand)
            {
                if (!this.filesListItems.ContainsKey(replaceCommand.Item))
                {
                    // An item was replaced, but its not in the current view
                    return;
                }

                var listItem = this.filesListItems[replaceCommand.Item];
                this.UpdateListItem(listItem);
            }
            else if (e.Command is RemoveFileCommand removeCommand)
            {
                if (!this.filesListItems.ContainsKey(removeCommand.Item))
                {
                    // An item was removed, but its not in the current view
                    return;
                }

                var listItem = this.filesListItems[removeCommand.Item];
                this.UpdateListItem(listItem);
            }
        }

        /// <summary>
        /// Opens a directory in the archive for viewing.
        /// </summary>
        /// <param name="dir">The directory to view. Use null for root directory.</param>
        private void OpenDirectory(NefsItem dir)
        {
            IEnumerable<NefsItem> itemsInDir;
            var items = this.Workspace.Archive.Items;

            // Clear the directory contents list view
            this.filesListView.Items.Clear();
            this.filesListItems.Clear();

            this.Directory = dir;
            if (dir == null)
            {
                // Display contents of root
                itemsInDir = items.EnumerateRootItems();

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
                itemsInDir = items.EnumerateItemChildren(dir.Id);
                this.pathLabel.Text = @"\" + items.GetItemFilePath(dir.Id);
            }

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

                this.filesListItems.Add(item, listItem);
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
            var parentId = this.Workspace.Archive.Items.GetItemDirectoryId(this.Directory.DirectoryId);
            var parent = this.Workspace.Archive.Items.GetItem(parentId);
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

        /// <summary>
        /// Updates visual appearance of a file list item.
        /// </summary>
        /// <param name="listItem">The item to update.</param>
        private void UpdateListItem(ListViewItem listItem)
        {
            if (!(listItem.Tag is NefsItem item))
            {
                Log.LogError("List view item did not have NefsItem as tag.");
                return;
            }

            switch (item.State)
            {
                case NefsItemState.None:
                    listItem.ForeColor = Color.Black;
                    break;

                case NefsItemState.Added:
                    listItem.ForeColor = Color.Green;
                    listItem.ToolTipText = $"Item will be added with {item.DataSource.FilePath}";
                    break;

                case NefsItemState.Removed:
                    listItem.ForeColor = Color.Red;
                    listItem.ToolTipText = "Item will be removed.";
                    break;

                case NefsItemState.Replaced:
                    listItem.ForeColor = Color.Blue;
                    listItem.ToolTipText = $"Item will be replaced with {item.DataSource.FilePath}";
                    break;
            }
        }
    }
}
