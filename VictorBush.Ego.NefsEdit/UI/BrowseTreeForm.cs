using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VictorBush.Ego.NefsEdit.Utility;
using VictorBush.Ego.NefsLib;
using WeifenLuo.WinFormsUI.Docking;

namespace VictorBush.Ego.NefsEdit.UI
{
    public partial class BrowseTreeForm : DockContent
    {
        NefsArchive _archive;
        NefsItem _dir;
        EditorForm _editor;

        public BrowseTreeForm(EditorForm editor)
        {
            InitializeComponent();

            _editor = editor;

            // Create the columns we want
            var columns = new ColumnHeader[]
            {
                new ColumnHeader() { Name = "id", Text = "Id" },
                new ColumnHeader() { Name = "filename", Text = "Name", Width = 200 },
                new ColumnHeader() { Name = "compressedSize", Text = "Compressed Size" },
                new ColumnHeader() { Name = "extractedSize", Text = "Extracted Size" },

            };

            filesListView.Columns.AddRange(columns);
        }

        public void LoadArchive(NefsArchive archive)
        {
            if (archive == null)
            {
                return;
            }

            _archive = archive;
            directoryTreeView.Nodes.Clear();

            // TODO : Change the root node to the name of the archive?
            var root = directoryTreeView.Nodes.Add("root");

            foreach (var item in archive.Items)
            {
                if (item.Type == NefsItem.NefsItemType.Directory)
                {
                    if (item.Id == item.DirectoryId)
                    {
                        /* This directory is at the root level */
                        var newNode = root.Nodes.Add(item.Filename);
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

                        var newNode = parent.Nodes.Add(item.Filename);
                        newNode.Tag = item;
                    }
                }
            }

            root.Expand();

            OpenDirectory(null);
        }


        // Use null for root directory
        public void OpenDirectory(NefsItem dir)
        {
            List<NefsItem> itemsInDir;

            _dir = dir;

            if (dir == null)
            {
                /* Display contents of root */
                itemsInDir = (from item in _archive.Items
                              where item.Id == item.DirectoryId
                              select item).ToList();

                /* Clear the directory contents list view */
                filesListView.Items.Clear();

                /* This is the root of the archive */
                pathLabel.Text = @"\";
            }
            else
            {
                if (dir.Type != NefsItem.NefsItemType.Directory)
                {
                    // TODO : FIX
                    MessageBox.Show("TODO: Log this --- can't browse a file.");
                }

                /* Display contents of specified directory */
                itemsInDir = (from item in _archive.Items
                              where item.DirectoryId == dir.Id && item.DirectoryId != item.Id
                              select item).ToList();

                pathLabel.Text = @"\" + dir.FilePathInArchive;
            }

            /* Clear the directory contents list view */
            filesListView.Items.Clear();

            // Load all items in the NeFS archive into the listview
            foreach (var item in itemsInDir)
            {
                var listItem = new ListViewItem();

                // The list item is actually the first column
                listItem.Text = item.Id.ToString("X");

                // Save a reference to the item object
                listItem.Tag = item;

                addSubItem(listItem, "filename", item.Filename);

                if (item.Type == NefsItem.NefsItemType.File)
                {
                    addSubItem(listItem, "compressedSize", item.CompressedSize.ToString("X"));
                    addSubItem(listItem, "extractedSize", item.ExtractedSize.ToString("X"));
                }

                if (item.Type == NefsItem.NefsItemType.Directory)
                {
                    listItem.BackColor = Color.LightBlue;
                }

                filesListView.Items.Add(listItem);
            }
        }

        private void addSubItem(ListViewItem item, string name, string text)
        {
            item.SubItems.Add(new ListViewItem.ListViewSubItem()
            {
                Name = name,
                Text = text
            });
        }

        private void directoryTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                OpenDirectory((NefsItem)e.Node.Tag);
            }
        }

        private void filesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItems = filesListView.SelectedItems;

            if (selectedItems.Count == 0)
            {
                return;
            }

            /* Build a list of selected NefsItems */
            var selectedNefsItems = new List<NefsItem>();

            foreach (ListViewItem item in filesListView.SelectedItems)
            {
                selectedNefsItems.Add(item.Tag as NefsItem);
            }

            /* Tell the editor what items are selected */
            _editor.SelectNefsItem(selectedNefsItems);
        }

        private void filesListView_DoubleClick(object sender, EventArgs e)
        {
            if (filesListView.SelectedItems.Count > 0)
            {
                var item = filesListView.SelectedItems[0].Tag as NefsItem;

                if (item.Type == NefsItem.NefsItemType.Directory)
                {
                    OpenDirectory(item);
                }
            }
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            if (_dir == null)
            {
                /* Can't go up a directory */
                return;
            }

            /* Find the parent directory */
            var parent = _archive.GetItem(_dir.DirectoryId);

            if (parent == _dir)
            {
                /* If the parent == the current dir, then display root */
                OpenDirectory(null);
            }
            else
            {
                OpenDirectory(parent);
            }
        }

        private void filesListView_MouseUp(object sender, MouseEventArgs e)
        {
            /* Show context menu if an item is right-clicked */
            if (e.Button == MouseButtons.Right)
            {
                _editor.ShowItemContextMenu(Cursor.Position);
            }
        }
    }
}
