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
    using VictorBush.Ego.NefsLib.DataSource;
    using VictorBush.Ego.NefsLib.Header;
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

        private string GetDebugInfoVersion16(NefsItem item, Nefs16Header h, NefsItemList items)
        {
            var p1 = h.Part1.EntriesByGuid[item.Guid];
            var p2 = h.Part2.EntriesByIndex[(int)p1.IndexPart2];
            var p6 = h.Part6.EntriesByGuid[item.Guid];
            var p7 = h.Part7.EntriesByIndex[(int)p1.IndexPart2];
            var numChunks = h.TableOfContents.ComputeNumChunks(p2.ExtractedSize);
            var chunkSize = h.TableOfContents.BlockSize;
            var attributes = p6.CreateAttributes();

            return $@"Item Info
-----------------------------------------------------------
Item name:                  {item.FileName}
Item path:                  {items.GetItemFilePath(item.Id)}

Part 1
-----------------------------------------------------------
Offset to data:             {p1.OffsetToData.ToString("X")}
Index in part 2:            {p1.IndexPart2.ToString("X")}
Index in part 4:            {p1.IndexPart4.ToString("X")}
Id:                         {p1.Id.Value.ToString("X")}

Part 2
-----------------------------------------------------------
Directory id:               {p2.DirectoryId.Value.ToString("X")}
First child id:             {p2.FirstChildId.Value.ToString("X")}
Offset in part 3:           {p2.OffsetIntoPart3.ToString("X")}
Extracted size:             {p2.ExtractedSize.ToString("X")}
Id:                         {p2.Id.Value.ToString("X")}

Part 4
-----------------------------------------------------------
{this.PrintChunkSizesToString(h.Part4.CreateChunksList(p1.IndexPart4, numChunks, chunkSize, h.Intro.GetAesKey()))}

Part 6
-----------------------------------------------------------
0x00:                       {p6.Volume.ToString("X")}
0x02:                       {((byte)p6.Flags).ToString("X")}
0x03:                       {p6.Unknown0x3.ToString("X")}

IsTransformed:              {attributes.V16IsTransformed}
IsDirectory:                {attributes.IsDirectory}
IsDuplicated:               {attributes.IsDuplicated}
IsCacheable:                {attributes.IsCacheable}
Unknown 0x10:               {attributes.V16Unknown0x10}
IsPatched:                  {attributes.IsPatched}
Unknown 0x40:               {attributes.V16Unknown0x40}
Unknown 0x80:               {attributes.V16Unknown0x80}

Part 7
-----------------------------------------------------------
Sibling id:                 {p7.SiblingId.Value.ToString("X")}
Item id:                    {p7.Id.Value.ToString("X")}
";
        }

        private string GetDebugInfoVersion20(NefsItem item, Nefs20Header h, NefsItemList items)
        {
            var p1 = h.Part1.EntriesByGuid[item.Guid];
            var p2 = h.Part2.EntriesByIndex[(int)p1.IndexPart2];
            var p6 = h.Part6.EntriesByGuid[item.Guid];
            var p7 = h.Part7.EntriesByIndex[(int)p1.IndexPart2];
            var numChunks = h.TableOfContents.ComputeNumChunks(p2.ExtractedSize);
            var attributes = p6.CreateAttributes();

            return $@"Item Info
-----------------------------------------------------------
Item name:                  {item.FileName}
Item path:                  {items.GetItemFilePath(item.Id)}

Part 1
-----------------------------------------------------------
Offset to data:             {p1.OffsetToData.ToString("X")}
Index in part 2:            {p1.IndexPart2.ToString("X")}
Index in part 4:            {p1.IndexPart4.ToString("X")}
Id:                         {p1.Id.Value.ToString("X")}

Part 2
-----------------------------------------------------------
Directory id:               {p2.DirectoryId.Value.ToString("X")}
First child id:             {p2.FirstChildId.Value.ToString("X")}
Offset in part 3:           {p2.OffsetIntoPart3.ToString("X")}
Extracted size:             {p2.ExtractedSize.ToString("X")}
Id:                         {p2.Id.Value.ToString("X")}

Part 4
-----------------------------------------------------------
Chunks                      {this.PrintChunkSizesToString(h.Part4.CreateChunksList(p1.IndexPart4, numChunks, item.Transform))}

Part 6
-----------------------------------------------------------
0x00:                       {p6.Volume.ToString("X")}
0x02:                       {((byte)p6.Flags).ToString("X")}
0x03:                       {p6.Unknown0x3.ToString("X")}

IsZlib:                     {attributes.V20IsZlib}
IsAes:                      {attributes.V20IsAes}
IsDirectory:                {attributes.IsDirectory}
IsDuplicated:               {attributes.IsDuplicated}
Unknown 0x10:               {attributes.V20Unknown0x10}
Unknown 0x20:               {attributes.V20Unknown0x20}
Unknown 0x40:               {attributes.V20Unknown0x40}
Unknown 0x80:               {attributes.V20Unknown0x80}

Part 7
-----------------------------------------------------------
Sibling id:                 {p7.SiblingId.Value.ToString("X")}
Item id:                    {p7.Id.Value.ToString("X")}
";
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

        private string PrintChunkSizesToString(IList<NefsDataChunk> sizes)
        {
            var sb = new StringBuilder();
            foreach (var s in sizes)
            {
                sb.Append("0x" + s.CumulativeSize.ToString("X") + $" [{s.Checksum.ToString("X")}] \n");
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

            if (archive.Header is Nefs20Header h20)
            {
                this.richTextBox.Text = this.GetDebugInfoVersion20(item, h20, archive.Items);
            }
            else if (archive.Header is Nefs16Header h16)
            {
                this.richTextBox.Text = this.GetDebugInfoVersion16(item, h16, archive.Items);
            }
            else
            {
                this.richTextBox.Text = "Unknown header version.";
            }
        }
    }
}
