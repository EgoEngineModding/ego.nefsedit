// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Settings;

/// <summary>
/// Settings.
/// </summary>
[Serializable]
public class Settings
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Settings"/> class.
	/// </summary>
	public Settings()
	{
		RecentFiles = new List<RecentFile>();
		OpenFileDialogState = new OpenFileDialogState();
		Dirt4Dir = "";
		DirtRally1Dir = "";
		DirtRally2Dir = "";
		QuickExtractDir = "";
	}

	/// <summary>
	/// The directory for DiRT 4.
	/// </summary>
	public string Dirt4Dir { get; set; }

	/// <summary>
	/// Gets or sets the DiRT Rally 1 directory.
	/// </summary>
	public string DirtRally1Dir { get; set; }

	/// <summary>
	/// The directory for DiRT Rally 2.
	/// </summary>
	public string DirtRally2Dir { get; set; }

	/// <summary>
	/// Gets or sets the last state info for the open file dialog.
	/// </summary>
	public OpenFileDialogState OpenFileDialogState { get; set; }

	/// <summary>
	/// Quick extract.
	/// </summary>
	public string QuickExtractDir { get; set; }

	/// <summary>
	/// Gets or sets the list of recently opened files.
	/// </summary>
	public List<RecentFile> RecentFiles { get; set; }
}
