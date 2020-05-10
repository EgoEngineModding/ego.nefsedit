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
        /// <param name="isTransformed">
        /// Whether the data in this data source has already been transformed (encrypted,
        /// compressed, etc).
        /// </param>
        public NefsFileDataSource(string filePath, UInt64 offset, NefsItemSize size, bool isTransformed)
        {
            this.FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            this.Offset = offset;
            this.Size = size;
            this.IsTransformed = isTransformed;
        }

        /// <inheritdoc/>
        public string FilePath { get; }

        /// <inheritdoc/>
        public Boolean IsTransformed { get; }

        /// <inheritdoc/>
        public UInt64 Offset { get; }

        /// <inheritdoc/>
        public NefsItemSize Size { get; }
    }
}
