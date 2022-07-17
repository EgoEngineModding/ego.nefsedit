// See LICENSE.txt for license information.

using System;
using System.IO;
using System.IO.Abstractions;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Serilog;
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

namespace VictorBush.Ego.NefsEdit
{
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
            var fileSystem = new FileSystem();
            var uiService = new UiService(System.Windows.Threading.Dispatcher.CurrentDispatcher, fileSystem);
            var settingsService = new SettingsService(fileSystem, uiService);
            var progressService = new ProgressService(uiService);
            var nefsTransformer = new NefsTransformer(fileSystem);
            var nefsReader = new NefsReader(fileSystem);
            var nefsWriter = new NefsWriter(TempDirectory, fileSystem, nefsTransformer);
            var workspace = new NefsEditWorkspace(
                fileSystem,
                progressService,
                uiService,
                settingsService,
                nefsReader,
                nefsWriter,
                nefsTransformer);

            // Run application
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EditorForm(workspace, uiService, settingsService));
        }
    }
}
