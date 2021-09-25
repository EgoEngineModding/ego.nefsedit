// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.ArchiveSource
{
    using System;
    using System.IO;

    public sealed class GameDatSource : NefsArchiveSource
    {
        internal GameDatSource(string dataFilePath, string headerFilePath, long primaryOffset, int? primarySize, long secondaryOffset, int? secondarySize)
        : base(dataFilePath)
            {
                this.HeaderFilePath = headerFilePath ?? throw new ArgumentNullException(nameof(headerFilePath));
                this.PrimaryOffset = primaryOffset;
                this.PrimarySize = primarySize;
                this.SecondaryOffset = secondaryOffset;
                this.SecondarySize = secondarySize;
            }

        /// <summary>
        /// Path to the file containing the file data (i.e., game.dat, game.bin, etc.).
        /// </summary>
        public string DataFilePath => this.FilePath;


            /// <summary>
            /// Path to the file containing the archive header (i.e., the game executable).
            /// </summary>
            public string HeaderFilePath { get; }


            /// <summary>
            /// Offset to the beginning of the header from the beginning of the file specified by <see cref="HeaderFilePath"/>.
            /// </summary>
            public long PrimaryOffset { get; }

            /// <summary>
            /// The size of the first chunk of header data. This includes the intro, table of contents,
            /// parts 1-5, and part 8. This size is not the same value as the total header size as
            /// stored within the header itself. Providing this value is optional. If not provided, the
            /// size of header part 8 will be deduced based on the total size of file data. The extra
            /// data from part 8 will not be loaded.
            /// </summary>
            public int? PrimarySize { get; }

            /// <summary>
            /// The offset to header part 6.
            /// </summary>
            public long SecondaryOffset { get; }

            /// <summary>
            /// The size of the part 6 and 7 chunk. Part 6 and 7 are stored together separately from the
            /// rest of the header data. This is optional. If not provided, the size of parts 6 and 7
            /// will be deduced from the other header data and any extra data at the end will be ignored.
            /// </summary>
            public int? SecondarySize { get; }
    }
}
