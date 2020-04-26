// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib
{
    /// <summary>
    /// Defines the source for archive data. A standard NeFS archive is a single .nefs file. But an
    /// archive can have its header and data separated (i.e., game.dat files).
    /// </summary>
    public class NefsArchiveSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsArchiveSource"/> class.
        /// </summary>
        /// <param name="nefsFilePath">Path to a nefs archive file.</param>
        public NefsArchiveSource(string nefsFilePath)
        {
            this.HeaderFilePath = nefsFilePath ?? throw new System.ArgumentNullException(nameof(nefsFilePath));
            this.HeaderOffset = 0;
            this.HeaderPart6Offset = 0;
            this.DataFilePath = nefsFilePath ?? throw new System.ArgumentNullException(nameof(nefsFilePath));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsArchiveSource"/> class.
        /// </summary>
        /// <param name="headerFilePath">File that contains header data.</param>
        /// <param name="headerOffset">Offset to header data.</param>
        /// <param name="headerPart6Offset">Offset to header part 6 data.</param>
        /// <param name="dataFilePath">File that contains item data.</param>
        public NefsArchiveSource(
            string headerFilePath,
            ulong headerOffset,
            ulong headerPart6Offset,
            string dataFilePath)
        {
            this.HeaderFilePath = headerFilePath ?? throw new System.ArgumentNullException(nameof(headerFilePath));
            this.HeaderOffset = headerOffset;
            this.HeaderPart6Offset = headerPart6Offset;
            this.DataFilePath = dataFilePath ?? throw new System.ArgumentNullException(nameof(dataFilePath));
        }

        /// <summary>
        /// The file that contents the item data.
        /// </summary>
        public string DataFilePath { get; }

        /// <summary>
        /// The file that contains the archive header.
        /// </summary>
        public string HeaderFilePath { get; }

        /// <summary>
        /// Offset to the header data.
        /// </summary>
        public ulong HeaderOffset { get; }

        /// <summary>
        /// Offset to header part 6 data.
        /// </summary>
        public ulong HeaderPart6Offset { get; }

        /// <summary>
        /// Gets a value indicating whether the archive's header data is in a separate file than the
        /// item data.
        /// </summary>
        public bool IsHeaderSeparate => this.HeaderFilePath != this.DataFilePath;
    }
}
