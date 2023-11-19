// See LICENSE.txt for license information.

using System.IO;
using VictorBush.Ego.NefsLib.ArchiveSource;

namespace VictorBush.Ego.NefsEdit.Settings;

/// <summary>
/// Recent file info.
/// </summary>
/// <remarks>
/// Keep this as a record type and not a class. This allows simple comparisons between
/// other recent files in the recent files list.
/// </remarks>
[Serializable]
public sealed record RecentFile
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
				StandardFilePath = standardSource.FilePath;
				Type = nameof(StandardSource);
				break;

			case HeadlessSource gameDatSource:
				GameDatDataFilePath = gameDatSource.DataFilePath;
				GameDatHeaderFilePath = gameDatSource.HeaderFilePath;
				GameDatPrimaryOffset = gameDatSource.PrimaryOffset;
				GameDatPrimarySize = gameDatSource.PrimarySize;
				GameDatSecondaryOffset = gameDatSource.SecondaryOffset;
				GameDatSecondarySize = gameDatSource.SecondarySize;
				Type = "GameDatSource";

				break;

			case NefsInjectSource nefsInjectSource:
				NefsInjectDataFilePath = nefsInjectSource.DataFilePath;
				NefsInjectFilePath = nefsInjectSource.NefsInjectFilePath;
				Type = nameof(NefsInjectSource);
				break;

			default:
				throw new ArgumentException("Unknown archive source type.");
		}
	}

	public string GameDatDataFilePath { get; set; } = "";
	public string GameDatHeaderFilePath { get; set; } = "";
	public long? GameDatPrimaryOffset { get; set; }
	public int? GameDatPrimarySize { get; set; }
	public long? GameDatSecondaryOffset { get; set; }
	public int? GameDatSecondarySize { get; set; }
	public string NefsInjectDataFilePath { get; set; } = "";
	public string NefsInjectFilePath { get; set; } = "";
	public string StandardFilePath { get; set; } = "";
	public string Type { get; set; } = "";

	public NefsArchiveSource ToArchiveSource()
	{
		switch (Type)
		{
			case nameof(StandardSource):
				return NefsArchiveSource.Standard(StandardFilePath);

			case "GameDatSource":
				return NefsArchiveSource.Headless(GameDatDataFilePath, GameDatHeaderFilePath, GameDatPrimaryOffset!.Value, GameDatPrimarySize, GameDatSecondaryOffset!.Value, GameDatSecondarySize);

			case nameof(NefsInjectSource):
				return NefsArchiveSource.NefsInject(NefsInjectDataFilePath, NefsInjectFilePath);

			default:
				throw new InvalidOperationException("Unknown source.");
		}
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		switch (Type)
		{
			case nameof(StandardSource):
				return $"{Path.GetFileName(StandardFilePath)}";

			case "GameDatSource":
				return $"[Headless] {Path.GetFileName(GameDatDataFilePath)} [{GameDatPrimaryOffset}|{GameDatSecondaryOffset}]";

			case nameof(NefsInjectSource):
				return $"[NefsInject] {Path.GetFileName(NefsInjectDataFilePath)}";

			default:
				return "Unknown source.";
		}
	}
}
