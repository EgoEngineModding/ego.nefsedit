// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
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
    /// Form that displays all items in the archive in a single list.
    /// </summary>
    internal partial class BrowseAllForm : DockContent
    {
        private static readonly ILogger Log = LogHelper.GetLogger();
        private readonly Dictionary<NefsItem, ListViewItem> listItems = new Dictionary<NefsItem, ListViewItem>();
        private readonly ListViewColumnSorter listViewItemSorter = new ListViewColumnSorter();

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowseAllForm"/> class.
        /// </summary>
        /// <param name="workspace">The workspace to use.</param>
        /// <param name="editorForm">The editor form.</param>
        /// <param name="uiService">UI service to use.</param>
        internal BrowseAllForm(
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

            this.itemsListView.ListViewItemSorter = this.listViewItemSorter;

            // Create the columns we want
            var columns = new ColumnHeader[]
            {
                new ColumnHeader() { Name = "id", Text = "Id" },
                new ColumnHeader() { Name = "filename", Text = "Filename", Width = 200 },
                new ColumnHeader() { Name = "directoryId", Text = "Directory Id" },
                new ColumnHeader() { Name = "compressedSize", Text = "Compressed Size" },
                new ColumnHeader() { Name = "extractedSize", Text = "Extracted Size" },
            };

            var debugColumns = new ColumnHeader[]
            {
                new ColumnHeader() { Name = "pt1.0x00", Text = "[pt1.0x00] Offset to Data" },
                new ColumnHeader() { Name = "pt1.0x08", Text = "[pt1.0x08] Index into pt2" },
                new ColumnHeader() { Name = "pt1.0x0c", Text = "[pt1.0x0c] Index into pt4 (chunk sizes)" },
                new ColumnHeader() { Name = "pt1.0x10", Text = "[pt1.0x10] Id" },

                new ColumnHeader() { Name = "pt2.0x00", Text = "[pt2.0x00] Directory Id" },
                new ColumnHeader() { Name = "pt2.0x04", Text = "[pt2.0x04] First Child" },
                new ColumnHeader() { Name = "pt2.0x08", Text = "[pt2.0x08] Offset into pt3 (filename strings)" },
                new ColumnHeader() { Name = "pt2.0x0c", Text = "[pt2.0x0c] Extracted size" },
                new ColumnHeader() { Name = "pt2.0x10", Text = "[pt2.0x10] Id" },

                new ColumnHeader() { Name = "pt6.0x00", Text = "[pt5.0x00]" },
                new ColumnHeader() { Name = "pt6.0x01", Text = "[pt5.0x01]" },
                new ColumnHeader() { Name = "pt6.0x02", Text = "[pt5.0x02]" },
                new ColumnHeader() { Name = "pt6.0x13", Text = "[pt5.0x03]" },

                new ColumnHeader() { Name = "pt7.0x00", Text = "[pt6.0x00]" },
                new ColumnHeader() { Name = "pt7.0x04", Text = "[pt6.0x04]" },
            };

            this.itemsListView.ShowItemToolTips = true;
            this.itemsListView.Columns.AddRange(columns);
            this.itemsListView.Columns.AddRange(debugColumns);
            this.itemsListView.ColumnClick += this.ItemsListView_ColumnClick;
        }

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

        private void ItemsListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == this.listViewItemSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (this.listViewItemSorter.Order == SortOrder.Ascending)
                {
                    this.listViewItemSorter.Order = SortOrder.Descending;
                }
                else
                {
                    this.listViewItemSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                this.listViewItemSorter.SortColumn = e.Column;
                this.listViewItemSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.itemsListView.Sort();
        }

        private void ItemsListView_MouseUp(object sender, MouseEventArgs e)
        {
            // Show context menu if an item is right-clicked
            if (e.Button == MouseButtons.Right)
            {
                this.EditorForm.ShowItemContextMenu(Cursor.Position);
            }
        }

        private void ItemsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItems = this.itemsListView.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return;
            }

            // Build a list of selected NefsItems
            var selectedNefsItems = new List<NefsItem>();

            foreach (ListViewItem item in this.itemsListView.SelectedItems)
            {
                selectedNefsItems.Add(item.Tag as NefsItem);
            }

            // Tell the editor what items are selected
            this.Workspace.SelectItems(selectedNefsItems);
        }

        private void LoadArchive(NefsArchive archive)
        {
            // Clear current list
            this.itemsListView.Items.Clear();
            this.listItems.Clear();

            if (archive == null)
            {
                return;
            }

            // Load all items in the NeFS archive into the listview
            foreach (var item in archive.Items)
            {
                var listItem = new ListViewItem();

                // The list item is actually the first column
                listItem.Text = item.Id.Value.ToString("X");

                // Save a reference to the item object
                listItem.Tag = item;

                this.AddSubItem(listItem, "filename", item.FileName);
                this.AddSubItem(listItem, "directoryId", item.DirectoryId.Value.ToString("X"));
                this.AddSubItem(listItem, "compressedSize", item.CompressedSize.ToString("X"));
                this.AddSubItem(listItem, "extractedSize", item.ExtractedSize.ToString("X"));

                var p1 = archive.Header.Part1.EntriesById[item.Id];
                this.AddSubItem(listItem, "pt1.0x00", p1.OffsetToData.Value.ToString("X"));
                this.AddSubItem(listItem, "pt1.0x08", p1.IndexIntoPart2.Value.ToString("X"));
                this.AddSubItem(listItem, "pt1.0x0c", p1.IndexIntoPart4.Value.ToString("X"));
                this.AddSubItem(listItem, "pt1.0x10", p1.Id.Value.ToString("X"));

                var p2 = archive.Header.Part2.EntriesById[item.Id];
                this.AddSubItem(listItem, "pt2.0x00", p2.DirectoryId.Value.ToString("X"));
                this.AddSubItem(listItem, "pt2.0x04", p2.FirstChildId.Value.ToString("X"));
                this.AddSubItem(listItem, "pt2.0x08", p2.OffsetIntoPart3.Value.ToString("X"));
                this.AddSubItem(listItem, "pt2.0x0c", p2.ExtractedSize.Value.ToString("X"));
                this.AddSubItem(listItem, "pt2.0x10", p2.Id.Value.ToString("X"));

                var p6 = archive.Header.Part6.EntriesById[item.Id];
                this.AddSubItem(listItem, "pt6.0x00", p6.Byte0.Value[0].ToString("X"));
                this.AddSubItem(listItem, "pt6.0x01", p6.Byte1.Value[0].ToString("X"));
                this.AddSubItem(listItem, "pt6.0x02", p6.Byte2.Value[0].ToString("X"));
                this.AddSubItem(listItem, "pt6.0x03", p6.Byte3.Value[0].ToString("X"));

                var p7 = archive.Header.Part7.EntriesById[item.Id];
                this.AddSubItem(listItem, "pt7.0x00", p7.SiblingId.Value.ToString("X"));
                this.AddSubItem(listItem, "pt7.0x04", p7.Id.Value.ToString("X"));

                if (item.Type == NefsItemType.Directory)
                {
                    listItem.BackColor = Color.LightBlue;
                }

                this.listItems.Add(item, listItem);
                this.itemsListView.Items.Add(listItem);
            }
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
            foreach (var item in this.listItems)
            {
                this.UpdateListItem(item.Value);
            }
        }

        private void OnWorkspaceCommandExecuted(Object sender, NefsEditCommandEventArgs e)
        {
            if (e.Command is ReplaceFileCommand replaceCommand)
            {
                if (!this.listItems.ContainsKey(replaceCommand.Item))
                {
                    // An item was replaced, but its not in the current view
                    return;
                }

                var listItem = this.listItems[replaceCommand.Item];
                this.UpdateListItem(listItem);
            }
            else if (e.Command is RemoveFileCommand removeCommand)
            {
                if (!this.listItems.ContainsKey(removeCommand.Item))
                {
                    // An item was removed, but its not in the current view
                    return;
                }

                var listItem = this.listItems[removeCommand.Item];
                this.UpdateListItem(listItem);
            }
        }

        /// <summary>
        /// Updates visual appearance of a file list item.
        /// </summary>
        /// <param name="listItem">The item to update.</param>
        private void UpdateListItem(ListViewItem listItem)
        {
            var item = listItem.Tag as NefsItem;
            if (item == null)
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
