// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Settings
{
    using System;

    /// <summary>
    /// Stores recent state of open file dialog so the user can quickly reopen files.
    /// </summary>
    [Serializable]
    public class OpenFileDialogState
    {
        /// <summary>
        /// The path to the data file.
        /// </summary>
        public string DataFilePath { get; set; }

        /// <summary>
        /// The header offset value.
        /// </summary>
        public string HeaderOffset { get; set; }

        /// <summary>
        /// The base offset used for part 6/7 offsets.
        /// </summary>
        public string HeaderPart6Offset { get; set; }

        /// <summary>
        /// Gets or sets the header file path.
        /// </summary>
        public string HeaderPath { get; set; }

        /// <summary>
        /// Whether the Is Advanced check box is checked.
        /// </summary>
        public bool IsAdvanced { get; set; }

        /// <summary>
        /// Gets or sets which mode the open file dialog was last in.
        /// </summary>
        public int LastMode { get; set; }
    }
}
