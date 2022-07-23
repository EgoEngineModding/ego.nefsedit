// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsEdit.Utility;

/// <summary>
/// Log utilities.
/// </summary>
public class LogHelper
{
	private static ILoggerFactory loggerFactory;

	/// <summary>
	/// The logger factory to use.
	/// </summary>
	public static ILoggerFactory LoggerFactory
	{
		get
		{
			if (loggerFactory == null)
			{
				return new NullLoggerFactory();
			}

			return loggerFactory;
		}

		set
		{
			loggerFactory = value;
		}
	}

	/// <summary>
	/// Gets a logger.
	/// </summary>
	/// <param name="filename">Name of file the logger is for.</param>
	/// <returns>The log instance.</returns>
	public static ILogger GetLogger([CallerFilePath] string filename = "")
	{
		return LoggerFactory.CreateLogger(filename);
	}
}
