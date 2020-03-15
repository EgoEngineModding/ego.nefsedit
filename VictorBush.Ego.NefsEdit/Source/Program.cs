// See LICENSE.txt for license information.

using System;
using System.IO;
using System.IO.Abstractions;
using System.Windows.Forms;
using System.Windows.Threading;
using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsEdit.UI;
using VictorBush.Ego.NefsEdit.Workspace;
using VictorBush.Ego.NefsLib.IO;

// Configure log4net to monitor the XML configuration file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

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
            // Need to do this to get log4net to work in release builds
            log4net.Config.XmlConfigurator.Configure();

            // TODO: Setup nefs lib logging

            // Setup workspace and services
            var fileSystem = new FileSystem();
            var uiService = new UiService(Dispatcher.CurrentDispatcher);
            var settingsService = new SettingsService(fileSystem, uiService);
            var progressService = new ProgressService(uiService);
            var nefsCompressor = new NefsCompressor(fileSystem);
            var nefsReader = new NefsReader(fileSystem);
            var nefsWriter = new NefsWriter(TempDirectory, fileSystem, nefsCompressor);
            var workspace = new NefsEditWorkspace(
                fileSystem,
                progressService,
                uiService,
                settingsService,
                nefsReader,
                nefsWriter,
                nefsCompressor);

            // Run application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EditorForm(workspace, uiService, settingsService));
        }
    }
}
