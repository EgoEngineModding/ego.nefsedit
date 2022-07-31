// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.IO;
using System.IO.Abstractions;
using System.Xml.Serialization;
using VictorBush.Ego.NefsEdit.Settings;
using VictorBush.Ego.NefsEdit.Utility;

namespace VictorBush.Ego.NefsEdit.Services;

/// <summary>
/// Settings service implementation.
/// </summary>
internal class SettingsService : ISettingsService
{
	private static readonly ILogger Log = LogHelper.GetLogger();

	private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");

	/// <summary>
	/// Initializes a new instance of the <see cref="SettingsService"/> class.
	/// </summary>
	/// <param name="fileSystem">The file system.</param>
	/// <param name="uiService">The UI service.</param>
	public SettingsService(IFileSystem fileSystem, IUiService uiService)
	{
		FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
		Settings = new Settings.Settings();
	}

	/// <inheritdoc/>
	public bool CheckForDatabaseUpdatesOnStartup
	{
		get => Settings.CheckForDatabaseUpdatesOnStartup;
		set => Settings.CheckForDatabaseUpdatesOnStartup = value;
	}

	/// <inheritdoc/>
	public String Dirt4Dir
	{
		get => Settings.Dirt4Dir;
		set => Settings.Dirt4Dir = value;
	}

	/// <inheritdoc/>
	public String Dirt4Exe => Path.Combine(Dirt4Dir, Constants.Dirt4ExeName);

	/// <inheritdoc/>
	public String Dirt4GameDatDir => Path.Combine(Dirt4Dir, Constants.Dirt4GameDatPath);

	/// <inheritdoc/>
	public String DirtRally1Dir
	{
		get => Settings.DirtRally1Dir;
		set => Settings.DirtRally1Dir = value;
	}

	/// <inheritdoc/>
	public String DirtRally1Exe => Path.Combine(DirtRally1Dir, Constants.DirtRally1ExeName);

	/// <inheritdoc/>
	public String DirtRally1GameBinDir => Path.Combine(DirtRally1Dir, Constants.DirtRally1GameBinPath);

	/// <inheritdoc/>
	public String DirtRally2Dir
	{
		get => Settings.DirtRally2Dir;
		set => Settings.DirtRally2Dir = value;
	}

	/// <inheritdoc/>
	public String DirtRally2Exe => Path.Combine(DirtRally2Dir, Constants.DirtRally2ExeName);

	/// <inheritdoc/>
	public String DirtRally2GameDatDir => Path.Combine(DirtRally2Dir, Constants.DirtRally2GameDatPath);

	/// <inheritdoc/>
	public OpenFileDialogState OpenFileDialogState
	{
		get => Settings.OpenFileDialogState;
		set => Settings.OpenFileDialogState = value;
	}

	/// <inheritdoc/>
	public string QuickExtractDir
	{
		get => Settings.QuickExtractDir ?? "";
		set => Settings.QuickExtractDir = value;
	}

	/// <inheritdoc/>
	public List<RecentFile> RecentFiles
	{
		get => Settings.RecentFiles;
		set => Settings.RecentFiles = value;
	}

	private IFileSystem FileSystem { get; }

	private Settings.Settings Settings { get; set; }

	private IUiService UiService { get; }

	/// <inheritdoc/>
	public bool ChooseQuickExtractDir()
	{
		var (result, dir) = UiService.ShowFolderBrowserDialog("Choose a quick extract directory.");
		if (result != DialogResult.OK)
		{
			return false;
		}

		if (Settings == null)
		{
			ResetSettings();
		}

		Settings.QuickExtractDir = dir;
		Save();
		return true;
	}

	/// <inheritdoc/>
	public void Load()
	{
		if (!FileSystem.File.Exists(SettingsFilePath))
		{
			// Settings file doesn't exist, create it
			ResetSettings();
			return;
		}

		Log.LogInformation("----------------------------");
		Log.LogInformation($"Loading settings...");

		// Settings file exists, load it
		try
		{
			// Deserialize XML settings file
			using (var reader = FileSystem.File.OpenRead(SettingsFilePath))
			{
				var xs = new XmlSerializer(typeof(Settings.Settings));
				Settings = xs.Deserialize(reader) as Settings.Settings;
			}

			Log.LogInformation($"Settings loaded.");
			OnSettingsLoaded();
		}
		catch (Exception ex)
		{
			Log.LogError($"Failed to read settings file.\r\n{ex.Message}");
			ResetSettings();
		}
	}

	/// <inheritdoc/>
	public void Save()
	{
		// Use defaults if no settings loaded
		if (Settings == null)
		{
			ResetSettings();
		}

		Log.LogInformation("----------------------------");
		Log.LogInformation($"Saving settings...");

		try
		{
			// Serialize to XML file
			using (var writer = FileSystem.File.Open(SettingsFilePath, FileMode.Create))
			{
				var xs = new XmlSerializer(typeof(Settings.Settings));
				xs.Serialize(writer, Settings);
			}

			Log.LogInformation($"Settings saved.");
		}
		catch (Exception ex)
		{
			Log.LogError($"Failed to save settings file.\r\n{ex.Message}");
		}
	}

	private string FindSteamGameDir(string gameSubPath)
	{
		try
		{
			using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
			{
				if (key?.GetValue("SteamPath") is string steamPath)
				{
					var path = Path.Combine(steamPath, gameSubPath);
					if (FileSystem.Directory.Exists(path))
					{
						return path;
					}
				}
			}
		}
		catch (Exception)
		{
		}

		return "";
	}

	/// <summary>
	/// Performs checks when settings are loaded.
	/// </summary>
	private void OnSettingsLoaded()
	{
		var shouldSave = false;

		// Try to auto-find game directories if not set
		if (string.IsNullOrWhiteSpace(DirtRally1Dir))
		{
			DirtRally1Dir = FindSteamGameDir(Constants.DirtRally1SteamPath);
			if (!string.IsNullOrWhiteSpace(DirtRally1Dir))
			{
				Log.LogInformation($"Found DiRT Rally directory: {DirtRally1Dir}");
				shouldSave = true;
			}
		}

		if (string.IsNullOrWhiteSpace(DirtRally2Dir))
		{
			DirtRally2Dir = FindSteamGameDir(Constants.DirtRally2SteamPath);
			if (!string.IsNullOrWhiteSpace(DirtRally2Dir))
			{
				Log.LogInformation($"Found DiRT Rally 2 directory: {DirtRally2Dir}");
				shouldSave = true;
			}
		}

		if (string.IsNullOrWhiteSpace(Dirt4Dir))
		{
			Dirt4Dir = FindSteamGameDir(Constants.Dirt4SteamPath);
			if (!string.IsNullOrWhiteSpace(Dirt4Dir))
			{
				Log.LogInformation($"Found DiRT 4 directory: {Dirt4Dir}");
				shouldSave = true;
			}
		}

		// Save updated settings if needed
		if (shouldSave)
		{
			Save();
		}
	}

	/// <summary>
	/// Resets application settings to default.
	/// </summary>
	private void ResetSettings()
	{
		Settings = new Settings.Settings
		{
			DirtRally1Dir = FindSteamGameDir(Constants.DirtRally1SteamPath),
			DirtRally2Dir = FindSteamGameDir(Constants.DirtRally2SteamPath),
			Dirt4Dir = FindSteamGameDir(Constants.Dirt4SteamPath),
		};

		Save();
	}
}
