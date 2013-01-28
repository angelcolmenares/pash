using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Xml;

namespace System.Runtime.Diagnostics
{
	internal abstract class DiagnosticTraceBase
	{
		protected const string DefaultTraceListenerName = "Default";

		protected const string TraceRecordVersion = "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord";

		protected static string AppDomainFriendlyName;

		private object thisLock;

		private bool tracingEnabled;

		private bool calledShutdown;

		private bool haveListeners;

		private SourceLevels level;

		protected string TraceSourceName;

		private TraceSource traceSource;

		[SecurityCritical]
		private string eventSourceName;

		private const ushort TracingEventLogCategory = 4;

		public static Guid ActivityId
		{
			[SecuritySafeCritical]
			get
			{
				object activityId = Trace.CorrelationManager.ActivityId;
				if (activityId == null)
				{
					return Guid.Empty;
				}
				else
				{
					return (Guid)activityId;
				}
			}
			[SecuritySafeCritical]
			set
			{
				Trace.CorrelationManager.ActivityId = value;
			}
		}

		protected bool CalledShutdown
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.calledShutdown;
			}
		}

		protected string EventSourceName
		{
			[SecuritySafeCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.eventSourceName;
			}
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.eventSourceName = value;
			}
		}

		public bool HaveListeners
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.haveListeners;
			}
		}

		protected DateTime LastFailure
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set;
		}

		public SourceLevels Level
		{
			get
			{
				if (this.TraceSource != null && this.TraceSource.Switch.Level != this.level)
				{
					this.level = this.TraceSource.Switch.Level;
				}
				return this.level;
			}
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.SetLevelThreadSafe(value);
			}
		}

		protected static int ProcessId
		{
			[SecuritySafeCritical]
			get
			{
				int id;
				Process currentProcess = Process.GetCurrentProcess();
				using (currentProcess)
				{
					id = currentProcess.Id;
				}
				return id;
			}
		}

		protected static string ProcessName
		{
			[SecuritySafeCritical]
			get
			{
				string processName;
				Process currentProcess = Process.GetCurrentProcess();
				using (currentProcess)
				{
					processName = currentProcess.ProcessName;
				}
				return processName;
			}
		}

		public TraceSource TraceSource
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.traceSource;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.SetTraceSource(value);
			}
		}

		public bool TracingEnabled
		{
			get
			{
				if (!this.tracingEnabled)
				{
					return false;
				}
				else
				{
					return this.traceSource != null;
				}
			}
		}

		static DiagnosticTraceBase()
		{
			DiagnosticTraceBase.AppDomainFriendlyName = AppDomain.CurrentDomain.FriendlyName;
		}

		public DiagnosticTraceBase(string traceSourceName)
		{
			this.tracingEnabled = true;
			this.thisLock = new object();
			this.TraceSourceName = traceSourceName;
			this.LastFailure = DateTime.MinValue;
		}

		[SecuritySafeCritical]
		protected void AddDomainEventHandlersForCleanup()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			if (this.TraceSource != null)
			{
				this.haveListeners = this.TraceSource.Listeners.Count > 0;
			}
			this.tracingEnabled = this.haveListeners;
			if (this.TracingEnabled)
			{
				currentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.UnhandledExceptionHandler);
				this.SetLevel(this.TraceSource.Switch.Level);
				currentDomain.DomainUnload += new EventHandler(this.ExitOrUnloadEventHandler);
				currentDomain.ProcessExit += new EventHandler(this.ExitOrUnloadEventHandler);
			}
		}

		protected static void AddExceptionToTraceString(XmlWriter xml, Exception exception)
		{
			xml.WriteElementString("ExceptionType", DiagnosticTraceBase.XmlEncode(exception.GetType().AssemblyQualifiedName));
			xml.WriteElementString("Message", DiagnosticTraceBase.XmlEncode(exception.Message));
			xml.WriteElementString("StackTrace", DiagnosticTraceBase.XmlEncode(DiagnosticTraceBase.StackTraceString(exception)));
			xml.WriteElementString("ExceptionString", DiagnosticTraceBase.XmlEncode(exception.ToString()));
			Win32Exception win32Exception = exception as Win32Exception;
			if (win32Exception != null)
			{
				int nativeErrorCode = win32Exception.NativeErrorCode;
				xml.WriteElementString("NativeErrorCode", nativeErrorCode.ToString("X", CultureInfo.InvariantCulture));
			}
			if (exception.Data != null && exception.Data.Count > 0)
			{
				xml.WriteStartElement("DataItems");
				foreach (object key in exception.Data.Keys)
				{
					xml.WriteStartElement("Data");
					xml.WriteElementString("Key", DiagnosticTraceBase.XmlEncode(key.ToString()));
					xml.WriteElementString("Value", DiagnosticTraceBase.XmlEncode(exception.Data[key].ToString()));
					xml.WriteEndElement();
				}
				xml.WriteEndElement();
			}
			if (exception.InnerException != null)
			{
				xml.WriteStartElement("InnerException");
				DiagnosticTraceBase.AddExceptionToTraceString(xml, exception.InnerException);
				xml.WriteEndElement();
			}
		}

		protected static string CreateSourceString(object source)
		{
			int hashCode = source.GetHashCode();
			return string.Concat(source.GetType().ToString(), "/", hashCode.ToString(CultureInfo.CurrentCulture));
		}

		private void ExitOrUnloadEventHandler(object sender, EventArgs e)
		{
			this.ShutdownTracing();
		}

		private SourceLevels FixLevel(SourceLevels level)
		{
			if ((level & -16 & SourceLevels.Verbose) == SourceLevels.Off)
			{
				if ((level & -8 & SourceLevels.Information) == SourceLevels.Off)
				{
					if ((level & -4 & SourceLevels.Warning) != SourceLevels.Off)
					{
						level = level | SourceLevels.Warning;
					}
				}
				else
				{
					level = level | SourceLevels.Information;
				}
			}
			else
			{
				level = level | SourceLevels.Verbose;
			}
			if ((level & -2 & SourceLevels.Error) != SourceLevels.Off)
			{
				level = level | SourceLevels.Error;
			}
			if ((level & SourceLevels.Critical) != SourceLevels.Off)
			{
				level = level | SourceLevels.Critical;
			}
			if (level == SourceLevels.ActivityTracing)
			{
				level = SourceLevels.Off;
			}
			return level;
		}

		public abstract bool IsEnabled();

		[SecuritySafeCritical]
		protected void LogTraceFailure(string traceString, Exception exception)
		{
			TimeSpan timeSpan = TimeSpan.FromMinutes(10);
			try
			{
				lock (this.thisLock)
				{
					DateTime utcNow = DateTime.UtcNow;
					if (utcNow.Subtract(this.LastFailure) >= timeSpan)
					{
						this.LastFailure = DateTime.UtcNow;
						EventLogger eventLogger = EventLogger.UnsafeCreateEventLogger(this.eventSourceName, this);
						if (exception != null)
						{
							string[] str = new string[2];
							str[0] = traceString;
							str[1] = exception.ToString();
							eventLogger.UnsafeLogEvent(TraceEventType.Error, 4, -1073676183, false, str);
						}
						else
						{
							string[] strArrays = new string[1];
							strArrays[0] = traceString;
							eventLogger.UnsafeLogEvent(TraceEventType.Error, 4, -1073676184, false, strArrays);
						}
					}
				}
			}
			catch (Exception exception2)
			{
				Exception exception1 = exception2;
				if (Fx.IsFatal(exception1))
				{
					throw;
				}
			}
		}

		protected static string LookupSeverity(TraceEventType type)
		{
			string str;
			TraceEventType traceEventType = type;
			if (traceEventType > TraceEventType.Verbose)
			{
				if (traceEventType > TraceEventType.Stop)
				{
					if (traceEventType == TraceEventType.Suspend)
					{
						str = "Suspend";
					}
					else
					{
						if (traceEventType != TraceEventType.Transfer)
						{
							str = (object)type.ToString();
							return str;
						}
						str = "Transfer";
					}
				}
				else
				{
					if (traceEventType == TraceEventType.Start)
					{
						str = "Start";
					}
					else
					{
						if (traceEventType != TraceEventType.Stop)
						{
							str = (object)type.ToString();
							return str;
						}
						str = "Stop";
					}
				}
			}
			else
			{
				if (traceEventType == TraceEventType.Critical)
				{
					str = "Critical";
					return str;
				}
				else if (traceEventType == TraceEventType.Error)
				{
					str = "Error";
					return str;
				}
				else if (traceEventType == (TraceEventType.Critical | TraceEventType.Error))
				{
					str = (object)type.ToString();
					return str;
				}
				else if (traceEventType == TraceEventType.Warning)
				{
					str = "Warning";
					return str;
				}
				if (traceEventType == TraceEventType.Information)
				{
					str = "Information";
				}
				else
				{
					if (traceEventType != TraceEventType.Verbose)
					{
						str = (object)type.ToString();
						return str;
					}
					str = "Verbose";
				}
			}
			return str;
		}

		protected virtual void OnSetLevel(SourceLevels level)
		{
		}

		protected abstract void OnShutdownTracing();

		protected abstract void OnUnhandledException(Exception exception);

		[SecurityCritical]
		private void SetLevel(SourceLevels level)
		{
			bool flag;
			SourceLevels sourceLevel = this.FixLevel(level);
			this.level = sourceLevel;
			if (this.TraceSource != null)
			{
				this.haveListeners = this.TraceSource.Listeners.Count > 0;
				this.OnSetLevel(level);
				DiagnosticTraceBase diagnosticTraceBase = this;
				if (!this.HaveListeners)
				{
					flag = false;
				}
				else
				{
					flag = level != SourceLevels.Off;
				}
				diagnosticTraceBase.tracingEnabled = flag;
				this.TraceSource.Switch.Level = level;
			}
		}

		[SecurityCritical]
		private void SetLevelThreadSafe(SourceLevels level)
		{
			lock (this.thisLock)
			{
				this.SetLevel(level);
			}
		}

		[SecuritySafeCritical]
		protected void SetTraceSource(TraceSource traceSource)
		{
			if (traceSource != null)
			{
				DiagnosticTraceBase.UnsafeRemoveDefaultTraceListener(traceSource);
				this.traceSource = traceSource;
				this.haveListeners = this.traceSource.Listeners.Count > 0;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public virtual bool ShouldTrace(TraceEventLevel level)
		{
			return this.ShouldTraceToTraceSource(level);
		}

		public bool ShouldTrace(TraceEventType type)
		{
			if (!this.TracingEnabled || !this.HaveListeners || this.TraceSource == null)
			{
				return false;
			}
			else
			{
				return SourceLevels.Off != (type & (TraceEventType)this.Level);
			}
		}

		public bool ShouldTraceToTraceSource(TraceEventLevel level)
		{
			return this.ShouldTrace(TraceLevelHelper.GetTraceEventType(level));
		}

		private void ShutdownTracing()
		{
			if (!this.calledShutdown)
			{
				this.calledShutdown = true;
				try
				{
					this.OnShutdownTracing();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.IsFatal(exception))
					{
						this.LogTraceFailure(null, exception);
					}
					else
					{
						throw;
					}
				}
			}
		}

		protected static string StackTraceString(Exception exception)
		{
			string stackTrace = exception.StackTrace;
			if (string.IsNullOrEmpty(stackTrace))
			{
				StackTrace stackTrace1 = new StackTrace(false);
				StackFrame[] frames = stackTrace1.GetFrames();
				int num = 0;
				bool flag = false;
				StackFrame[] stackFrameArray = frames;
				for (int i = 0; i < (int)stackFrameArray.Length; i++)
				{
					StackFrame stackFrame = stackFrameArray[i];
					string name = stackFrame.GetMethod().Name;
					string str = name;
					string str1 = str;
					if (str != null)
					{
						if (str1 == "StackTraceString" || str1 == "AddExceptionToTraceString" || str1 == "BuildTrace" || str1 == "TraceEvent" || str1 == "TraceException" || str1 == "GetAdditionalPayload")
						{
							num++;
							goto Label0;
						}
					}
					if (!name.StartsWith("ThrowHelper", StringComparison.Ordinal))
					{
						flag = true;
					}
					else
					{
						num++;
					}
				Label0:
					if (flag)
					{
						break;
					}
				}
				stackTrace1 = new StackTrace(num, false);
				stackTrace = stackTrace1.ToString();
			}
			return stackTrace;
		}

		public abstract void TraceEventLogEvent(TraceEventType type, TraceRecord traceRecord);

		protected void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception exceptionObject = (Exception)args.ExceptionObject;
			this.OnUnhandledException(exceptionObject);
			this.ShutdownTracing();
		}

		[SecurityCritical]
		[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
		private static void UnsafeRemoveDefaultTraceListener(TraceSource traceSource)
		{
			traceSource.Listeners.Remove("Default");
		}

		public static string XmlEncode(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				int length = text.Length;
				StringBuilder stringBuilder = new StringBuilder(length + 8);
				for (int i = 0; i < length; i++)
				{
					char chr = text[i];
					char chr1 = chr;
					if (chr1 == '&')
					{
						stringBuilder.Append("&amp;");
					}
					else
					{
						switch (chr1)
						{
							case '<':
							{
								stringBuilder.Append("&lt;");
								break;
							}
							case '=':
							{
							Label0:
								stringBuilder.Append(chr);
								break;
							}
							case '>':
							{
								stringBuilder.Append("&gt;");
								break;
							}
							default:
							{
								goto Label0;
							}
						}
					}
				}
				return stringBuilder.ToString();
			}
			else
			{
				return text;
			}
		}
	}
}