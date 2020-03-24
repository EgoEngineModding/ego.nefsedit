// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsEdit.Workspace;
    using VictorBush.Ego.NefsLib;
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

            this.richTextBox.Text = $@"Archive Source
-----------------------------------------------------------
Header source file:         {source.HeaderFilePath}
Header offset:              {source.HeaderOffset}
Data file path:             {source.DataFilePath}
Is header/data separate:    {source.IsHeaderSeparate}

General Info
-----------------------------------------------------------
Archive Size:               {archive.Header.Part5.ArchiveSize}
Is Header Encrypted?        {archive.Header.Intro.IsEncrypted}

Header Intro
-----------------------------------------------------------
Magic Number:               {archive.Header.Intro.MagicNumber}
Expected SHA-256 hash:      {archive.Header.Intro.ExpectedHash}
AES 256 key hex string:     {archive.Header.Intro.AesKeyHexString}
Header size:                {archive.Header.Intro.HeaderSize}
Unknown 0x68:               {archive.Header.Intro.Unknown0x68}
Number of items:            {archive.Header.Intro.NumberOfItems}
Unknown 0x70:               {archive.Header.Intro.Unknown0x70zlib}
Unknown 0x78:               {archive.Header.Intro.Unknown0x78}

Header Table of Contents
-----------------------------------------------------------
Offset to Part 1:           {archive.Header.TableOfContents.OffsetToPart1}
Offset to Part 2:           {archive.Header.TableOfContents.OffsetToPart2}
Offset to Part 3:           {archive.Header.TableOfContents.OffsetToPart3}
Offset to Part 4:           {archive.Header.TableOfContents.OffsetToPart4}
Offset to Part 5:           {archive.Header.TableOfContents.OffsetToPart5}
Offset to Part 6:           {archive.Header.TableOfContents.OffsetToPart6}
Offset to Part 7:           {archive.Header.TableOfContents.OffsetToPart7}
Offset to Part 8:           {archive.Header.TableOfContents.OffsetToPart8}
Unknown 0x00:               {archive.Header.TableOfContents.Unknown0x00}
Unknown 0x24:               {archive.Header.TableOfContents.Unknown0x24}
";
        }
    }
}
