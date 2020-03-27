// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Workspace;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.Item;
    using WeifenLuo.WinFormsUI.Docking;

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
            this.InitializeComponent();
            this.Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            this.UiService = uiService;
            this.Workspace.ArchiveOpened += this.OnWorkspaceArchiveOpened;
            this.Workspace.ArchiveClosed += this.OnWorkspaceArchiveClosed;
            this.Workspace.SelectedItemsChanged += this.OnWorkspaceSelectedItemsChanged;
        }

        private IUiService UiService { get; }

        private INefsEditWorkspace Workspace { get; }

        private void ArchiveDebugForm_Load(Object sender, EventArgs e)
        {
        }

        private void OnWorkspaceArchiveClosed(Object sender, EventArgs e)
        {
            // Update on UI thread
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.PrintDebugInfo(null, null);
            });
        }

        private void OnWorkspaceArchiveOpened(Object sender, EventArgs e)
        {
            // Update on UI thread
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.PrintDebugInfo(this.Workspace.SelectedItems.FirstOrDefault(), this.Workspace.Archive);
            });
        }

        private void OnWorkspaceSelectedItemsChanged(Object sender, EventArgs e)
        {
            // Update on UI thread
            this.UiService.Dispatcher.Invoke(() =>
            {
                this.PrintDebugInfo(this.Workspace.SelectedItems.FirstOrDefault(), this.Workspace.Archive);
            });
        }

        private string PrintChunkSizesToString(IList<uint> sizes)
        {
            var sb = new StringBuilder();
            foreach (var s in sizes)
            {
                sb.Append("0x" + s.ToString("X") + ", ");
            }

            return sb.ToString();
        }

        private void PrintDebugInfo(NefsItem item, NefsArchive archive)
        {
            this.richTextBox.Text = "";

            if (item == null || archive == null)
            {
                return;
            }

            this.richTextBox.Text = $@"Item Info
-----------------------------------------------------------
Item name:                  {item.FileName}
Item path:                  {archive.Items.GetItemFilePath(item.Id)}

Part 1
-----------------------------------------------------------
Offset to data:             {archive.Header.Part1.EntriesById[item.Id].OffsetToData.ToString("X")}
Index in part 2:            {archive.Header.Part1.EntriesById[item.Id].MetadataIndex.ToString("X")}
Index in part 4:            {archive.Header.Part1.EntriesById[item.Id].IndexIntoPart4.ToString("X")}
Id:                         {archive.Header.Part1.EntriesById[item.Id].Id.Value.ToString("X")}

Part 2
-----------------------------------------------------------
Directory id:               {archive.Header.Part2.EntriesById[item.Id].DirectoryId.Value.ToString("X")}
First child id:             {archive.Header.Part2.EntriesById[item.Id].FirstChildId.Value.ToString("X")}
Offset in part 3:           {archive.Header.Part2.EntriesById[item.Id].OffsetIntoPart3.ToString("X")}
Extracted size:             {archive.Header.Part2.EntriesById[item.Id].ExtractedSize.ToString("X")}
Id:                         {archive.Header.Part2.EntriesById[item.Id].Id.Value.ToString("X")}

Part 4
-----------------------------------------------------------
Chunks                      {this.PrintChunkSizesToString(archive.Header.Part4.GetChunkSizesForItem(item))}

Part 6
-----------------------------------------------------------
0x00:                       {archive.Header.Part6.EntriesById[item.Id].Byte0.ToString("X")}
0x01:                       {archive.Header.Part6.EntriesById[item.Id].Byte1.ToString("X")}
0x02:                       {archive.Header.Part6.EntriesById[item.Id].Byte2.ToString("X")}
0x03:                       {archive.Header.Part6.EntriesById[item.Id].Byte3.ToString("X")}

Part 7
-----------------------------------------------------------
Sibling id:                 {archive.Header.Part7.EntriesById[item.Id].SiblingId.Value.ToString("X")}
Item id:                    {archive.Header.Part7.EntriesById[item.Id].Id.Value.ToString("X")}
";
        }
    }
}
