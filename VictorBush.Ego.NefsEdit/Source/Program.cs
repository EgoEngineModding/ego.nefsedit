// See LICENSE.txt for license information.

using System.IO;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using VictorBush.Ego.NefsCommon.InjectionDatabase;
using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.UI;
using VictorBush.Ego.NefsEdit.Utility;
using VictorBush.Ego.NefsEdit.Workspace;
using VictorBush.Ego.NefsLib;
using VictorBush.Ego.NefsLib.IO;

// Expose NefsEdit classes to test project
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VictorBush.Ego.NefsEdit.Tests")]

// Required for mocking in the test project
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace VictorBush.Ego.NefsEdit;

/// <summary>
/// The NefsEdit application.
/// </summary>
internal static class Program
{
	/// <summary>
	/// Gets the directory where the application exe is located.
	/// </summary>
	internal static string ExeDirectory => Path.GetDirectoryName(typeof(Program).Assembly.Location);

	/// <summary>
	/// Gets the directory used by the application for writing temporary files.
	/// </summary>
	internal static string TempDirectory => Path.Combine(ExeDirectory, "temp");

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	internal static void Main()
	{
		// Logging
		LogHelper.LoggerFactory = new LoggerFactory();
		var logConfig = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateLogger();
		LogHelper.LoggerFactory.AddSerilog(logConfig);
		NefsLog.LoggerFactory = LogHelper.LoggerFactory;

		// Setup workspace and services
		var host = new HostBuilder()
			.ConfigureLogging(x => x.AddSerilog(logConfig))
			.ConfigureServices(x =>
			{
				x.AddSingleton<EditorForm>();
				x.AddSingleton<IFileSystem, FileSystem>();
				x.AddSingleton(_ => System.Windows.Threading.Dispatcher.CurrentDispatcher);
				x.AddSingleton<IUiService, UiService>();
				x.AddSingleton<ISettingsService, SettingsService>();
				x.AddSingleton<IProgressService, ProgressService>();
				x.AddSingleton<INefsTransformer, NefsTransformer>();
				x.AddSingleton<INefsReader, NefsReader>();
				x.AddSingleton<INefsWriter>(x => new NefsWriter(TempDirectory, x.GetRequiredService<IFileSystem>(), x.GetRequiredService<INefsTransformer>()));
				x.AddSingleton<INefsEditWorkspace, NefsEditWorkspace>();
				x.AddSingleton<IFileDownloader, FileDownloader>();
				x.AddSingleton<IInjectionDatabaseService, InjectionDatabaseService>();
			}).Build();

		// Run application
		Application.EnableVisualStyles();
		Application.SetHighDpiMode(HighDpiMode.SystemAware);
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(host.Services.GetRequiredService<EditorForm>());
	}
}
