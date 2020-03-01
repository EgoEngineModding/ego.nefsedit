using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.IO;

namespace VictorBush.Ego.NefsEdit
{
    internal interface INefsEditWorkspace
    {
        NefsWriter NefsWriter { get; }

        NefsReader NefsReader { get; }

        INefsCompressor NefsCompressor { get; }

        IFileSystem FileSystem { get; }

        IProgressService ProgressService { get; }

        /// <summary>
        /// Raised when an archive is opened.
        /// </summary>
        event EventHandler ArchiveOpened;

        /// <summary>
        /// Raised when an archive is closed.
        /// </summary>
        event EventHandler ArchiveClosed;

        /// <summary>
        /// Raised when the selected items changed.
        /// </summary>
        event EventHandler SelectedItemsChanged;

        Task CloseArchiveAsync();

        Task OpenArchiveByDialogAsync();

        Task OpenArchiveAsync(string filePath);

        Task SaveArchiveAsync();

        Task ExtractItemAsync();
    }
}
