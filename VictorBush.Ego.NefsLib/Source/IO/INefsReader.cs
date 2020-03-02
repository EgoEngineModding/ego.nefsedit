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
    }
}
