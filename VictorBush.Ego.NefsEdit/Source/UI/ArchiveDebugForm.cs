// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using System.Text;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Workspace;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.Header;
    using VictorBush.Ego.NefsLib.Utility;
    using WeifenLuo.WinFormsUI.Docking;

    /// <summary>
    /// Form used for displaying debug information about an archive.
    /// </summary>
    internal partial class ArchiveDebugForm : DockContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveDebugForm"/> class.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="uiService">The UI service.</param>
        public ArchiveDebugForm(INefsEditWorkspace workspace, IUiService uiService)
        {
            this.InitializeComponent();
            this.Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            this.UiService = uiService;
            this.Workspace.ArchiveOpened += this.OnWorkspaceArchiveOpened;
            this.Workspace.ArchiveClosed += this.OnWorkspaceArchiveClosed;
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
                this.PrintDebugInfo(this.Workspace.Archive, this.Workspace.ArchiveSource);
            });
        }

        private void PrintDebugInfo(NefsArchive archive, NefsArchiveSource source)
        {
            this.richTextBox.Text = "";

            if (archive == null)
            {
                return;
            }

            var headerPart1String = new StringBuilder();
            foreach (var entry in archive.Header.Part1.EntriesByIndex)
            {
                headerPart1String.Append($"0x{entry.OffsetToData.ToString("X")}".PadRight(20));
                headerPart1String.Append($"0x{entry.MetadataIndex.ToString("X")}".PadRight(20));
                headerPart1String.Append($"0x{entry.IndexIntoPart4.ToString("X")}".PadRight(20));
                headerPart1String.Append($"0x{entry.Id.Value.ToString("X")}".PadRight(20));
                headerPart1String.Append("\n");
            }

            var headerPart2String = new StringBuilder();
            foreach (var entry in archive.Header.Part2.EntriesByIndex)
            {
                headerPart2String.Append($"0x{entry.DirectoryId.Value.ToString("X")}".PadRight(20));
                headerPart2String.Append($"0x{entry.FirstChildId.Value.ToString("X")}".PadRight(20));
                headerPart2String.Append($"0x{entry.OffsetIntoPart3.ToString("X")}".PadRight(20));
                headerPart2String.Append($"0x{entry.ExtractedSize.ToString("X")}".PadRight(20));
                headerPart2String.Append($"0x{entry.Id.Value.ToString("X")}".PadRight(20));
                headerPart2String.Append("\n");
            }

            var headerPart3String = new StringBuilder();
            foreach (var s in archive.Header.Part3.FileNames)
            {
                headerPart3String.AppendLine(s);
            }

            var headerPart6String = new StringBuilder();
            foreach (var entry in archive.Header.Part6.EntriesByIndex)
            {
                headerPart6String.Append($"0x{entry.Byte0.ToString("X")}".PadRight(20));
                headerPart6String.Append($"0x{entry.Byte1.ToString("X")}".PadRight(20));
                headerPart6String.Append($"0x{entry.Byte2.ToString("X")}".PadRight(20));
                headerPart6String.Append($"0x{entry.Byte3.ToString("X")}".PadRight(20));
                headerPart6String.Append("\n");
            }

            var headerPart7String = new StringBuilder();
            foreach (var entry in archive.Header.Part7.EntriesByIndex)
            {
                headerPart7String.Append($"0x{entry.SiblingId.Value.ToString("X")}".PadRight(20));
                headerPart7String.Append($"0x{entry.Id.Value.ToString("X")}".PadRight(20));
                headerPart7String.Append("\n");
            }

            this.richTextBox.Text = $@"Archive Source
-----------------------------------------------------------
Header source file:         {source.HeaderFilePath}
Header offset:              {source.HeaderOffset.ToString("X")}
Data file path:             {source.DataFilePath}
Is header/data separate:    {source.IsHeaderSeparate}

General Info
-----------------------------------------------------------
Archive Size:               {archive.Header.Part5.ArchiveSize.ToString("X")}
Is Header Encrypted?        {archive.Header.Intro.IsEncrypted}

Header size:                {archive.Header.Intro.HeaderSize.ToString("X")}
Intro size:                 {NefsHeaderIntro.Size.ToString("X")}
Toc size:                   {NefsHeaderIntroToc.Size.ToString("X")}
Part 1 size:                {archive.Header.TableOfContents.Part1Size.ToString("X")}
Part 2 size:                {archive.Header.TableOfContents.Part2Size.ToString("X")}
Part 3 size:                {archive.Header.TableOfContents.Part3Size.ToString("X")}
Part 4 size:                {archive.Header.TableOfContents.Part4Size.ToString("X")}
Part 5 size:                {archive.Header.TableOfContents.Part5Size.ToString("X")}
Part 6 size:                {archive.Header.TableOfContents.Part6Size.ToString("X")}
Part 7 size:                {archive.Header.TableOfContents.Part7Size.ToString("X")}
Part 8 size:                {(archive.Header.Intro.HeaderSize - archive.Header.TableOfContents.OffsetToPart8).ToString("X")}

Header Intro
-----------------------------------------------------------
Magic Number:               {archive.Header.Intro.MagicNumber.ToString("X")}
Expected SHA-256 hash:      {HexHelper.ByteArrayToString(archive.Header.Intro.ExpectedHash)}
AES 256 key hex string:     {HexHelper.ByteArrayToString(archive.Header.Intro.AesKeyHexString)}
Header size:                {archive.Header.Intro.HeaderSize.ToString("X")}
Unknown 0x68:               {archive.Header.Intro.Unknown0x68.ToString("X")}
Number of items:            {archive.Header.Intro.NumberOfItems.ToString("X")}
Unknown 0x70:               {archive.Header.Intro.Unknown0x70zlib.ToString("X")}
Unknown 0x78:               {archive.Header.Intro.Unknown0x78.ToString("X")}

Header Table of Contents
-----------------------------------------------------------
Offset to Part 1:           {archive.Header.TableOfContents.OffsetToPart1.ToString("X")}
Offset to Part 2:           {archive.Header.TableOfContents.OffsetToPart2.ToString("X")}
Offset to Part 3:           {archive.Header.TableOfContents.OffsetToPart3.ToString("X")}
Offset to Part 4:           {archive.Header.TableOfContents.OffsetToPart4.ToString("X")}
Offset to Part 5:           {archive.Header.TableOfContents.OffsetToPart5.ToString("X")}
Offset to Part 6:           {archive.Header.TableOfContents.OffsetToPart6.ToString("X")}
Offset to Part 7:           {archive.Header.TableOfContents.OffsetToPart7.ToString("X")}
Offset to Part 8:           {archive.Header.TableOfContents.OffsetToPart8.ToString("X")}
Unknown 0x00:               {archive.Header.TableOfContents.Unknown0x00.ToString("X")}
Unknown 0x24:               {HexHelper.ByteArrayToString(archive.Header.TableOfContents.Unknown0x24)}

Header Part 1
-----------------------------------------------------------
Data Offset         Metadata index      Index to Part 4     Id
{headerPart1String.ToString()}
Header Part 2
-----------------------------------------------------------
Directory Id        First child Id      Part 3 offset       Extracted size      Id
{headerPart2String.ToString()}
Header Part 3
-----------------------------------------------------------
{headerPart3String.ToString()}
Header Part 4
-----------------------------------------------------------
Number of files:            {archive.Header.Part4.EntriesByIndex.Count.ToString("X")}

Header Part 5
-----------------------------------------------------------
Archive size:               {archive.Header.Part5.ArchiveSize.ToString("X")}
Unknown:                    {archive.Header.Part5.UnknownData.ToString("X")}

Header Part 6
-----------------------------------------------------------
{headerPart6String.ToString()}
Header Part 7
-----------------------------------------------------------
{headerPart7String.ToString()}
";
        }
    }
}
