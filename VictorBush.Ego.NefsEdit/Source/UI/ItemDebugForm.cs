// See LICENSE.txt for license information.

using System.Text;
using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.Workspace;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Item;
using WeifenLuo.WinFormsUI.Docking;

namespace VictorBush.Ego.NefsEdit.UI;

/// <summary>
/// Form used for displaying debug information about an archive.
/// </summary>
internal partial class ItemDebugForm : DockContent
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ItemDebugForm"/> class.
	/// </summary>
	/// <param name="workspace">The workspace.</param>
	/// <param name="uiService">The UI service.</param>
	public ItemDebugForm(INefsEditWorkspace workspace, IUiService uiService)
	{
		InitializeComponent();
		Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
		UiService = uiService;
		Workspace.ArchiveOpened += OnWorkspaceArchiveOpened;
		Workspace.ArchiveClosed += OnWorkspaceArchiveClosed;
		Workspace.SelectedItemsChanged += OnWorkspaceSelectedItemsChanged;
	}

	private IUiService UiService { get; }

	private INefsEditWorkspace Workspace { get; }

	private void ArchiveDebugForm_Load(Object sender, EventArgs e)
	{
	}

	private string GetDebugInfoVersion16(NefsItem item, NefsHeader160 h, NefsItemList items)
	{
		var p1 = h.EntryTable.Entries[item.Id.Index];
		var p2 = h.SharedEntryInfoTable.Entries[(int)p1.SharedInfo];
		var p6 = h.WriteableEntryTable.Entries[item.Id.Index];
		var p7 = h.WriteableSharedEntryInfoTable.Entries[(int)p1.SharedInfo];
		var attributes = item.Attributes;

		return $"""
		        Item Info
		        -----------------------------------------------------------
		        Item name:                  {item.FileName}
		        Item path:                  {items.GetItemFilePath(item.Id)}

		        Part 1
		        -----------------------------------------------------------
		        Offset to data:             {p1.Start.ToString("X")}
		        Index in part 2:            {p1.SharedInfo.ToString("X")}
		        Index in part 4:            {p1.FirstBlock.ToString("X")}
		        Next duplicate:             {p1.NextDuplicate.ToString("X")}

		        Part 2
		        -----------------------------------------------------------
		        Directory id:               {p2.Parent.ToString("X")}
		        First child id:             {p2.FirstChild.ToString("X")}
		        Offset in part 3:           {p2.NameOffset.ToString("X")}
		        Extracted size:             {p2.Size.ToString("X")}
		        First duplicate:            {p2.FirstDuplicate.ToString("X")}

		        Part 4
		        -----------------------------------------------------------
		        {PrintChunkSizesToString(item.DataSource.Size.Chunks)}

		        Part 6
		        -----------------------------------------------------------
		        0x00:                       {p6.Volume.ToString("X")}
		        0x02:                       {p6.Flags.ToString("X")}

		        IsTransformed:              {attributes.V16IsTransformed}
		        IsDirectory:                {attributes.IsDirectory}
		        IsDuplicated:               {attributes.IsDuplicated}
		        IsCacheable:                {attributes.IsCacheable}
		        IsLastSibling:              {attributes.IsLastSibling}
		        IsPatched:                  {attributes.IsPatched}

		        Part 7
		        -----------------------------------------------------------
		        Sibling id:                 {p7.NextSibling.ToString("X")}
		        Item id:                    {p7.PatchedEntry.ToString("X")}
		        """;
	}

	private string GetDebugInfoVersion20(NefsItem item, NefsHeader200 h, NefsItemList items)
	{
		var p1 = h.EntryTable.Entries[item.Id.Index];
		var p2 = h.SharedEntryInfoTable.Entries[(int)p1.SharedInfo];
		var p6 = h.WriteableEntryTable.Entries[item.Id.Index];
		var p7 = h.WriteableSharedEntryInfoTable.Entries[(int)p1.SharedInfo];
		var attributes = item.Attributes;

		return $"""
		        Item Info
		        -----------------------------------------------------------
		        Item name:                  {item.FileName}
		        Item path:                  {items.GetItemFilePath(item.Id)}

		        Part 1
		        -----------------------------------------------------------
		        Offset to data:             {p1.Start.ToString("X")}
		        Index in part 2:            {p1.SharedInfo.ToString("X")}
		        Index in part 4:            {p1.FirstBlock.ToString("X")}
		        Next duplicate:             {p1.NextDuplicate.ToString("X")}

		        Part 2
		        -----------------------------------------------------------
		        Directory id:               {p2.Parent.ToString("X")}
		        First child id:             {p2.FirstChild.ToString("X")}
		        Offset in part 3:           {p2.NameOffset.ToString("X")}
		        Extracted size:             {p2.Size.ToString("X")}
		        First duplicate:            {p2.FirstDuplicate.ToString("X")}

		        Part 4
		        -----------------------------------------------------------
		        Chunks                      {(item.Transform is not null ? PrintChunkSizesToString(item.DataSource.Size.Chunks) : "Item has no transform.")}

		        Part 6
		        -----------------------------------------------------------
		        0x00:                       {p6.Volume.ToString("X")}
		        0x02:                       {p6.Flags.ToString("X")}

		        IsZlib:                     {attributes.V20IsZlib}
		        IsAes:                      {attributes.V20IsAes}
		        IsDirectory:                {attributes.IsDirectory}
		        IsDuplicated:               {attributes.IsDuplicated}
		        IsLastSibling:              {attributes.IsLastSibling}

		        Part 7
		        -----------------------------------------------------------
		        Sibling id:                 {p7.NextSibling.ToString("X")}
		        Item id:                    {p7.PatchedEntry.ToString("X")}
		        """;
	}

	private void OnWorkspaceArchiveClosed(object? sender, EventArgs e)
	{
		// Update on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			PrintDebugInfo(null, null);
		});
	}

	private void OnWorkspaceArchiveOpened(object? sender, EventArgs e)
	{
		// Update on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			PrintDebugInfo(Workspace.SelectedItems.FirstOrDefault(), Workspace.Archive);
		});
	}

	private void OnWorkspaceSelectedItemsChanged(object? sender, EventArgs e)
	{
		// Update on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			PrintDebugInfo(Workspace.SelectedItems.FirstOrDefault(), Workspace.Archive);
		});
	}

	private string PrintChunkSizesToString(IReadOnlyList<NefsDataChunk> sizes)
	{
		var sb = new StringBuilder();
		foreach (var s in sizes)
		{
			sb.Append("0x" + s.CumulativeSize.ToString("X") /*+ $" [{s.Checksum.ToString("X")}] */ + "\n");
		}

		return sb.ToString();
	}

	private void PrintDebugInfo(NefsItem? item, NefsArchive? archive)
	{
		this.richTextBox.Text = "";

		if (item == null || archive == null)
		{
			return;
		}

		if (archive.Header is NefsHeader200 h20)
		{
			this.richTextBox.Text = GetDebugInfoVersion20(item, h20, archive.Items);
		}
		else if (archive.Header is NefsHeader160 h16)
		{
			this.richTextBox.Text = GetDebugInfoVersion16(item, h16, archive.Items);
		}
		else
		{
			this.richTextBox.Text = "Unknown header version.";
		}
	}
}
