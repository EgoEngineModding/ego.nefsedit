// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Utility
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Log utilities.
    /// </summary>
    public class LogHelper
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="filename">Name of file the logger is for.</param>
        /// <returns>The log instance.</returns>
        public static log4net.ILog GetLogger([CallerFilePath]string filename = "")
        {
            return log4net.LogManager.GetLogger(filename);
        }
    }
}
