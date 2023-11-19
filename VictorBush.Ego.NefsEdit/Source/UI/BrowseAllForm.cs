// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsEdit.Commands;
using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.Utility;
using VictorBush.Ego.NefsEdit.Workspace;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Item;
using WeifenLuo.WinFormsUI.Docking;

namespace VictorBush.Ego.NefsEdit.UI;

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
		InitializeComponent();
		EditorForm = editorForm ?? throw new ArgumentNullException(nameof(editorForm));
		UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
		Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
		Workspace.ArchiveOpened += OnWorkspaceArchiveOpened;
		Workspace.ArchiveClosed += OnWorkspaceArchiveClosed;
		Workspace.ArchiveSaved += OnWorkspaceArchiveSaved;
		Workspace.CommandExecuted += OnWorkspaceCommandExecuted;

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
				new ColumnHeader() { Name = "pt1.0x00", Text = "Offset to Data [pt1.0x00]" },
				new ColumnHeader() { Name = "pt1.0x08", Text = "Index Part 2 [pt1.0x08]" },
				new ColumnHeader() { Name = "pt1.0x0c", Text = "Index Part 4 [pt1.0x0c]" },
				new ColumnHeader() { Name = "pt1.0x10", Text = "Id [pt1.0x10]" },

				new ColumnHeader() { Name = "pt2.0x00", Text = "Directory Id [pt2.0x00]" },
				new ColumnHeader() { Name = "pt2.0x04", Text = "First Child [pt2.0x04]" },
				new ColumnHeader() { Name = "pt2.0x08", Text = "Pt3 offset (filename strings) [pt2.0x08]" },
				new ColumnHeader() { Name = "pt2.0x0c", Text = "Extracted size [pt2.0x0c]" },
				new ColumnHeader() { Name = "pt2.0x10", Text = "Id [pt2.0x10]" },

				new ColumnHeader() { Name = "pt6.0x00", Text = "Volume [pt6.0x00]" },
				new ColumnHeader() { Name = "pt6.0x02", Text = "Flags [pt6.0x02]" },
				new ColumnHeader() { Name = "pt6.0x03", Text = "Unknown [pt6.0x03]" },

				new ColumnHeader() { Name = "pt7.0x00", Text = "Sibling Id [pt7.0x00]" },
				new ColumnHeader() { Name = "pt7.0x04", Text = "Id [pt7.0x04]" },
		};

		this.itemsListView.ShowItemToolTips = true;
		this.itemsListView.Columns.AddRange(columns);
		this.itemsListView.Columns.AddRange(debugColumns);
		this.itemsListView.ColumnClick += ItemsListView_ColumnClick;
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

	private void ItemsListView_ColumnClick(object? sender, ColumnClickEventArgs e)
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
			EditorForm.ShowItemContextMenu(Cursor.Position);
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
			selectedNefsItems.Add((NefsItem)item.Tag);
		}

		// Tell the editor what items are selected
		Workspace.SelectItems(selectedNefsItems);
	}

	private void LoadArchive(NefsArchive? archive)
	{
		// Clear current list
		this.itemsListView.Items.Clear();
		this.listItems.Clear();

		if (archive == null)
		{
			return;
		}

		// Load all items in the NeFS archive into the listview
		foreach (var item in archive.Items.EnumerateById())
		{
			var listItem = new ListViewItem();

			// The list item is actually the first column
			listItem.Text = item.Id.Value.ToString("X");

			// Save a reference to the item object
			listItem.Tag = item;

			AddSubItem(listItem, "filename", item.FileName);
			AddSubItem(listItem, "directoryId", item.DirectoryId.Value.ToString("X"));
			AddSubItem(listItem, "compressedSize", item.CompressedSize.ToString("X"));
			AddSubItem(listItem, "extractedSize", item.ExtractedSize.ToString("X"));

			if (archive.Header is Nefs20Header h20)
			{
				var p1 = h20.Part1.EntriesByGuid[item.Guid];
				AddSubItem(listItem, "pt1.0x00", p1.OffsetToData.ToString("X"));
				AddSubItem(listItem, "pt1.0x08", p1.IndexPart2.ToString("X"));
				AddSubItem(listItem, "pt1.0x0c", p1.IndexPart4.ToString("X"));
				AddSubItem(listItem, "pt1.0x10", p1.Id.Value.ToString("X"));

				var p2 = h20.Part2.EntriesByIndex[(int)p1.IndexPart2];
				AddSubItem(listItem, "pt2.0x00", p2.DirectoryId.Value.ToString("X"));
				AddSubItem(listItem, "pt2.0x04", p2.FirstChildId.Value.ToString("X"));
				AddSubItem(listItem, "pt2.0x08", p2.OffsetIntoPart3.ToString("X"));
				AddSubItem(listItem, "pt2.0x0c", p2.ExtractedSize.ToString("X"));
				AddSubItem(listItem, "pt2.0x10", p2.Id.Value.ToString("X"));

				var p6 = h20.Part6.EntriesByGuid[item.Guid];
				AddSubItem(listItem, "pt6.0x00", p6.Volume.ToString("X"));
				AddSubItem(listItem, "pt6.0x02", ((byte)p6.Flags).ToString("X"));
				AddSubItem(listItem, "pt6.0x03", p6.Unknown0x3.ToString("X"));

				var p7 = h20.Part7.EntriesByIndex[(int)p1.IndexPart2];
				AddSubItem(listItem, "pt7.0x00", p7.SiblingId.Value.ToString("X"));
				AddSubItem(listItem, "pt7.0x04", p7.Id.Value.ToString("X"));
			}
			else if (archive.Header is Nefs16Header h16)
			{
				var p1 = h16.Part1.EntriesByGuid[item.Guid];
				AddSubItem(listItem, "pt1.0x00", p1.OffsetToData.ToString("X"));
				AddSubItem(listItem, "pt1.0x08", p1.IndexPart2.ToString("X"));
				AddSubItem(listItem, "pt1.0x0c", p1.IndexPart4.ToString("X"));
				AddSubItem(listItem, "pt1.0x10", p1.Id.Value.ToString("X"));

				var p2 = h16.Part2.EntriesByIndex[(int)p1.IndexPart2];
				AddSubItem(listItem, "pt2.0x00", p2.DirectoryId.Value.ToString("X"));
				AddSubItem(listItem, "pt2.0x04", p2.FirstChildId.Value.ToString("X"));
				AddSubItem(listItem, "pt2.0x08", p2.OffsetIntoPart3.ToString("X"));
				AddSubItem(listItem, "pt2.0x0c", p2.ExtractedSize.ToString("X"));
				AddSubItem(listItem, "pt2.0x10", p2.Id.Value.ToString("X"));

				var p6 = h16.Part6.EntriesByGuid[item.Guid];
				AddSubItem(listItem, "pt6.0x00", p6.Volume.ToString("X"));
				AddSubItem(listItem, "pt6.0x02", ((byte)p6.Flags).ToString("X"));
				AddSubItem(listItem, "pt6.0x03", p6.Unknown0x3.ToString("X"));

				var p7 = h16.Part7.EntriesByIndex[(int)p1.IndexPart2];
				AddSubItem(listItem, "pt7.0x00", p7.SiblingId.Value.ToString("X"));
				AddSubItem(listItem, "pt7.0x04", p7.Id.Value.ToString("X"));
			}

			if (item.Type == NefsItemType.Directory)
			{
				listItem.BackColor = Color.LightBlue;
			}

			if (item.Attributes.IsDuplicated)
			{
				listItem.BackColor = Color.LightPink;
			}

			this.listItems.Add(item, listItem);
			this.itemsListView.Items.Add(listItem);
		}
	}

	private void OnWorkspaceArchiveClosed(object? sender, EventArgs e)
	{
		// Clear items list - must do on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			LoadArchive(null);
		});
	}

	private void OnWorkspaceArchiveOpened(object? sender, EventArgs e)
	{
		// Update items list - must do on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			LoadArchive(Workspace.Archive);
		});
	}

	private void OnWorkspaceArchiveSaved(object? sender, EventArgs e)
	{
		UiService.Dispatcher.Invoke(() =>
		{
			LoadArchive(Workspace.Archive);
		});
	}

	private void OnWorkspaceCommandExecuted(object? sender, NefsEditCommandEventArgs e)
	{
		if (e.Command is ReplaceFileCommand replaceCommand)
		{
			if (!this.listItems.ContainsKey(replaceCommand.Item))
			{
				// An item was replaced, but its not in the current view
				return;
			}

			var listItem = this.listItems[replaceCommand.Item];
			UpdateListItem(listItem);
		}
		else if (e.Command is RemoveFileCommand removeCommand)
		{
			if (!this.listItems.ContainsKey(removeCommand.Item))
			{
				// An item was removed, but its not in the current view
				return;
			}

			var listItem = this.listItems[removeCommand.Item];
			UpdateListItem(listItem);
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
