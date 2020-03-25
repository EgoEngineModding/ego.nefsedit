// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib
{
    using System.Runtime.CompilerServices;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Library configuration.
    /// </summary>
    public static class NefsLog
    {
        private static ILoggerFactory logFactory = null;

        /// <summary>
        /// Gets or sets the logger factory used by the library.
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (logFactory == null)
                {
                    logFactory = new NullLoggerFactory();
                }

                return logFactory;
            }

            set
            {
                logFactory = value;
            }
        }

        /// <summary>
        /// Gets a logger.
        /// </summary>
        /// <param name="filename">Name of file the logger is for.</param>
        /// <returns>The log instance.</returns>
        public static ILogger GetLogger([CallerFilePath]string filename = "")
        {
            return LoggerFactory.CreateLogger(filename);
        }
    }
}
