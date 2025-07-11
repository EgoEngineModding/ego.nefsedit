// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Settings;

/// <summary>
/// Stores recent state of open file dialog so the user can quickly reopen files.
/// </summary>
[Serializable]
public class OpenFileDialogState
{
	/// <summary>
	/// [GameDat] - The path to the data file.
	/// </summary>
	public string GameDatDataFilePath { get; set; } = "";

	/// <summary>
	/// [GameDat] - The path to the header file.
	/// </summary>
	public string GameDatHeaderFilePath { get; set; } = "";

	/// <summary>
	/// [GameDat] - The primary offset value.
	/// </summary>
	public string GameDatPrimaryOffset { get; set; } = "";

	/// <summary>
	/// [GameDat] - The primary size value.
	/// </summary>
	public string GameDatPrimarySize { get; set; } = "";

	/// <summary>
	/// [GameDat] - The secondary offset value.
	/// </summary>
	public string GameDatSecondaryOffset { get; set; } = "";

	/// <summary>
	/// [GameDat] - The secondary size value.
	/// </summary>
	public string GameDatSecondarySize { get; set; } = "";

	/// <summary>
	/// Gets or sets which mode the open file dialog was last in.
	/// </summary>
	public int LastMode { get; set; }

	/// <summary>
	/// [NeFS] - The path to the data file.
	/// </summary>
	public string NefsFilePath { get; set; } = "";

	public string HeadlessExePath { get; set; } = "";

	public string HeadlessDataDirPath { get; set; } = "";
}
