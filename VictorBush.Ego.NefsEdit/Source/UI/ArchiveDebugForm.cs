// See LICENSE.txt for license information.

using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.Workspace;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
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

	private string GetDebugInfoVersion16(Nefs160Header h, NefsArchiveSource source)
	{
		return $"""
		        Archive Source
		        -----------------------------------------------------------
		        {GetArchiveSourceInfo(source)}

		        {h.ToString("DBG", null)}
		        """;
	}

	private string GetDebugInfoVersion20(Nefs200Header h, NefsArchiveSource source)
	{
		return $"""
		        Archive Source
		        -----------------------------------------------------------
		        {GetArchiveSourceInfo(source)}

		        {h.ToString("DBG", null)}
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
			PrintDebugInfo(Workspace.Archive, Workspace.ArchiveSource);
		});
	}

	private void OnWorkspaceArchiveSaved(object? sender, EventArgs e)
	{
		// Update on UI thread
		UiService.Dispatcher.Invoke(() =>
		{
			PrintDebugInfo(Workspace.Archive, Workspace.ArchiveSource);
		});
	}

	private void PrintDebugInfo(NefsArchive? archive, NefsArchiveSource? source)
	{
		this.richTextBox.Text = "";

		if (archive == null || source is null)
		{
			return;
		}

		if (archive.Header is Nefs200Header h20)
		{
			this.richTextBox.Text = GetDebugInfoVersion20(h20, source);
		}
		else if (archive.Header is Nefs160Header h16)
		{
			this.richTextBox.Text = GetDebugInfoVersion16(h16, source);
		}
		else
		{
			this.richTextBox.Text = "Unknown header version.";
		}
	}
}
