// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.ArchiveSource
{
    using System;
    using System.IO;

    /// <summary>
    /// Defines the source for archive data. A standard NeFS archive is a single .nefs file. But an
    /// archive can have its header and data separated (i.e., game.dat files).
    /// </summary>
    public sealed class NefsInjectSource : NefsArchiveSource
    {
        internal NefsInjectSource(string dataFilePath, string nefsInjectFilePath)
            : base(dataFilePath)
            {
                this.NefsInjectFilePath = nefsInjectFilePath ?? throw new ArgumentNullException(nameof(nefsInjectFilePath));
            }

        /// <summary>
        /// Path to the file containing the file data (i.e., game.dat, game.bin, etc.).
        /// </summary>
        public string DataFilePath => this.FilePath;

            /// <summary>
            /// Path to the NefsInject file that contains the archive header and related offsets.
            /// </summary>
            public string NefsInjectFilePath { get; }

    }
}
