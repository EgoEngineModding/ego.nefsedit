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
        /// Searches the array of bytes from the specified offset until the end for a NeFS header.
        /// Returns the name of the data file and the offset to the beginning of the header in the
        /// byte array. Returns null if no header found. This cannot find encrypted headers.
        /// </summary>
        /// <param name="bytes">Byte array to search.</param>
        /// <param name="offset">Offset into the array to start looking at.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>
        /// The name of the data file and the offset of the header in the byte array. Null if no
        /// header found.
        /// </returns>
        Task<(string, ulong)?> FindHeaderAsync(byte[] bytes, ulong offset, NefsProgress p);

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
