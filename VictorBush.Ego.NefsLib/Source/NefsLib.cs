// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Library configuration.
    /// </summary>
    public static class NefsLib
    {
        private static ILoggerFactory logFactory = null;

        /// <summary>
        /// Gets or sets the logger factory used by the library.
        /// </summary>
        public static ILoggerFactory LogFactory
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
    }
}
