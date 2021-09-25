// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.IO
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.ArchiveSource;
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
        //Task<List<NefsArchiveSource>> FindHeadersAsync(string exePath, string dataFileDir, NefsProgress p);

        /// <summary>
        /// Reads a standard NeFS archive from the file system.
        /// </summary>
        /// <param name="filePath">The file path of the archive to read.</param>
        /// <param name="p">Progress information.</param>
        /// <returns>The loaded <see cref="NefsArchive"/>.</returns>
        //Task<NefsArchive> ReadArchiveAsync(string filePath, NefsProgress p);

        /// <summary>
        /// Reads a standard NeFS archive from the file system.
        /// </summary>
        /// <param name="filePath">The file path of the archive to read.</param>
        /// <param name="headerOffset">The offset to the header intro.</param>
        /// <param name="p">Progress information.</param>
        /// <returns>The loaded <see cref="NefsArchive"/>.</returns>
        //Task<NefsArchive> ReadArchiveAsync(string filePath, long headerOffset, NefsProgress p);

        /// <summary>
        /// Reads a NeFS archive from the specified source.
        /// </summary>
        /// <param name="source">The archive source.</param>
        /// <param name="p">Progress information.</param>
        /// <returns>The loaded <see cref="NefsArchive"/>.</returns>
        //Task<NefsArchive> ReadArchiveAsync(NefsArchiveSource source, NefsProgress p);

        /// <summary>
        /// Reads a NeFS archive with a split header. For example, archives like game.dat or
        /// game.bin have their headers stored in the game's executable file.
        /// </summary>
        /// <param name="dataFilePath">The archive file path containing the file data (i.e., game.dat).</param>
        /// <param name="headerFilePath">
        /// The file path containing the header data (i.e., the game executable).
        /// </param>
        /// <param name="primaryOffset">The offset to the header intro.</param>
        /// <param name="primarySize">The size of the primary header data. Use null if unknown.</param>
        /// <param name="secondaryOffset">The offset to header part 6.</param>
        /// <param name="secondarySize">
        /// The size of the secondary header data (part 6/7). Use null if unknown.
        /// </param>
        /// <param name="p">Progress information.</param>
        /// <returns>The loaded <see cref="NefsArchive"/>.</returns>
        //Task<NefsArchive> ReadArchiveAsync(string dataFilePath, string headerFilePath, long primaryOffset, int? primarySize, long secondaryOffset, int? secondarySize, NefsProgress p);

        Task<NefsArchive> ReadArchiveAsync(string filePath, long headerOffset, NefsProgress p);
        Task<NefsArchive> ReadArchiveAsync(NefsArchiveSource source, NefsProgress p);
        //Task<NefsArchive> ReadGameDatArchiveAsync(string dataFilePath, string headerFilePath, long primaryOffset, int? primarySize, long secondaryOffset, int? secondarySize, NefsProgress p);
        //Task<NefsArchive> ReadNefsInjectArchiveAsync(string dataFilePath, string nefsInjectFilePath, NefsProgress p);

    }
}
