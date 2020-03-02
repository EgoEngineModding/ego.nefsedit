// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Workspace
{
    using System;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsEdit.Services;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.IO;

    /// <summary>
    /// Workspace that provides operations for opening, editing, and saving archives. Exposes
    /// various services and events that can be used throughout the application.
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
        /// Gets the current open archive.
        /// </summary>
        NefsArchive Archive { get; }

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
        /// Gets the progress service.
        /// </summary>
        IProgressService ProgressService { get; }

        /// <summary>
        /// Gets the UI service.
        /// </summary>
        IUiService UiService { get; }

        /// <summary>
        /// Closes the current open archive.
        /// </summary>
        /// <returns>True if the archive was closed; false otherwise.</returns>
        Task<bool> CloseArchiveAsync();

        /// <summary>
        /// Opens the specified archive.
        /// </summary>
        /// <param name="filePath">File path to the archive to open.</param>
        /// <returns>True if archive was opened.</returns>
        Task<bool> OpenArchiveAsync(string filePath);

        /// <summary>
        /// Shows an open file dialog so the user can choose an archive to open, then opens the archive.
        /// </summary>
        /// <returns>True if archive was opened.</returns>
        Task<bool> OpenArchiveByDialogAsync();
    }
}
