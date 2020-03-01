// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataSource
{
    using System;
    using VictorBush.Ego.NefsLib.Item;

    /// <summary>
    /// Defines an item data source from a file on disk.
    /// </summary>
    public class NefsFileDataSource : INefsDataSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsFileDataSource"/> class.
        /// </summary>
        /// <param name="filePath">The path of the file that contain's the data.</param>
        /// <param name="offset">The offset in the source file where the data begins.</param>
        /// <param name="size">Size information about the item's data.</param>
        /// <param name="shouldCompress">
        /// A value indicating whether to compress the data when putting into the archive.
        /// </param>
        public NefsFileDataSource(string filePath, UInt64 offset, NefsItemSize size, bool shouldCompress)
        {
            this.FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            this.Offset = offset;
            this.Size = size;
            this.ShouldCompress = shouldCompress;
        }

        /// <inheritdoc/>
        public string FilePath { get; }

        /// <inheritdoc/>
        public UInt64 Offset { get; }

        /// <inheritdoc/>
        public bool ShouldCompress { get; }

        /// <inheritdoc/>
        public NefsItemSize Size { get; }
    }
}
