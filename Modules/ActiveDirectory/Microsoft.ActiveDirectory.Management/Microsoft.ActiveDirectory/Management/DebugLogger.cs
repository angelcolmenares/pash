using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Microsoft.ActiveDirectory.Management
{
	internal static class DebugLogger
	{
		private static DebugLogLevel _level;

		public static DebugLogLevel Level
		{
			get
			{
				return DebugLogger._level;
			}
		}

		static DebugLogger()
		{
			ConfigurationHandler section = (ConfigurationHandler)ConfigurationManager.GetSection("Microsoft.ActiveDirectory");
			if (section == null || section.TraceSection == null)
			{
				return;
			}
			else
			{
				TraceElement traceSection = section.TraceSection;
				DebugLogger._level = traceSection.LogLevel;
				string logFile = traceSection.LogFile;
				if (!string.IsNullOrEmpty(logFile))
				{
					logFile = Environment.ExpandEnvironmentVariables(logFile);
					foreach (TraceListener listener in Trace.Listeners)
					{
						if (listener as DefaultTraceListener == null)
						{
							continue;
						}
						((DefaultTraceListener)listener).LogFileName = logFile;
						break;
					}
				}
				if (DebugLogger._level > DebugLogLevel.Off)
				{
					StringBuilder stringBuilder = new StringBuilder();
					DateTime now = DateTime.Now;
					stringBuilder.Append(string.Concat("[", now.ToString(), "] "));
					stringBuilder.Append(string.Concat(Process.GetCurrentProcess().ProcessName.ToString(), ": "));
					int id = Process.GetCurrentProcess().Id;
					stringBuilder.Append(string.Concat(id.ToString(), ": "));
					stringBuilder.Append(string.Concat("level=", DebugLogger._level.ToString(), ": "));
					stringBuilder.Append(string.Concat("logFile=", logFile));
					Trace.WriteLine(null);
					Trace.WriteLine(stringBuilder.ToString());
				}
				return;
			}
		}

		public static void Assert(bool condition, string category, string message)
		{
			if (DebugLogger._level >= DebugLogLevel.Error && !condition)
			{
				DebugLogger.LogError(category, message);
			}
		}

		public static void Assert(bool condition, string category, string format, object[] args)
		{
			if (DebugLogger._level >= DebugLogLevel.Error && !condition)
			{
				DebugLogger.LogError(category, format, args);
			}
		}

		private static string DetailedLogMessage(string category, DebugLogLevel type, string message)
		{
			StringBuilder stringBuilder = new StringBuilder();
			DateTime now = DateTime.Now;
			stringBuilder.Append(string.Concat("[", now.ToString(), "] "));
			int managedThreadId = Thread.CurrentThread.ManagedThreadId;
			stringBuilder.Append(string.Concat(managedThreadId.ToString(), ": "));
			stringBuilder.Append(string.Concat(category, ": "));
			stringBuilder.Append(string.Concat(type.ToString(), ": "));
			stringBuilder.Append(message);
			return stringBuilder.ToString();
		}

		private static string DetailedLogMessage(string category, DebugLogLevel type, string format, object[] args)
		{
			return DebugLogger.DetailedLogMessage(category, type, string.Format(CultureInfo.InvariantCulture, format, args));
		}

		public static void LogError(string category, string message)
		{
			if (DebugLogger._level >= DebugLogLevel.Error)
			{
				Trace.WriteLine(DebugLogger.DetailedLogMessage(category, DebugLogLevel.Error, message));
			}
		}

		public static void LogError(string category, string format, object[] args)
		{
			if (DebugLogger._level >= DebugLogLevel.Error)
			{
				Trace.WriteLine(DebugLogger.DetailedLogMessage(category, DebugLogLevel.Error, format, args));
			}
		}

		public static void LogInfo(string category, string message)
		{
			if (DebugLogger._level >= DebugLogLevel.Info)
			{
				Trace.WriteLine(DebugLogger.DetailedLogMessage(category, DebugLogLevel.Info, message));
			}
		}

		public static void LogInfo(string category, string format, object[] args)
		{
			if (DebugLogger._level >= DebugLogLevel.Info)
			{
				Trace.WriteLine(DebugLogger.DetailedLogMessage(category, DebugLogLevel.Info, format, args));
			}
		}

		public static void LogWarning(string category, string message)
		{
			if (DebugLogger._level >= DebugLogLevel.Warning)
			{
				Trace.WriteLine(DebugLogger.DetailedLogMessage(category, DebugLogLevel.Warning, message));
			}
		}

		public static void LogWarning(string category, string format, object[] args)
		{
			if (DebugLogger._level >= DebugLogLevel.Warning)
			{
				Trace.WriteLine(DebugLogger.DetailedLogMessage(category, DebugLogLevel.Warning, format, args));
			}
		}

		public static void WriteLine(string category, string message)
		{
			if (DebugLogger._level >= DebugLogLevel.Verbose)
			{
				Trace.WriteLine(DebugLogger.DetailedLogMessage(category, DebugLogLevel.Verbose, message));
			}
		}

		public static void WriteLine(string category, string format, object[] args)
		{
			if (DebugLogger._level >= DebugLogLevel.Verbose)
			{
				Trace.WriteLine(DebugLogger.DetailedLogMessage(category, DebugLogLevel.Verbose, format, args));
			}
		}

		public static void WriteLineIf(bool condition, string category, string message)
		{
			if (condition)
			{
				DebugLogger.WriteLine(category, message);
			}
		}

		public static void WriteLineIf(bool condition, string category, string format, object[] args)
		{
			if (condition)
			{
				DebugLogger.WriteLine(category, format, args);
			}
		}
	}
}