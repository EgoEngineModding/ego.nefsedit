// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.IO
{
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// Reads NeFS archives.
    /// </summary>
    public interface INefsReader
    {
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
        /// <param name="dataFilePath">The path to the file that contains the item data.</param>
        /// <param name="p">Progress information.</param>
        /// <returns>The loaded <see cref="NefsArchive"/>.</returns>
        Task<NefsArchive> ReadArchiveAsync(
            string headerFilePath,
            ulong headerOffset,
            string dataFilePath,
            NefsProgress p);
    }
}
