// See LICENSE.txt for license information.

using System.Windows.Threading;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.IO;

namespace VictorBush.Ego.NefsEdit.Services;

/// <summary>
/// Provides user interface dialogs and other services.
/// </summary>
internal interface IUiService
{
	/// <summary>
	/// Gets the dispatcher for the UI thread.
	/// </summary>
	Dispatcher Dispatcher { get; }

	/// <summary>
	/// Shows a folder browser dialog.
	/// </summary>
	/// <param name="message">A message to show in the dialog.</param>
	/// <returns>The dialog result and the folder path (if applicable).</returns>
	(DialogResult Result, string Path) ShowFolderBrowserDialog(string message);

	/// <summary>
	/// Shows a message box.
	/// </summary>
	/// <param name="message">The message to display.</param>
	/// <param name="title">The title of the message box.</param>
	/// <param name="buttons">Buttons to show.</param>
	/// <param name="icon">Icon to display.</param>
	/// <returns>The dialog result.</returns>
	DialogResult ShowMessageBox(
		string message,
		string? title = null,
		MessageBoxButtons buttons = MessageBoxButtons.OK,
		MessageBoxIcon icon = MessageBoxIcon.None);

	/// <summary>
	/// Shows the NeFS Edit dialog for opening an archive file.
	/// </summary>
	/// <param name="settingsService">The settings service.</param>
	/// <param name="progressService">The progress service.</param>
	/// <param name="reader">The nefs reader.</param>
	/// <param name="exeHeaderFinder">The exe header finder.</param>
	/// <returns>The dialog result and the archive source (if applicable).</returns>
	(DialogResult Result, NefsArchiveSource? Source) ShowNefsEditOpenFileDialog(
		ISettingsService settingsService,
		IProgressService progressService,
		INefsReader reader,
		INefsExeHeaderFinder exeHeaderFinder);

	/// <summary>
	/// Shows an open file dialog.
	/// </summary>
	/// <param name="filter">Filter for dialog.</param>
	/// <returns>The dialog result and the file name (if applicable).</returns>
	(DialogResult Result, string FileName) ShowOpenFileDialog(string? filter = null);

	/// <summary>
	/// Shows a save file dialog.
	/// </summary>
	/// <param name="defaultName">The default file name.</param>
	/// <param name="filter">Filter for dialog.</param>
	/// <returns>The dialog result and the file name (if applicable).</returns>
	(DialogResult Result, string FileName) ShowSaveFileDialog(string defaultName, string? filter = null);

	/// <summary>
	/// Shows the settings dialog.
	/// </summary>
	/// <param name="settingsService">The settings service to use.</param>
	/// <returns>The dialog result.</returns>
	DialogResult ShowSettingsDialog(ISettingsService settingsService);
}
