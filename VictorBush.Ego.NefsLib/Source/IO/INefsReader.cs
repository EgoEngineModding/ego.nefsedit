// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.IO
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// Reads NeFS archives.
    /// </summary>
    public interface INefsReader
    {
        /// <summary>
        /// Searches an executable for NeFS headers. This cannot find encrypted headers.
        /// </summary>
        /// <param name="exePath">The exectuable to search.</param>
        /// <param name="dataFileDir">The directory that contains data files (i.e.: game.dat, game.bin).</param>
        /// <param name="p">Progress info.</param>
        /// <returns>List of archive sources for discovered headers.</returns>
        Task<List<NefsArchiveSource>> FindHeadersAsync(string exePath, string dataFileDir, NefsProgress p);

        /// <summary>
        /// Reads a NeFS archive from the file system.
        /// </summary>
        /// <param name="filePath">The path to the archive to load.</param>
        /// <param name="p">Progress information.</param>
        /// <returns>The loaded <see cref="NefsArchive"/>.</returns>
        Task<NefsArchive> ReadArchiveAsync(string filePath, NefsProgress p);

        /// <summary>
        /// Reads a NeFS archive from the file system when the header and item data are in separate files.
        /// </summary>
        /// <param name="source">The source of the archive to open.</param>
        /// <param name="p">Progress information.</param>
        /// <returns>The loaded <see cref="NefsArchive"/>.</returns>
        Task<NefsArchive> ReadArchiveAsync(
            NefsArchiveSource source,
            NefsProgress p);

        /// <summary>
        /// Reads a NeFS archive from the file system when the header and item data are in separate files.
        /// </summary>
        /// <param name="headerFilePath">The path to the file that contains the header.</param>
        /// <param name="headerOffset">The offset to where the header data begins.</param>
        /// <param name="headerPart6Offset">The offset to where part 6 and 7 data begins.</param>
        /// <param name="dataFilePath">The path to the file that contains the item data.</param>
        /// <param name="p">Progress information.</param>
        /// <returns>The loaded <see cref="NefsArchive"/>.</returns>
        Task<NefsArchive> ReadArchiveAsync(
            string headerFilePath,
            ulong headerOffset,
            ulong headerPart6Offset,
            string dataFilePath,
            NefsProgress p);
    }
}
