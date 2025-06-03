// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib;

/// <summary>
/// Library configuration.
/// </summary>
public static class NefsLog
{
	private static ILoggerFactory? logFactory;

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
	public static ILogger GetLogger([CallerFilePath] string filename = "")
	{
		return LoggerFactory.CreateLogger(filename);
	}
}
