using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VictorBush.Ego.NefsLib;
using WeifenLuo.WinFormsUI.Docking;

namespace VictorBush.Ego.NefsEdit.UI
{
    public partial class BrowseAllForm : DockContent
    {
        EditorForm _editor;

        public BrowseAllForm(EditorForm editor)
        {
            InitializeComponent();

            _editor = editor;

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
                new ColumnHeader() { Name = "pt1.0x08", Text = "[pt1.0x08] Offset into pt2" },
                new ColumnHeader() { Name = "pt1.0x0c", Text = "[pt1.0x0c] Offset into pt4 (chunk sizes)" },
                new ColumnHeader() { Name = "pt1.0x10", Text = "[pt1.0x10] Id" },

                new ColumnHeader() { Name = "pt2.0x00", Text = "[pt2.0x00] Directory Id" },
                new ColumnHeader() { Name = "pt2.0x04", Text = "[pt2.0x04] First Child" },
                new ColumnHeader() { Name = "pt2.0x08", Text = "[pt2.0x08] Offset into pt3 (filename strings)" },
                new ColumnHeader() { Name = "pt2.0x0c", Text = "[pt2.0x0c] Extracted size" },
                new ColumnHeader() { Name = "pt2.0x10", Text = "[pt2.0x10] Id" },


                new ColumnHeader() { Name = "pt5.0x00", Text = "[pt5.0x00]" },
                new ColumnHeader() { Name = "pt5.0x01", Text = "[pt5.0x01]" },
                new ColumnHeader() { Name = "pt5.0x02", Text = "[pt5.0x02]" },
                new ColumnHeader() { Name = "pt5.0x13", Text = "[pt5.0x03]" },

                new ColumnHeader() { Name = "pt6.0x00", Text = "[pt6.0x00]" },
                new ColumnHeader() { Name = "pt6.0x04", Text = "[pt6.0x04]" },
            };

            itemsListView.Columns.AddRange(columns);
            itemsListView.Columns.AddRange(debugColumns);
        }

        public void LoadArchive(NefsArchive archive)
        {
            if (archive == null)
            {
                return;
            }

            // Clear current list
            itemsListView.Items.Clear();

            // Load all items in the NeFS archive into the listview
            foreach (var item in archive.Items)
            {
                ListViewItem listItem = new ListViewItem();

                // The list item is actually the first column
                listItem.Text = item.Id.ToString("X");
                
                // Save a reference to the item object
                listItem.Tag = item;

                addSubItem(listItem, "filename", item.Filename);
                addSubItem(listItem, "directoryId", item.DirectoryId.ToString("X"));
                addSubItem(listItem, "compressedSize", item.CompressedSize.ToString("X"));
                addSubItem(listItem, "extractedSize", item.ExtractedSize.ToString("X"));

                addSubItem(listItem, "pt1.0x00", item.Part1Entry.OffsetToData.ToString("X"));
                addSubItem(listItem, "pt1.0x08", item.Part1Entry.OffsetIntoPt2Raw.ToString("X"));
                addSubItem(listItem, "pt1.0x0c", item.Part1Entry.OffsetIntoPt4Raw.ToString("X"));
                addSubItem(listItem, "pt1.0x10", item.Part1Entry.Id.ToString("X"));

                addSubItem(listItem, "pt2.0x00", item.Part2Entry.DirectoryId.ToString("X"));
                addSubItem(listItem, "pt2.0x04", item.Part2Entry.FirstChildId.ToString("X"));
                addSubItem(listItem, "pt2.0x08", item.Part2Entry.FilenameOffset.ToString("X"));
                addSubItem(listItem, "pt2.0x0c", item.Part2Entry.ExtractedSize.ToString("X"));
                addSubItem(listItem, "pt2.0x10", item.Part2Entry.Id.ToString("X"));

                addSubItem(listItem, "pt5.0x00", item.Part5Entry.Byte1.ToString("X"));
                addSubItem(listItem, "pt5.0x01", item.Part5Entry.Byte2.ToString("X"));
                addSubItem(listItem, "pt5.0x02", item.Part5Entry.Byte3.ToString("X"));
                addSubItem(listItem, "pt5.0x03", item.Part5Entry.Byte4.ToString("X"));

                addSubItem(listItem, "pt6.0x00", item.Part6Entry.Off_0x00.ToString("X"));
                addSubItem(listItem, "pt6.0x04", item.Part6Entry.Off_0x04.ToString("X"));

                if (item.Type == NefsItem.NefsItemType.Directory)
                {
                    listItem.BackColor = Color.LightBlue;
                }

                itemsListView.Items.Add(listItem);
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

        private void itemsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItems = itemsListView.SelectedItems;

            if (selectedItems.Count == 0)
            {
                return;
            }

            /* Build a list of selected NefsItems */
            var selectedNefsItems = new List<NefsItem>();

            foreach (ListViewItem item in itemsListView.SelectedItems)
            {
                selectedNefsItems.Add(item.Tag as NefsItem);
            }

            /* Tell the editor what items are selected */
            _editor.SelectNefsItem(selectedNefsItems);
        }

        private void itemsListView_MouseUp(object sender, MouseEventArgs e)
        {
            /* Show context menu if an item is right-clicked */
            if (e.Button == MouseButtons.Right)
            {
                _editor.ShowItemContextMenu(Cursor.Position);
            }
        }
    }
}
