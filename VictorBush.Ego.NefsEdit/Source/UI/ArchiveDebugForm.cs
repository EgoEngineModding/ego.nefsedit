// See LICENSE.txt for license information.

using System.Text;
using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.Workspace;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Utility;
using WeifenLuo.WinFormsUI.Docking;

namespace VictorBush.Ego.NefsEdit.UI;

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
		InitializeComponent();
		Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
		UiService = uiService;
		Workspace.ArchiveOpened += OnWorkspaceArchiveOpened;
		Workspace.ArchiveClosed += OnWorkspaceArchiveClosed;
		Workspace.ArchiveSaved += OnWorkspaceArchiveSaved;
	}

	private IUiService UiService { get; }

	private INefsEditWorkspace Workspace { get; }

	private void ArchiveDebugForm_Load(Object sender, EventArgs e)
	{
	}

	private string GetArchiveSourceInfo(NefsArchiveSource source)
	{
		switch (source)
		{
			case StandardSource standardSource:
				return $@"Standard NeFS archive.
Archive file:               {standardSource.FilePath}
Header offset:              0";

			case NefsInjectSource nefsInjectSource:
				return $@"NefsInject archive.
Data file:                  {nefsInjectSource.DataFilePath}
NefsInject file:            {nefsInjectSource.NefsInjectFilePath}";

			case HeadlessSource gameDatSource:
				return $@"GameDat archive.
Data file:                  {gameDatSource.DataFilePath}
Header file:                {gameDatSource.HeaderFilePath}
Primary offset:             {gameDatSource.PrimaryOffset.ToString("X")}
Primary size:               {gameDatSource.PrimarySize?.ToString("X")}
Secondary offset:           {gameDatSource.SecondaryOffset.ToString("X")}
Secondary size:             {gameDatSource.SecondarySize?.ToString("X")}";

			default:
				return "Unknown archive source.";
		}
	}

	private string GetDebugInfoVersion16(Nefs16Header h, NefsArchiveSource source)
	{
		var headerPart1String = new StringBuilder();
		foreach (var entry in h.Part1.EntriesByIndex)
		{
			headerPart1String.Append($"0x{entry.OffsetToData.ToString("X")}".PadRight(20));
			headerPart1String.Append($"0x{entry.IndexPart2.ToString("X")}".PadRight(20));
			headerPart1String.Append($"0x{entry.IndexPart4.ToString("X")}".PadRight(20));
			headerPart1String.Append($"0x{entry.Id.Value.ToString("X")}".PadRight(20));
			headerPart1String.Append("\n");
		}

		var headerPart2String = new StringBuilder();
		foreach (var entry in h.Part2.EntriesByIndex)
		{
			headerPart2String.Append($"0x{entry.DirectoryId.Value.ToString("X")}".PadRight(20));
			headerPart2String.Append($"0x{entry.FirstChildId.Value.ToString("X")}".PadRight(20));
			headerPart2String.Append($"0x{entry.OffsetIntoPart3.ToString("X")}".PadRight(20));
			headerPart2String.Append($"0x{entry.ExtractedSize.ToString("X")}".PadRight(20));
			headerPart2String.Append($"0x{entry.Id.Value.ToString("X")}".PadRight(20));
			headerPart2String.Append("\n");
		}

		var headerPart3String = new StringBuilder();
		foreach (var s in h.Part3.FileNames)
		{
			headerPart3String.AppendLine(s);
		}

		var headerPart6String = new StringBuilder();
		foreach (var entry in h.Part6.EntriesByIndex)
		{
			headerPart6String.Append($"0x{entry.Volume.ToString("X")}".PadRight(20));
			headerPart6String.Append($"0x{((byte)entry.Flags).ToString("X")}".PadRight(20));
			headerPart6String.Append($"0x{entry.Unknown0x3.ToString("X")}".PadRight(20));
			headerPart6String.Append("\n");
		}

		var headerPart7String = new StringBuilder();
		foreach (var entry in h.Part7.EntriesByIndex)
		{
			headerPart7String.Append($"0x{entry.SiblingId.Value.ToString("X")}".PadRight(20));
			headerPart7String.Append($"0x{entry.Id.Value.ToString("X")}".PadRight(20));
			headerPart7String.Append("\n");
		}

		var headerPart8String = new StringBuilder();
		foreach (var hash in h.Part8.FileDataHashes)
		{
			headerPart8String.Append(hash);
			headerPart8String.Append("\n");
		}

		return $@"Archive Source
-----------------------------------------------------------
{GetArchiveSourceInfo(source)}

General Info
-----------------------------------------------------------
Archive Size:               {h.Part5.DataSize.ToString("X")}
Is Header Encrypted?        {h.Intro.IsEncrypted}

Header size:                {h.Intro.HeaderSize.ToString("X")}
Intro size:                 {NefsHeaderIntro.Size.ToString("X")}
Toc size:                   {Nefs16HeaderIntroToc.Size.ToString("X")}
Part 1 size:                {h.TableOfContents.Part1Size.ToString("X")}
Part 2 size:                {h.TableOfContents.Part2Size.ToString("X")}
Part 3 size:                {h.TableOfContents.Part3Size.ToString("X")}
Part 4 size:                {h.TableOfContents.Part4Size.ToString("X")}
Part 8 size:                {(h.Intro.HeaderSize - h.TableOfContents.OffsetToPart8).ToString("X")}

Header Intro
-----------------------------------------------------------
Magic Number:               {h.Intro.MagicNumber.ToString("X")}
Expected SHA-256 hash:      {h.Intro.ExpectedHash}
AES 256 key hex string:     {StringHelper.ByteArrayToString(h.Intro.AesKeyHexString)}
Header size:                {h.Intro.HeaderSize.ToString("X")}
NeFS version:               {h.Intro.NefsVersion.ToString("X")}
Number of items:            {h.Intro.NumberOfItems.ToString("X")}
Unknown 0x70:               {h.Intro.Unknown0x70zlib.ToString("X")}
Unknown 0x78:               {h.Intro.Unknown0x78.ToString("X")}

Header Table of Contents
-----------------------------------------------------------
Offset to Part 1:           {h.TableOfContents.OffsetToPart1.ToString("X")}
Offset to Part 2:           {h.TableOfContents.OffsetToPart2.ToString("X")}
Offset to Part 3:           {h.TableOfContents.OffsetToPart3.ToString("X")}
Offset to Part 4:           {h.TableOfContents.OffsetToPart4.ToString("X")}
Offset to Part 5:           {h.TableOfContents.OffsetToPart5.ToString("X")}
Offset to Part 6:           {h.TableOfContents.OffsetToPart6.ToString("X")}
Offset to Part 7:           {h.TableOfContents.OffsetToPart7.ToString("X")}
Offset to Part 8:           {h.TableOfContents.OffsetToPart8.ToString("X")}
Num Volumes:                {h.TableOfContents.NumVolumes.ToString("X")}
Hash Block Size (<< 15):    {h.TableOfContents.HashBlockSize.ToString("X")}
Block Size (<< 15):         {h.TableOfContents.BlockSize.ToString("X")}
Split Size (<< 15):         {h.TableOfContents.SplitSize.ToString("X")}
Unknown 0x28:               {StringHelper.ByteArrayToString(h.TableOfContents.Unknown0x28)}

Header Part 1
-----------------------------------------------------------
Data Offset         Index Part 2       Index Part 4        Id
{headerPart1String}
Header Part 2
-----------------------------------------------------------
Directory Id        First child Id      Part 3 offset       Extracted size      Id
{headerPart2String}
Header Part 3
-----------------------------------------------------------
{headerPart3String}
Header Part 4
-----------------------------------------------------------
Number of entries:          {h.Part4.EntriesByIndex.Count.ToString("X")}
Unknown last value:         {h.Part4.UnkownEndValue.ToString("X")}

Header Part 5
-----------------------------------------------------------
Archive size:               {h.Part5.DataSize.ToString("X")}
First data offset:          {h.Part5.FirstDataOffset.ToString("X")}
Archive name string offset: {h.Part5.DataFileNameStringOffset.ToString("X")}

Header Part 6 (Count: {h.Part7.EntriesByIndex.Count})
-----------------------------------------------------------
{headerPart6String}
Header Part 7 (Count: {h.Part7.EntriesByIndex.Count})
-----------------------------------------------------------
{headerPart7String}
Header Part 8 (Count: {h.Part8.FileDataHashes.Count})
-----------------------------------------------------------
{headerPart8String}
";
	}

	private string GetDebugInfoVersion20(Nefs20Header h, NefsArchiveSource source)
	{
		var headerPart1String = new StringBuilder();
		foreach (var entry in h.Part1.EntriesByIndex)
		{
			headerPart1String.Append($"0x{entry.OffsetToData.ToString("X")}".PadRight(20));
			headerPart1String.Append($"0x{entry.IndexPart2.ToString("X")}".PadRight(20));
			headerPart1String.Append($"0x{entry.IndexPart4.ToString("X")}".PadRight(20));
			headerPart1String.Append($"0x{entry.Id.Value.ToString("X")}".PadRight(20));
			headerPart1String.Append("\n");
		}

		var headerPart2String = new StringBuilder();
		foreach (var entry in h.Part2.EntriesByIndex)
		{
			headerPart2String.Append($"0x{entry.DirectoryId.Value.ToString("X")}".PadRight(20));
			headerPart2String.Append($"0x{entry.FirstChildId.Value.ToString("X")}".PadRight(20));
			headerPart2String.Append($"0x{entry.OffsetIntoPart3.ToString("X")}".PadRight(20));
			headerPart2String.Append($"0x{entry.ExtractedSize.ToString("X")}".PadRight(20));
			headerPart2String.Append($"0x{entry.Id.Value.ToString("X")}".PadRight(20));
			headerPart2String.Append("\n");
		}

		var headerPart3String = new StringBuilder();
		foreach (var s in h.Part3.FileNames)
		{
			headerPart3String.AppendLine(s);
		}

		var headerPart6String = new StringBuilder();
		foreach (var entry in h.Part6.EntriesByIndex)
		{
			headerPart6String.Append($"0x{entry.Volume.ToString("X")}".PadRight(20));
			headerPart6String.Append($"0x{((byte)entry.Flags).ToString("X")}".PadRight(20));
			headerPart6String.Append($"0x{entry.Unknown0x3.ToString("X")}".PadRight(20));
			headerPart6String.Append("\n");
		}

		var headerPart7String = new StringBuilder();
		foreach (var entry in h.Part7.EntriesByIndex)
		{
			headerPart7String.Append($"0x{entry.SiblingId.Value.ToString("X")}".PadRight(20));
			headerPart7String.Append($"0x{entry.Id.Value.ToString("X")}".PadRight(20));
			headerPart7String.Append("\n");
		}

		var headerPart8String = new StringBuilder();
		foreach (var hash in h.Part8.FileDataHashes)
		{
			headerPart8String.Append(hash);
			headerPart8String.Append("\n");
		}

		return $@"Archive Source
-----------------------------------------------------------
{GetArchiveSourceInfo(source)}

General Info
-----------------------------------------------------------
Archive Size:               {h.Part5.DataSize.ToString("X")}
Is Header Encrypted?        {h.Intro.IsEncrypted}

Header size:                {h.Intro.HeaderSize.ToString("X")}
Intro size:                 {NefsHeaderIntro.Size.ToString("X")}
Toc size:                   {Nefs20HeaderIntroToc.Size.ToString("X")}
Part 1 size:                {h.TableOfContents.Part1Size.ToString("X")}
Part 2 size:                {h.TableOfContents.Part2Size.ToString("X")}
Part 3 size:                {h.TableOfContents.Part3Size.ToString("X")}
Part 4 size:                {h.TableOfContents.Part4Size.ToString("X")}
Part 8 size:                {(h.Intro.HeaderSize - h.TableOfContents.OffsetToPart8).ToString("X")}

Header Intro
-----------------------------------------------------------
Magic Number:               {h.Intro.MagicNumber.ToString("X")}
Expected SHA-256 hash:      {h.Intro.ExpectedHash}
AES 256 key hex string:     {StringHelper.ByteArrayToString(h.Intro.AesKeyHexString)}
Header size:                {h.Intro.HeaderSize.ToString("X")}
NeFS version:               {h.Intro.NefsVersion.ToString("X")}
Number of items:            {h.Intro.NumberOfItems.ToString("X")}
Unknown 0x70:               {h.Intro.Unknown0x70zlib.ToString("X")}
Unknown 0x78:               {h.Intro.Unknown0x78.ToString("X")}

Header Table of Contents
-----------------------------------------------------------
Offset to Part 1:           {h.TableOfContents.OffsetToPart1.ToString("X")}
Offset to Part 2:           {h.TableOfContents.OffsetToPart2.ToString("X")}
Offset to Part 3:           {h.TableOfContents.OffsetToPart3.ToString("X")}
Offset to Part 4:           {h.TableOfContents.OffsetToPart4.ToString("X")}
Offset to Part 5:           {h.TableOfContents.OffsetToPart5.ToString("X")}
Offset to Part 6:           {h.TableOfContents.OffsetToPart6.ToString("X")}
Offset to Part 7:           {h.TableOfContents.OffsetToPart7.ToString("X")}
Offset to Part 8:           {h.TableOfContents.OffsetToPart8.ToString("X")}
Num Volumes:                {h.TableOfContents.NumVolumes.ToString("X")}
Hash Block Size (<< 15):    {h.TableOfContents.HashBlockSize.ToString("X")}
Unknown 0x24:               {StringHelper.ByteArrayToString(h.TableOfContents.Unknown0x24)}

Header Part 1
-----------------------------------------------------------
Data Offset         Index Part 2        Index Part 4        Id
{headerPart1String}
Header Part 2
-----------------------------------------------------------
Directory Id        First child Id      Part 3 offset       Extracted size      Id
{headerPart2String}
Header Part 3
-----------------------------------------------------------
{headerPart3String}
Header Part 4
-----------------------------------------------------------
Number of entries:          {h.Part4.EntriesByIndex.Count.ToString("X")}

Header Part 5
-----------------------------------------------------------
Archive size:               {h.Part5.DataSize.ToString("X")}
First data offset:          {h.Part5.FirstDataOffset.ToString("X")}
Archive name string offset: {h.Part5.DataFileNameStringOffset.ToString("X")}

Header Part 6 (Count: {h.Part6.EntriesByIndex.Count})
-----------------------------------------------------------
{headerPart6String}
Header Part 7 (Count: {h.Part7.EntriesByIndex.Count})
-----------------------------------------------------------
{headerPart7String}
Header Part 8 (Count: {h.Part8.FileDataHashes.Count})
-----------------------------------------------------------
{headerPart8String}
";
	}

	private void OnWorkspaceArchiveClosed(Object sender, EventArgs e)
	{
		// Update on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			PrintDebugInfo(null, null);
		});
	}

	private void OnWorkspaceArchiveOpened(Object sender, EventArgs e)
	{
		// Update on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			PrintDebugInfo(Workspace.Archive, Workspace.ArchiveSource);
		});
	}

	private void OnWorkspaceArchiveSaved(Object sender, EventArgs e)
	{
		// Update on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			PrintDebugInfo(Workspace.Archive, Workspace.ArchiveSource);
		});
	}

	private void PrintDebugInfo(NefsArchive archive, NefsArchiveSource source)
	{
		this.richTextBox.Text = "";

		if (archive == null)
		{
			return;
		}

		if (archive.Header is Nefs20Header h20)
		{
			this.richTextBox.Text = GetDebugInfoVersion20(h20, source);
		}
		else if (archive.Header is Nefs16Header h16)
		{
			this.richTextBox.Text = GetDebugInfoVersion16(h16, source);
		}
		else
		{
			this.richTextBox.Text = "Unknown header version.";
		}
	}
}
