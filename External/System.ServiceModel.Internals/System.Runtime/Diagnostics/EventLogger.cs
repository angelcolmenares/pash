using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Runtime.Interop;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace System.Runtime.Diagnostics
{
	internal sealed class EventLogger
	{
		[SecurityCritical]
		private static int logCountForPT;

		private static bool canLogEvent;

		private DiagnosticTraceBase diagnosticTrace;

		[SecurityCritical]
		private string eventLogSourceName;

		private bool isInPartialTrust;

		private const int MaxEventLogsInPT = 5;

		static EventLogger()
		{
			EventLogger.canLogEvent = true;
		}

		[Obsolete("For System.Runtime.dll use only. Call FxTrace.EventLog instead")]
		public EventLogger(string eventLogSourceName, DiagnosticTraceBase diagnosticTrace)
		{
			try
			{
				this.diagnosticTrace = diagnosticTrace;
				if (EventLogger.canLogEvent)
				{
					this.SafeSetLogSourceName(eventLogSourceName);
				}
			}
			catch (SecurityException securityException)
			{
				EventLogger.canLogEvent = false;
			}
		}

		private EventLogger()
		{
			this.isInPartialTrust = this.IsInPartialTrust();
		}

		private static EventLogEntryType EventLogEntryTypeFromEventType(TraceEventType type)
		{
			EventLogEntryType eventLogEntryType = EventLogEntryType.Information;
			TraceEventType traceEventType = type;
			switch (traceEventType)
			{
				case TraceEventType.Critical:
				case TraceEventType.Error:
				{
					eventLogEntryType = EventLogEntryType.Error;
					return eventLogEntryType;
				}
				case TraceEventType.Critical | TraceEventType.Error:
				{
					return eventLogEntryType;
				}
				case TraceEventType.Warning:
				{
					eventLogEntryType = EventLogEntryType.Warning;
					return eventLogEntryType;
				}
				default:
				{
					return eventLogEntryType;
				}
			}
		}

		[SecuritySafeCritical]
		private bool IsInPartialTrust()
		{
			bool flag;
			try
			{
				Process currentProcess = Process.GetCurrentProcess();
				using (currentProcess)
				{
					flag = string.IsNullOrEmpty(currentProcess.ProcessName);
				}
			}
			catch (SecurityException securityException)
			{
				flag = true;
			}
			return flag;
		}

		public void LogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, bool shouldTrace, string[] values)
		{
			if (EventLogger.canLogEvent)
			{
				try
				{
					this.SafeLogEvent(type, eventLogCategory, eventId, shouldTrace, values);
				}
				catch (SecurityException securityException1)
				{
					SecurityException securityException = securityException1;
					EventLogger.canLogEvent = false;
					if (shouldTrace)
					{
						Fx.Exception.TraceHandledException(securityException, TraceEventType.Information);
					}
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void LogEvent(TraceEventType type, ushort eventLogCategory, int eventId, string[] values)
		{
			this.LogEvent(type, eventLogCategory, eventId, true, values);
		}

		internal static string NormalizeEventLogParameter(string eventLogParameter)
		{
			if (eventLogParameter.IndexOf('%') >= 0)
			{
				StringBuilder stringBuilder = null;
				int length = eventLogParameter.Length;
				for (int i = 0; i < length; i++)
				{
					char chr = eventLogParameter[i];
					if (chr == '%')
					{
						if (i + 1 < length)
						{
							if (eventLogParameter[i + 1] < '0' || eventLogParameter[i + 1] > '9')
							{
								if (stringBuilder != null)
								{
									stringBuilder.Append(chr);
								}
							}
							else
							{
								if (stringBuilder == null)
								{
									stringBuilder = new StringBuilder(length + 2);
									for (int j = 0; j < i; j++)
									{
										stringBuilder.Append(eventLogParameter[j]);
									}
								}
								stringBuilder.Append(chr);
								stringBuilder.Append(' ');
							}
						}
						else
						{
							if (stringBuilder != null)
							{
								stringBuilder.Append(chr);
							}
						}
					}
					else
					{
						if (stringBuilder != null)
						{
							stringBuilder.Append(chr);
						}
					}
				}
				if (stringBuilder != null)
				{
					return stringBuilder.ToString();
				}
				else
				{
					return eventLogParameter;
				}
			}
			else
			{
				return eventLogParameter;
			}
		}

		[SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
		[SecuritySafeCritical]
		private void SafeLogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, bool shouldTrace, string[] values)
		{
			this.UnsafeLogEvent(type, eventLogCategory, eventId, shouldTrace, values);
		}

		[SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
		[SecuritySafeCritical]
		private void SafeSetLogSourceName(string eventLogSourceName)
		{
			this.eventLogSourceName = eventLogSourceName;
		}

		[SecurityCritical]
		private void SetLogSourceName(string eventLogSourceName, DiagnosticTraceBase diagnosticTrace)
		{
			this.eventLogSourceName = eventLogSourceName;
			this.diagnosticTrace = diagnosticTrace;
		}

		[SecurityCritical]
		public static EventLogger UnsafeCreateEventLogger(string eventLogSourceName, DiagnosticTraceBase diagnosticTrace)
		{
			EventLogger eventLogger = new EventLogger();
			eventLogger.SetLogSourceName(eventLogSourceName, diagnosticTrace);
			return eventLogger;
		}

		[SecurityCritical]
		[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
		private int UnsafeGetProcessId()
		{
			int id;
			Process currentProcess = Process.GetCurrentProcess();
			using (currentProcess)
			{
				id = currentProcess.Id;
			}
			return id;
		}

		[SecurityCritical]
		[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
		private string UnsafeGetProcessName()
		{
			string processName;
			Process currentProcess = Process.GetCurrentProcess();
			using (currentProcess)
			{
				processName = currentProcess.ProcessName;
			}
			return processName;
		}

		[SecurityCritical]
		public void UnsafeLogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, bool shouldTrace, string[] values)
		{
			string empty;
			if (EventLogger.logCountForPT < 5)
			{
				try
				{
					int length = 0;
					string[] strArrays = new string[(int)values.Length + 2];
					for (int i = 0; i < (int)values.Length; i++)
					{
						string str = values[i];
						if (string.IsNullOrEmpty(str))
						{
							str = string.Empty;
						}
						else
						{
							str = EventLogger.NormalizeEventLogParameter(str);
						}
						strArrays[i] = str;
						length = length + str.Length + 1;
					}
					string str1 = EventLogger.NormalizeEventLogParameter(this.UnsafeGetProcessName());
					strArrays[(int)strArrays.Length - 2] = str1;
					length = length + str1.Length + 1;
					int num = this.UnsafeGetProcessId();
					string str2 = num.ToString(CultureInfo.InvariantCulture);
					strArrays[(int)strArrays.Length - 1] = str2;
					length = length + str2.Length + 1;
					if (length > 0x6400)
					{
						int length1 = 0x6400 / (int)strArrays.Length - 1;
						for (int j = 0; j < (int)strArrays.Length; j++)
						{
							if (strArrays[j].Length > length1)
							{
								strArrays[j] = strArrays[j].Substring(0, length1);
							}
						}
					}
					SecurityIdentifier user = WindowsIdentity.GetCurrent().User;
					byte[] numArray = new byte[user.BinaryLength];
					user.GetBinaryForm(numArray, 0);
					IntPtr[] intPtrArray = new IntPtr[(int)strArrays.Length];
					GCHandle gCHandle = new GCHandle();
					GCHandle[] gCHandleArray = null;
					try
					{
						gCHandle = GCHandle.Alloc(intPtrArray, GCHandleType.Pinned);
						gCHandleArray = new GCHandle[(int)strArrays.Length];
						for (int k = 0; k < (int)strArrays.Length; k++)
						{
							gCHandleArray[k] = GCHandle.Alloc(strArrays[k], GCHandleType.Pinned);
							intPtrArray[k] = gCHandleArray[k].AddrOfPinnedObject();
						}
						this.UnsafeWriteEventLog(type, eventLogCategory, eventId, strArrays, numArray, gCHandle);
					}
					finally
					{
						if (gCHandle.AddrOfPinnedObject() != IntPtr.Zero)
						{
							gCHandle.Free();
						}
						if (gCHandleArray != null)
						{
							GCHandle[] gCHandleArray1 = gCHandleArray;
							for (int l = 0; l < (int)gCHandleArray1.Length; l++)
							{
								GCHandle gCHandle1 = gCHandleArray1[l];
								gCHandle1.Free();
							}
						}
					}
					if (shouldTrace && this.diagnosticTrace != null && this.diagnosticTrace.IsEnabled())
					{
						Dictionary<string, string> strs = new Dictionary<string, string>((int)strArrays.Length + 4);
						strs["CategoryID.Name"] = "EventLogCategory";
						strs["CategoryID.Value"] = eventLogCategory.ToString(CultureInfo.InvariantCulture);
						strs["InstanceID.Name"] = "EventId";
						strs["InstanceID.Value"] = eventId.ToString(CultureInfo.InvariantCulture);
						for (int m = 0; m < (int)values.Length; m++)
						{
							Dictionary<string, string> strs1 = strs;
							string str3 = string.Concat("Value", m.ToString(CultureInfo.InvariantCulture));
							if (values[m] == null)
							{
								empty = string.Empty;
							}
							else
							{
								empty = DiagnosticTraceBase.XmlEncode(values[m]);
							}
							strs1.Add(str3, empty);
						}
						this.diagnosticTrace.TraceEventLogEvent(type, new DictionaryTraceRecord(strs));
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (Fx.IsFatal(exception))
					{
						throw;
					}
				}
				if (this.isInPartialTrust)
				{
					EventLogger.logCountForPT = EventLogger.logCountForPT + 1;
				}
			}
		}

		[SecurityCritical]
		[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
		private void UnsafeWriteEventLog(TraceEventType type, ushort eventLogCategory, uint eventId, string[] logValues, byte[] sidBA, GCHandle stringsRootHandle)
		{
			SafeEventLogWriteHandle safeEventLogWriteHandle = SafeEventLogWriteHandle.RegisterEventSource(null, this.eventLogSourceName);
			using (safeEventLogWriteHandle)
			{
				if (safeEventLogWriteHandle != null)
				{
					HandleRef handleRef = new HandleRef(safeEventLogWriteHandle, stringsRootHandle.AddrOfPinnedObject());
					UnsafeNativeMethods.ReportEvent(safeEventLogWriteHandle, (ushort)EventLogger.EventLogEntryTypeFromEventType(type), eventLogCategory, eventId, sidBA, (ushort)((int)logValues.Length), 0, handleRef, null);
				}
			}
		}
	}
}