// See LICENSE.txt for license information.

using System;
using System.IO.Abstractions;
using System.Windows.Forms;
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
            var progressService = new ProgressService();
            var uiService = new UiService();
            var nefsCompressor = new NefsCompressor(fileSystem);
            var workspace = new NefsEditWorkspace(
                fileSystem,
                progressService,
                uiService,
                nefsCompressor);

            // Run application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EditorForm(workspace));
        }
    }
}
