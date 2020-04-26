// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Settings
{
    using System;
    using System.Collections.Generic;
    using VictorBush.Ego.NefsLib;

    /// <summary>
    /// Settings.
    /// </summary>
    [Serializable]
    public class Settings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            this.RecentFiles = new List<NefsArchiveSource>();
            this.OpenFileDialogState = new OpenFileDialogState();
            this.Dirt4Dir = "";
            this.DirtRally1Dir = "";
            this.DirtRally2Dir = "";
            this.QuickExtractDir = "";
        }

        /// <summary>
        /// The directory for DiRT 4.
        /// </summary>
        public string Dirt4Dir { get; set; }

        /// <summary>
        /// Gets or sets the DiRT Rally 1 directory.
        /// </summary>
        public string DirtRally1Dir { get; set; }

        /// <summary>
        /// The directory for DiRT Rally 2.
        /// </summary>
        public string DirtRally2Dir { get; set; }

        /// <summary>
        /// Gets or sets the last state info for the open file dialog.
        /// </summary>
        public OpenFileDialogState OpenFileDialogState { get; set; }

        /// <summary>
        /// Quick extract.
        /// </summary>
        public string QuickExtractDir { get; set; }

        /// <summary>
        /// Gets or sets the list of recently opened files.
        /// </summary>
        public List<NefsArchiveSource> RecentFiles { get; set; }
    }
}
