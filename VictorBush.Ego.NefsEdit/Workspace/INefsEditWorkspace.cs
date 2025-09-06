// See LICENSE.txt for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using VictorBush.Ego.NefsEdit.Commands;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.ArchiveSource;
using VictorBush.Ego.NefsLib.IO;
using VictorBush.Ego.NefsLib.Item;

namespace VictorBush.Ego.NefsEdit.Workspace;

/// <summary>
/// Workspace that provides operations for opening, editing, and saving archives. Exposes various services and events
/// that can be used throughout the application.
/// </summary>
internal interface INefsEditWorkspace
{
	/// <summary>
	/// Raised when an archive is closed.
	/// </summary>
	event EventHandler ArchiveClosed;

	/// <summary>
	/// Raised when an archive is opened.
	/// </summary>
	event EventHandler ArchiveOpened;

	/// <summary>
	/// Raised when an archive is saved.
	/// </summary>
	event EventHandler ArchiveSaved;

	/// <summary>
	/// Raised when a command is executed. This could be a new command, an undo command, or a redo command.
	/// </summary>
	event EventHandler<NefsEditCommandEventArgs> CommandExecuted;

	/// <summary>
	/// Raised when items are selected or de-selected.
	/// </summary>
	event EventHandler SelectedItemsChanged;

	/// <summary>
	/// Gets the current open archive. Will be null if no archive is open.
	/// </summary>
	NefsArchive? Archive { get; }

	/// <summary>
	/// Gets a value indicating whether the archive has unsaved modifications.
	/// </summary>
	bool ArchiveIsModified { get; }

	/// <summary>
	/// Gets the archive file source info. Will be null if no archive is open.
	/// </summary>
	NefsArchiveSource? ArchiveSource { get; }

	/// <summary>
	/// Gets a value indicating whether a redo is available.
	/// </summary>
	bool CanRedo { get; }

	/// <summary>
	/// Gets a value indicating whether an undo is available.
	/// </summary>
	bool CanUndo { get; }

	/// <summary>
	/// Gets the file system.
	/// </summary>
	IFileSystem FileSystem { get; }

	/// <summary>
	/// Gets the nefs reader.
	/// </summary>
	INefsReader NefsReader { get; }

	/// <summary>
	/// Gets the nefs writer.
	/// </summary>
	INefsWriter NefsWriter { get; }

	/// <summary>
	/// Gets the list of currently selected items.
	/// </summary>
	IReadOnlyList<NefsItem> SelectedItems { get; }

	/// <summary>
	/// Closes the current open archive.
	/// </summary>
	/// <returns>True if the archive was closed; false otherwise.</returns>
	Task<bool> CloseArchiveAsync();

	/// <summary>
	/// Executes a command and adds to the undo buffer.
	/// </summary>
	/// <param name="command">The command to execute.</param>
	void Execute(INefsEditCommand command);

	/// <summary>
	/// Shows a dialog to allow the user to choose where to save the file(s) and extracts them to the desired location.
	/// </summary>
	/// <param name="items">The items to extract.</param>
	/// <returns>True if the items are extracted.</returns>
	Task<bool> ExtractItemsByDialogAsync(IReadOnlyList<NefsItem> items);

	/// <summary>
	/// Extracts the selected items to the quick extract directory, preserving an directory structure from the source archive.
	/// </summary>
	/// <param name="items">The items to extract.</param>
	/// <returns>True if the items were extracted.</returns>
	Task<bool> ExtractItemsByQuickExtractAsync(IReadOnlyList<NefsItem> items);

	/// <summary>
	/// Handles the CLI arguments.
	/// </summary>
	Task HandleCliArgs();

	/// <summary>
	/// Opens the specified archive.
	/// </summary>
	/// <param name="filePath">File path to the archive to open.</param>
	/// <returns>True if archive was opened.</returns>
	Task<bool> OpenArchiveAsync(string filePath);

	/// <summary>
	/// Opens the specified archive.
	/// </summary>
	/// <param name="source">Archive source to open.</param>
	/// <returns>True if archive was opened.</returns>
	Task<bool> OpenArchiveAsync(NefsArchiveSource source);

	/// <summary>
	/// Shows an open file dialog so the user can choose an archive to open, then opens the archive.
	/// </summary>
	/// <returns>True if archive was opened.</returns>
	Task<bool> OpenArchiveByDialogAsync();

	/// <summary>
	/// Performs a redo operation if available.
	/// </summary>
	void Redo();

	/// <summary>
	/// Shows an open file dialog so the user can choose a file to replace the specified item with.
	/// </summary>
	/// <param name="item">The item to replace.</param>
	/// <returns>True if item was marked for replacement.</returns>
	bool ReplaceItemByDialog(NefsItem item);

	/// <summary>
	/// Shows an open file dialog so the user can choose a file to replace the currently selected item with.
	/// </summary>
	/// <returns>True if item was marked for replacement.</returns>
	bool ReplaceSeletedItemByDialog();

	/// <summary>
	/// Saves the currently open archive to the last known location.
	/// </summary>
	/// <returns>True if the archive is saved.</returns>
	Task<bool> SaveArchiveAsync();

	/// <summary>
	/// Saves the currently open archive to the specified location.
	/// </summary>
	/// <param name="destFilePath">The file path to save to.</param>
	/// <returns>True if the archive is saved.</returns>
	Task<bool> SaveArchiveAsync(string destFilePath);

	/// <summary>
	/// Shows a save file dialog to allow the user to choose where to save the archive file.
	/// </summary>
	/// <returns>True if the archive is saved.</returns>
	Task<bool> SaveArchiveByDialogAsync();

	/// <summary>
	/// Selects a single item in the workspace.
	/// </summary>
	/// <param name="item">The item that is now selected.</param>
	void SelectItem(NefsItem item);

	/// <summary>
	/// Selects multiple items in the workspace.
	/// </summary>
	/// <param name="items">The items that are now selected.</param>
	void SelectItems(IEnumerable<NefsItem> items);

	/// <summary>
	/// Performs an undo operation if available.
	/// </summary>
	void Undo();
}
