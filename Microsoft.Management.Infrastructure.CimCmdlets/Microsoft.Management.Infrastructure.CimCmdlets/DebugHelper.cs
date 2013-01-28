using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal static class DebugHelper
	{
		private static bool generateLog;

		private static bool logInitialized;

		private static bool genrateVerboseMessage;

		internal static string logFile;

		internal static string space;

		internal static string[] spaces;

		internal static object logLock;

		internal static string runspaceStateChanged;

		internal static string classDumpInfo;

		internal static string propertyDumpInfo;

		internal static string defaultPropertyType;

		internal static string propertyValueSet;

		internal static string addParameterSetName;

		internal static string removeParameterSetName;

		internal static string currentParameterSetNameCount;

		internal static string currentParameterSetNameInCache;

		internal static string currentnonMadatoryParameterSetInCache;

		internal static string optionalParameterSetNameCount;

		internal static string finalParameterSetName;

		internal static string addToOptionalParameterSet;

		internal static string startToResolveParameterSet;

		internal static string reservedString;

		internal static bool GenerateLog
		{
			get
			{
				return DebugHelper.generateLog;
			}
			set
			{
				DebugHelper.generateLog = value;
			}
		}

		internal static bool GenrateVerboseMessage
		{
			get
			{
				return DebugHelper.genrateVerboseMessage;
			}
			set
			{
				DebugHelper.genrateVerboseMessage = value;
			}
		}

		static DebugHelper()
		{
			DebugHelper.generateLog = true;
			DebugHelper.logInitialized = false;
			DebugHelper.genrateVerboseMessage = true;
			DebugHelper.logFile = "c:\\temp\\Cim.log";
			DebugHelper.space = "    ";
			string[] empty = new string[6];
			empty[0] = string.Empty;
			empty[1] = DebugHelper.space;
			empty[2] = string.Concat(DebugHelper.space, DebugHelper.space);
			empty[3] = string.Concat(DebugHelper.space, DebugHelper.space, DebugHelper.space);
			empty[4] = string.Concat(DebugHelper.space, DebugHelper.space, DebugHelper.space, DebugHelper.space);
			string[] strArrays = new string[5];
			strArrays[0] = DebugHelper.space;
			strArrays[1] = DebugHelper.space;
			strArrays[2] = DebugHelper.space;
			strArrays[3] = DebugHelper.space;
			strArrays[4] = DebugHelper.space;
			empty[5] = string.Concat(strArrays);
			DebugHelper.spaces = empty;
			DebugHelper.logLock = new object();
			DebugHelper.runspaceStateChanged = "Runspace {0} state changed to {1}";
			DebugHelper.classDumpInfo = "Class type is {0}";
			DebugHelper.propertyDumpInfo = "Property name {0} of type {1}, its value is {2}";
			DebugHelper.defaultPropertyType = "It is a default property, default value is {0}";
			DebugHelper.propertyValueSet = "This property value is set by user {0}";
			DebugHelper.addParameterSetName = "Add parameter set {0} name to cache";
			DebugHelper.removeParameterSetName = "Remove parameter set {0} name from cache";
			DebugHelper.currentParameterSetNameCount = "Cache have {0} parameter set names";
			DebugHelper.currentParameterSetNameInCache = "Cache have parameter set {0} valid {1}";
			DebugHelper.currentnonMadatoryParameterSetInCache = "Cache have optional parameter set {0} valid {1}";
			DebugHelper.optionalParameterSetNameCount = "Cache have {0} optional parameter set names";
			DebugHelper.finalParameterSetName = "------Final parameter set name of the cmdlet is {0}";
			DebugHelper.addToOptionalParameterSet = "Add to optional ParameterSetNames {0}";
			DebugHelper.startToResolveParameterSet = "------Resolve ParameterSet Name";
			DebugHelper.reservedString = "------";
		}

		[Conditional("LOGENABLE")]
		private static void FormatLogMessage(ref string outMessage, string message, object[] args)
		{
			outMessage = string.Format(CultureInfo.CurrentUICulture, message, args);
		}

		internal static string GetSourceCodeInformation(bool withFileName, int depth)
		{
			StackTrace stackTrace = new StackTrace();
			StackFrame frame = stackTrace.GetFrame(depth);
			object[] name = new object[2];
			name[0] = frame.GetMethod().DeclaringType.Name;
			name[1] = frame.GetMethod().Name;
			return string.Format(CultureInfo.CurrentUICulture, "{0}::{1}        ", name);
		}

		internal static void WriteEmptyLine()
		{
			DebugHelper.WriteLog(string.Empty, 0);
		}

		internal static void WriteLog(string message)
		{
			DebugHelper.WriteLog(message, 0);
		}

		internal static void WriteLog(string message, int indent, object[] args)
		{
			string empty = string.Empty;
			DebugHelper.WriteLog(empty, indent);
		}

		internal static void WriteLog(string message, int indent)
		{
		}

		internal static void WriteLogEx(string message, int indent, object[] args)
		{
		}

		internal static void WriteLogEx(string message, int indent)
		{
		}

		internal static void WriteLogEx()
		{

		}

		[Conditional("LOGENABLE")]
		private static void WriteLogInternal(string message, int indent, int depth)
		{
			if (!DebugHelper.logInitialized)
			{
				lock (DebugHelper.logLock)
				{
					if (!DebugHelper.logInitialized)
					{
						DebugHelper.GenerateLog = File.Exists(DebugHelper.logFile);
						DebugHelper.logInitialized = true;
					}
				}
			}
			if (DebugHelper.generateLog)
			{
				if (indent < 0)
				{
					indent = 0;
				}
				if (indent > 5)
				{
					indent = 5;
				}
				string empty = string.Empty;
				if (depth != -1)
				{
					object[] managedThreadId = new object[5];
					managedThreadId[0] = Thread.CurrentThread.ManagedThreadId;
					DateTime now = DateTime.Now;
					managedThreadId[1] = now.Hour;
					DateTime dateTime = DateTime.Now;
					managedThreadId[2] = dateTime.Minute;
					DateTime now1 = DateTime.Now;
					managedThreadId[3] = now1.Second;
					managedThreadId[4] = DebugHelper.GetSourceCodeInformation(true, depth);
					empty = string.Format(CultureInfo.InvariantCulture, "Thread {0}#{1}:{2}:{3} {4}", managedThreadId);
				}
				lock (DebugHelper.logLock)
				{
					using (StreamWriter streamWriter = new StreamWriter(DebugHelper.logFile, true))
					{
						streamWriter.WriteLine(string.Concat(DebugHelper.spaces[indent], empty, "        ", message));
						streamWriter.Close();
					}
				}
			}
		}
	}
}