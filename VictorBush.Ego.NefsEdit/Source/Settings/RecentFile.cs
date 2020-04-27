// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Settings
{
    using System;
    using System.IO;
    using VictorBush.Ego.NefsLib;

    /// <summary>
    /// Recent file info.
    /// </summary>
    [Serializable]
    public class RecentFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecentFile"/> class.
        /// </summary>
        public RecentFile()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecentFile"/> class.
        /// </summary>
        /// <param name="source">Archive source.</param>
        public RecentFile(NefsArchiveSource source)
        {
            this.DataFilePath = source.DataFilePath;
            this.HeaderFilePath = source.HeaderFilePath;
            this.HeaderOffset = source.HeaderOffset;
            this.HeaderPart6Offset = source.HeaderPart6Offset;
        }

        /// <summary>
        /// The file that contents the item data.
        /// </summary>
        public string DataFilePath { get; set; }

        /// <summary>
        /// The file that contains the archive header.
        /// </summary>
        public string HeaderFilePath { get; set; }

        /// <summary>
        /// Offset to the header data.
        /// </summary>
        public ulong HeaderOffset { get; set; }

        /// <summary>
        /// Offset to header part 6 data.
        /// </summary>
        public ulong HeaderPart6Offset { get; set; }

        /// <inheritdoc/>
        public override String ToString()
        {
            var headerOffsetStr = this.HeaderOffset != 0 ? this.HeaderOffset.ToString() : "";
            var headerPart6OffsetStr = this.HeaderPart6Offset != 0 ? this.HeaderPart6Offset.ToString() : "";
            var headerName = Path.GetFileName(this.HeaderFilePath);
            var dataName = Path.GetFileName(this.DataFilePath);

            var str = $"{headerName}";

            if (this.DataFilePath != this.HeaderFilePath)
            {
                str += $" + {dataName}";
            }

            if (headerOffsetStr != "" || headerPart6OffsetStr != "")
            {
                str += $" [{headerOffsetStr}|{headerPart6OffsetStr}]";
            }

            str += $" [{this.DataFilePath}]";

            return str;
        }
    }
}
