using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Diagnostics
{
	internal class TraceLog
	{
		private const string BaseFileName = "ADClaimsTransformationRulesParserTraceLog";

		private const string BaseFileExt = "txt";

		private const string TargetDirectory = "%windir%\\debug";

		private const string AltTargetDirectory = "%LOCALAPPDATA%\\debug";

		private const string TracingRegKeyName = "SYSTEM\\CurrentControlSet\\Control\\Lsa\\Transformation";

		private const string TracingRegSettingName = "EnableParserTracing";

		private string _traceSource;

		private string _criticalTraceEventSymbol;

		private string _errorTraceEventSymbol;

		private string _warningTraceEventSymbol;

		private string _infoTraceEventSymbol;

		private string _verboseMethodName;

		private string _verboseEventSymbolName;

		private string _infoEventSymbolName;

		private static TextWriter textWriter;

		static TraceLog()
		{
		}

		public TraceLog(string sourceKeywordSymbol)
		{
			this._traceSource = sourceKeywordSymbol;
			this._criticalTraceEventSymbol = string.Concat(sourceKeywordSymbol, "CriticalTraceEvent");
			this._errorTraceEventSymbol = string.Concat(sourceKeywordSymbol, "ErrorTraceEvent");
			this._warningTraceEventSymbol = string.Concat(sourceKeywordSymbol, "WarningTraceEvent");
			this._infoTraceEventSymbol = string.Concat(sourceKeywordSymbol, "InformationalTraceEvent");
			this._verboseMethodName = string.Concat(sourceKeywordSymbol, "VerboseTraceEvent");
			this._verboseEventSymbolName = string.Concat(sourceKeywordSymbol, "{0}VerboseTraceEvent");
			this._infoEventSymbolName = string.Concat(sourceKeywordSymbol, "{0}InformationalTraceEvent");
			this.InitializeTextWriter();
		}

		public void Assert(bool assertion, string msg, object[] args)
		{
			if (!assertion)
			{
				this.Error(string.Concat("ASSERTION: ", msg), args);
				return;
			}
			else
			{
				return;
			}
		}

		public void Error(string msg, object[] args)
		{
			this.WriteLine(this._errorTraceEventSymbol, msg, args);
		}

		public void ErrorSafe(string msg, object[] args)
		{
			try
			{
				this.Error(msg, args);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (ExceptionUtility.IsFatal(exception))
				{
					throw;
				}
			}
		}

		public void Info(string msg, object[] args)
		{
			if (TraceLog.IsEnabled())
			{
				this.WriteLine(this._infoTraceEventSymbol, msg, args);
				return;
			}
			else
			{
				return;
			}
		}

		public void InfoSafe(string msg, object[] args)
		{
			try
			{
				this.Info(msg, args);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (ExceptionUtility.IsFatal(exception))
				{
					throw;
				}
			}
		}

		private void InitializeTextWriter()
		{
			string str;
			if (!TraceLog.IsEnabled())
			{
				if (this.TracingEnabledInRegistry())
				{
					bool flag = false;
					string str1 = Environment.ExpandEnvironmentVariables("%windir%\\debug");
					try
					{
						str = str1;
						string str2 = str;
						string[] strArrays = new string[6];
						strArrays[0] = str2;
						strArrays[1] = "\\ADClaimsTransformationRulesParserTraceLog_";
						DateTime now = DateTime.Now;
						long ticks = now.Ticks;
						strArrays[2] = ticks.ToString(CultureInfo.InvariantCulture);
						strArrays[3] = "_";
						int id = Process.GetCurrentProcess().Id;
						strArrays[4] = id.ToString(CultureInfo.InvariantCulture);
						strArrays[5] = ".txt";
						str = string.Concat(strArrays);
						TraceLog.textWriter = new StreamWriter(str);
					}
					catch (Exception exception)
					{
						flag = true;
					}
					if (flag)
					{
						str1 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\debug");
						try
						{
							if (!Directory.Exists(str1))
							{
								Directory.CreateDirectory(str1);
							}
							str = str1;
							string str3 = str;
							string[] strArrays1 = new string[5];
							strArrays1[0] = str3;
							strArrays1[1] = "\\ADClaimsTransformationRulesParserTraceLog_";
							DateTime dateTime = DateTime.Now;
							strArrays1[2] = dateTime.ToString(CultureInfo.InvariantCulture);
							int num = Process.GetCurrentProcess().Id;
							strArrays1[3] = num.ToString(CultureInfo.InvariantCulture);
							strArrays1[4] = "_.txt";
							str = string.Concat(strArrays1);
							TraceLog.textWriter = new StreamWriter(str);
						}
						catch (Exception exception1)
						{
							Console.WriteLine("Failed to create a trace file in folder {0}", "%LOCALAPPDATA%\\debug");
						}
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		public static bool IsEnabled()
		{
			return TraceLog.textWriter != null;
		}

		private bool TracingEnabledInRegistry()
		{
			int value = 0;
			RegistryKey registryKey = null;
			try
			{
				try
				{
					registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Lsa\\Transformation");
					value = (int)registryKey.GetValue("EnableParserTracing", 0);
				}
				catch (Exception exception)
				{
					value = 0;
				}
			}
			finally
			{
				if (registryKey != null)
				{
					registryKey.Close();
				}
			}
			return value != 0;
		}

		private void WriteLine(string eventSymbol, string msg, object[] args)
		{
			if (TraceLog.IsEnabled())
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (args == null || (int)args.Length == 0)
				{
					stringBuilder.Append(msg);
				}
				else
				{
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, msg, args);
				}
				TraceLog.textWriter.WriteLine(string.Concat(eventSymbol, "::", stringBuilder.ToString(), "\r\n"));
				TraceLog.textWriter.Flush();
				return;
			}
			else
			{
				return;
			}
		}
	}
}