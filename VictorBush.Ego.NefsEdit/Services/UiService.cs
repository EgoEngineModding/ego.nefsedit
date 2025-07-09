// See LICENSE.txt for license information.

using System.IO.Abstractions;
using System.Windows.Threading;
using VictorBush.Ego.NefsEdit.UI;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.IO;

namespace VictorBush.Ego.NefsEdit.Services;

/// <summary>
/// UI service implementation.
/// </summary>
internal class UiService : IUiService
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UiService"/> class.
	/// </summary>
	public UiService(Dispatcher dispatcher, IFileSystem fileSystem)
	{
		Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
	}

	/// <inheritdoc/>
	public Dispatcher Dispatcher { get; }

	private IFileSystem FileSystem { get; }

	/// <inheritdoc/>
	public (DialogResult Result, string Path) ShowFolderBrowserDialog(string message)
	{
		using (var dialog = new FolderBrowserDialog())
		{
			dialog.Description = message;
			dialog.ShowNewFolderButton = true;
			var result = dialog.ShowDialog();
			return (result, dialog.SelectedPath);
		}
	}

	/// <inheritdoc/>
	public DialogResult ShowMessageBox(string message, string? title = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
	{
		return MessageBox.Show(message, title, buttons, icon);
	}

	/// <inheritdoc/>
	public (DialogResult Result, NefsArchiveSource? Source) ShowNefsEditOpenFileDialog(
		ISettingsService settingsService,
		IProgressService progressService,
		INefsReader reader,
		INefsExeHeaderFinder exeHeaderFinder)
	{
		using var dialog = new OpenFileForm(settingsService, this, progressService, reader, FileSystem, exeHeaderFinder);
		var result = dialog.ShowDialog();
		var source = dialog.ArchiveSource;
		return (result, source);
	}

	/// <inheritdoc/>
	public (DialogResult Result, string FileName) ShowOpenFileDialog(string? filter = null)
	{
		using (var dialog = new OpenFileDialog())
		{
			dialog.Multiselect = false;
			dialog.Filter = filter;
			var result = dialog.ShowDialog();
			return (result, dialog.FileName);
		}
	}

	/// <inheritdoc/>
	public (DialogResult Result, string FileName) ShowSaveFileDialog(string defaultName, string? filter = null)
	{
		using (var dialog = new SaveFileDialog())
		{
			dialog.OverwritePrompt = true;
			dialog.FileName = defaultName;
			dialog.Filter = filter;
			var result = dialog.ShowDialog();
			return (result, dialog.FileName);
		}
	}

	/// <inheritdoc/>
	public DialogResult ShowSettingsDialog(ISettingsService settingsService)
	{
		using (var dialog = new SettingsForm(settingsService, this))
		{
			return dialog.ShowDialog();
		}
	}
}
