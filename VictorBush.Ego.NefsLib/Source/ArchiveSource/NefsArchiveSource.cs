// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.ArchiveSource
{
    using System;
    using System.IO;

    public abstract class NefsArchiveSource
    {
        public static StandardSource Standard(string filePath)
        {
            return new StandardSource(filePath);
        }

        public static NefsInjectSource NefsInject(string dataFilePath, string nefsInjectFilePath)
        {
            return new NefsInjectSource(dataFilePath, nefsInjectFilePath);
        }

        public static GameDatSource GameDat(string dataFilePath, string headerFilePath, long primaryOffset, int? primarySize, long secondaryOffset, int? secondarySize)
        {
            return new GameDatSource(dataFilePath, headerFilePath, primaryOffset, primarySize, secondaryOffset, secondarySize);
        }

        protected NefsArchiveSource(string filePath)
        {
            this.FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>
        /// The name of the archive file.
        /// </summary>
        public string FileName => Path.GetFileName(this.FilePath);

        /// <summary>
        /// Path to the archive file. containing the archive's file data.
        /// </summary>
        public string FilePath { get; }
    }
}
