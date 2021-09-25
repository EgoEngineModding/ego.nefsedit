// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Settings
{
    using System;
    using System.IO;
    using VictorBush.Ego.NefsLib;
    using VictorBush.Ego.NefsLib.ArchiveSource;

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
            switch (source)
            {
                case StandardSource standardSource:
                    this.StandardFilePath = standardSource.FilePath;
                    this.Type = nameof(StandardSource);
                    break;

                case GameDatSource gameDatSource:
                    this.GameDatDataFilePath = gameDatSource.DataFilePath;
                    this.GameDatHeaderFilePath = gameDatSource.HeaderFilePath;
                    this.GameDatPrimaryOffset = gameDatSource.PrimaryOffset;
                    this.GameDatPrimarySize = gameDatSource.PrimarySize;
                    this.GameDatSecondaryOffset = gameDatSource.SecondaryOffset;
                    this.GameDatSecondarySize = gameDatSource.SecondarySize;
                    this.Type = nameof(GameDatSource);
                    break;

                case NefsInjectSource nefsInjectSource:
                    this.NefsInjectDataFilePath = nefsInjectSource.DataFilePath;
                    this.NefsInjectFilePath = nefsInjectSource.NefsInjectFilePath;
                    this.Type = nameof(NefsInjectSource);
                    break;

                default:
                    throw new ArgumentException("Unknown archive source type.");
            }
        }

        public string Type { get; set; }
        
        public string StandardFilePath { get; set; }

        public string NefsInjectDataFilePath { get; set; }
        public string NefsInjectFilePath { get; set; }

        public string GameDatDataFilePath { get; set; }

        public string GameDatHeaderFilePath { get; set; }

        public long? GameDatPrimaryOffset { get; set; }

        public int? GameDatPrimarySize { get; set; }

        public long? GameDatSecondaryOffset { get; set; }

        public int? GameDatSecondarySize { get; set; }

        public NefsArchiveSource ToArchiveSource()
        {
            switch (this.Type)
            {
                case nameof(StandardSource):
                    return NefsArchiveSource.Standard(this.StandardFilePath);

                case nameof(GameDatSource):
                    return NefsArchiveSource.GameDat(this.GameDatDataFilePath, this.GameDatHeaderFilePath, this.GameDatPrimaryOffset.Value, this.GameDatPrimarySize, this.GameDatSecondaryOffset.Value, this.GameDatSecondarySize);

                case nameof(NefsInjectSource):
                    return NefsArchiveSource.NefsInject(this.NefsInjectDataFilePath, this.NefsInjectFilePath);

                default:
                    throw new InvalidOperationException("Unknown source.");
            }
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            switch (this.Type)
            {
                case nameof(StandardSource):
                    return $"{Path.GetFileName(this.StandardFilePath)}";

                case nameof(GameDatSource):
                    return $"{Path.GetFileName(this.GameDatDataFilePath)} [{this.GameDatPrimaryOffset}|{this.GameDatSecondaryOffset}]";

                case nameof(NefsInjectSource):
                    return $"{Path.GetFileName(this.NefsInjectDataFilePath)}";

                default:
                    return "Unknown source.";
            }
        }
    }
}
