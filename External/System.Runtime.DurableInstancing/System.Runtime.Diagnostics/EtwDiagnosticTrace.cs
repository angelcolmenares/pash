using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace System.Runtime.Diagnostics
{
	internal sealed class EtwDiagnosticTrace : DiagnosticTraceBase
	{
		public readonly static Guid ImmutableDefaultEtwProviderId;

		[SecurityCritical]
		private static Guid defaultEtwProviderId;

		private static Hashtable etwProviderCache;

		private static bool isVistaOrGreater;

		private static Func<string> traceAnnotation;

		[SecurityCritical]
		private EtwProvider etwProvider;

		private Guid etwProviderId;

		[SecurityCritical]
		private static EventDescriptor transferEventDescriptor;

		private const int WindowsVistaMajorNumber = 6;

		private const string EventSourceVersion = "4.0.0.0";

		private const ushort TracingEventLogCategory = 4;

		private const int MaxExceptionStringLength = 0x7000;

		private const int MaxExceptionDepth = 64;

		private const string DiagnosticTraceSource = "System.ServiceModel.Diagnostics";

		private const int XmlBracketsLength = 5;

		public static Guid DefaultEtwProviderId
		{
			[SecuritySafeCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return EtwDiagnosticTrace.defaultEtwProviderId;
			}
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				EtwDiagnosticTrace.defaultEtwProviderId = value;
			}
		}

		public EtwProvider EtwProvider
		{
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.etwProvider;
			}
		}

		private bool EtwTracingEnabled
		{
			[SecuritySafeCritical]
			get
			{
				return this.etwProvider != null;
			}
		}

		public bool IsEnd2EndActivityTracingEnabled
		{
			[SecuritySafeCritical]
			get
			{
				if (!this.IsEtwProviderEnabled)
				{
					return false;
				}
				else
				{
					return this.EtwProvider.IsEnd2EndActivityTracingEnabled;
				}
			}
		}

		public bool IsEtwProviderEnabled
		{
			[SecuritySafeCritical]
			get
			{
				if (!this.EtwTracingEnabled)
				{
					return false;
				}
				else
				{
					return this.etwProvider.IsEnabled();
				}
			}
		}

		public Action RefreshState
		{
			[SecuritySafeCritical]
			get
			{
				return this.EtwProvider.ControllerCallBack;
			}
			[SecuritySafeCritical]
			set
			{
				this.EtwProvider.ControllerCallBack = value;
			}
		}

		[SecurityCritical]
		static EtwDiagnosticTrace()
		{
			EtwDiagnosticTrace.ImmutableDefaultEtwProviderId = new Guid("{c651f5f6-1c0d-492e-8ae1-b4efd7c9d503}");
			EtwDiagnosticTrace.defaultEtwProviderId = EtwDiagnosticTrace.ImmutableDefaultEtwProviderId;
			EtwDiagnosticTrace.etwProviderCache = new Hashtable();
			EtwDiagnosticTrace.isVistaOrGreater = Environment.OSVersion.Version.Major >= 6;
			EtwDiagnosticTrace.transferEventDescriptor = new EventDescriptor(0x1f3, 0, 18, 0, 0, 0, 0x20000000001a0065L);
			if (!PartialTrustHelpers.HasEtwPermissions())
			{
				EtwDiagnosticTrace.defaultEtwProviderId = Guid.Empty;
			}
		}

		[SecurityCritical]
		public EtwDiagnosticTrace(string traceSourceName, Guid etwProviderId) : base(traceSourceName)
		{
			try
			{
				this.TraceSourceName = traceSourceName;
				base.EventSourceName = string.Concat(this.TraceSourceName, " ", "4.0.0.0");
				this.CreateTraceSource();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (!Fx.IsFatal(exception))
				{
					EventLogger eventLogger = new EventLogger(base.EventSourceName, null);
					string[] str = new string[1];
					str[0] = exception.ToString();
					eventLogger.LogEvent(TraceEventType.Error, 4, -1073676188, false, str);
				}
				else
				{
					throw;
				}
			}
			try
			{
				this.CreateEtwProvider(etwProviderId);
			}
			catch (Exception exception3)
			{
				Exception exception2 = exception3;
				if (!Fx.IsFatal(exception2))
				{
					this.etwProvider = null;
					EventLogger eventLogger1 = new EventLogger(base.EventSourceName, null);
					string[] strArrays = new string[1];
					strArrays[0] = exception2.ToString();
					eventLogger1.LogEvent(TraceEventType.Error, 4, -1073676188, false, strArrays);
				}
				else
				{
					throw;
				}
			}
			if (base.TracingEnabled || this.EtwTracingEnabled)
			{
				base.AddDomainEventHandlersForCleanup();
			}
		}

		[SecurityCritical]
		private static string BuildTrace(ref EventDescriptor eventDescriptor, string description, TracePayload payload, string msdnTraceCode)
		{
			string str;
			StringBuilder stringBuilder = EtwDiagnosticTrace.StringBuilderPool.Take();
			try
			{
				using (StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.CurrentCulture))
				{
					using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
					{
						xmlTextWriter.WriteStartElement("TraceRecord");
						xmlTextWriter.WriteAttributeString("xmlns", "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord");
						xmlTextWriter.WriteAttributeString("Severity", TraceLevelHelper.LookupSeverity((TraceEventLevel)eventDescriptor.Level, (TraceEventOpcode)eventDescriptor.Opcode));
						xmlTextWriter.WriteAttributeString("Channel", EtwDiagnosticTrace.LookupChannel((TraceChannel)eventDescriptor.Channel));
						xmlTextWriter.WriteElementString("TraceIdentifier", msdnTraceCode);
						xmlTextWriter.WriteElementString("Description", description);
						xmlTextWriter.WriteElementString("AppDomain", payload.AppDomainFriendlyName);
						if (!string.IsNullOrEmpty(payload.EventSource))
						{
							xmlTextWriter.WriteElementString("Source", payload.EventSource);
						}
						if (!string.IsNullOrEmpty(payload.ExtendedData))
						{
							xmlTextWriter.WriteRaw(payload.ExtendedData);
						}
						if (!string.IsNullOrEmpty(payload.SerializedException))
						{
							xmlTextWriter.WriteRaw(payload.SerializedException);
						}
						xmlTextWriter.WriteEndElement();
						xmlTextWriter.Flush();
						stringWriter.Flush();
						str = stringBuilder.ToString();
					}
				}
			}
			finally
			{
				EtwDiagnosticTrace.StringBuilderPool.Return(stringBuilder);
			}
			return str;
		}

		[SecurityCritical]
		private void CreateEtwProvider(Guid etwProviderId)
		{
			if (etwProviderId != Guid.Empty && EtwDiagnosticTrace.isVistaOrGreater)
			{
				this.etwProvider = (EtwProvider)EtwDiagnosticTrace.etwProviderCache[(object)etwProviderId];
				if (this.etwProvider == null)
				{
					lock (EtwDiagnosticTrace.etwProviderCache)
					{
						this.etwProvider = (EtwProvider)EtwDiagnosticTrace.etwProviderCache[(object)etwProviderId];
						if (this.etwProvider == null)
						{
							this.etwProvider = new EtwProvider(etwProviderId);
							EtwDiagnosticTrace.etwProviderCache.Add(etwProviderId, this.etwProvider);
						}
					}
				}
				this.etwProviderId = etwProviderId;
			}
		}

		[SecuritySafeCritical]
		private void CreateTraceSource()
		{
			if (!string.IsNullOrEmpty(this.TraceSourceName))
			{
				base.SetTraceSource(new DiagnosticTraceSource(this.TraceSourceName));
			}
		}

		[SecuritySafeCritical]
		public void Event(int eventId, TraceEventLevel traceEventLevel, TraceChannel channel, string description)
		{
			if (base.TracingEnabled)
			{
				EventDescriptor eventDescriptor = EtwDiagnosticTrace.GetEventDescriptor(eventId, channel, traceEventLevel);
				this.Event(ref eventDescriptor, description);
			}
		}

		[SecurityCritical]
		public void Event(ref EventDescriptor eventDescriptor, string description)
		{
			if (base.TracingEnabled)
			{
				TracePayload serializedPayload = this.GetSerializedPayload(null, null, null);
				this.WriteTraceSource(ref eventDescriptor, description, serializedPayload);
			}
		}

		internal static string ExceptionToTraceString(Exception exception, int maxTraceStringLength)
		{
			string str;
			StringBuilder stringBuilder = EtwDiagnosticTrace.StringBuilderPool.Take();
			try
			{
				using (StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.CurrentCulture))
				{
					using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
					{
						EtwDiagnosticTrace.WriteExceptionToTraceString(xmlTextWriter, exception, maxTraceStringLength, 64);
						xmlTextWriter.Flush();
						stringWriter.Flush();
						str = stringBuilder.ToString();
					}
				}
			}
			finally
			{
				EtwDiagnosticTrace.StringBuilderPool.Return(stringBuilder);
			}
			return str;
		}

		[SecurityCritical]
		private static void GenerateLegacyTraceCode(ref EventDescriptor eventDescriptor, out string msdnTraceCode, out int legacyEventId)
		{
			int eventId;
			int num = eventDescriptor.EventId;
			switch (num)
			{
				case 0xe031:
				{
					msdnTraceCode = EtwDiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "AppDomainUnload");
					legacyEventId = 0x20001;
					return;
				}
				case 0xe032:
				case 0xe03c:
				case 0xe03d:
				case 0xe03e:
				{
					msdnTraceCode = EtwDiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "TraceHandledException");
					legacyEventId = 0x20004;
					return;
				}
				case 0xe033:
				case 0xe036:
				case 0xe037:
				case 0xe038:
				case 0xe039:
				case 0xe03a:
				case 0xe03b:
				{
					eventId = eventDescriptor.EventId;
					msdnTraceCode = eventId.ToString(CultureInfo.InvariantCulture);
					legacyEventId = eventDescriptor.EventId;
					return;
				}
				case 0xe034:
				case 0xe03f:
				{
					msdnTraceCode = EtwDiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "ThrowingException");
					legacyEventId = 0x20003;
					return;
				}
				case 0xe035:
				{
					msdnTraceCode = EtwDiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "UnhandledException");
					legacyEventId = 0x20005;
					return;
				}
				default:
				{
					eventId = eventDescriptor.EventId;
					msdnTraceCode = eventId.ToString(CultureInfo.InvariantCulture);
					legacyEventId = eventDescriptor.EventId;
					return;
				}
			}
		}

		private static string GenerateMsdnTraceCode(string traceSource, string traceCodeString)
		{
			object[] name = new object[3];
			name[0] = CultureInfo.CurrentCulture.Name;
			name[1] = traceSource;
			name[2] = traceCodeString;
			return string.Format(CultureInfo.InvariantCulture, "http://msdn.microsoft.com/{0}/library/{1}.{2}.aspx", name);
		}

		[SecurityCritical]
		private static EventDescriptor GetEventDescriptor(int eventId, TraceChannel channel, TraceEventLevel traceEventLevel)
		{
			long num = (long)0;
			if (channel != TraceChannel.Admin)
			{
				if (channel != TraceChannel.Operational)
				{
					if (channel != TraceChannel.Analytic)
					{
						if (channel != TraceChannel.Debug)
						{
							if (channel == TraceChannel.Perf)
							{
								num = num | 0x800000000000000L;
							}
						}
						else
						{
							num = num | 0x100000000000000L;
						}
					}
					else
					{
						num = num | 0x2000000000000000L;
					}
				}
				else
				{
					num = num | 0x4000000000000000L;
				}
			}
			else
			{
				num = num | -9223372036854775808L;
			}
			return new EventDescriptor(eventId, 0, (byte)channel, (byte)traceEventLevel, 0, 0, num);
		}

		private static string GetExceptionData(Exception exception)
		{
			string str;
			StringBuilder stringBuilder = EtwDiagnosticTrace.StringBuilderPool.Take();
			try
			{
				using (StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.CurrentCulture))
				{
					using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
					{
						xmlTextWriter.WriteStartElement("DataItems");
						foreach (object key in exception.Data.Keys)
						{
							xmlTextWriter.WriteStartElement("Data");
							xmlTextWriter.WriteElementString("Key", DiagnosticTraceBase.XmlEncode(key.ToString()));
							xmlTextWriter.WriteElementString("Value", DiagnosticTraceBase.XmlEncode(exception.Data[key].ToString()));
							xmlTextWriter.WriteEndElement();
						}
						xmlTextWriter.WriteEndElement();
						xmlTextWriter.Flush();
						stringWriter.Flush();
						str = stringBuilder.ToString();
					}
				}
			}
			finally
			{
				EtwDiagnosticTrace.StringBuilderPool.Return(stringBuilder);
			}
			return str;
		}

		private static string GetInnerException(Exception exception, int remainingLength, int remainingAllowedRecursionDepth)
		{
			string str;
			if (remainingAllowedRecursionDepth >= 1)
			{
				StringBuilder stringBuilder = EtwDiagnosticTrace.StringBuilderPool.Take();
				try
				{
					using (StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.CurrentCulture))
					{
						using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
						{
							if (EtwDiagnosticTrace.WriteStartElement(xmlTextWriter, "InnerException", ref remainingLength))
							{
								EtwDiagnosticTrace.WriteExceptionToTraceString(xmlTextWriter, exception.InnerException, remainingLength, remainingAllowedRecursionDepth);
								xmlTextWriter.WriteEndElement();
								xmlTextWriter.Flush();
								stringWriter.Flush();
								str = stringBuilder.ToString();
							}
							else
							{
								str = null;
							}
						}
					}
				}
				finally
				{
					EtwDiagnosticTrace.StringBuilderPool.Return(stringBuilder);
				}
				return str;
			}
			else
			{
				return null;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TracePayload GetSerializedPayload(object source, TraceRecord traceRecord, Exception exception)
		{
			return this.GetSerializedPayload(source, traceRecord, exception, false);
		}

		public TracePayload GetSerializedPayload(object source, TraceRecord traceRecord, Exception exception, bool getServiceReference)
		{
			string str = null;
			string str1 = null;
			string traceString = null;
			if (source != null)
			{
				str = DiagnosticTraceBase.CreateSourceString(source);
			}
			if (traceRecord != null)
			{
				StringBuilder stringBuilder = EtwDiagnosticTrace.StringBuilderPool.Take();
				try
				{
					using (StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.CurrentCulture))
					{
						using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
						{
							xmlTextWriter.WriteStartElement("ExtendedData");
							traceRecord.WriteTo(xmlTextWriter);
							xmlTextWriter.WriteEndElement();
							xmlTextWriter.Flush();
							stringWriter.Flush();
							str1 = stringBuilder.ToString();
						}
					}
				}
				finally
				{
					EtwDiagnosticTrace.StringBuilderPool.Return(stringBuilder);
				}
			}
			if (exception != null)
			{
				traceString = EtwDiagnosticTrace.ExceptionToTraceString(exception, 0x7000);
			}
			if (!getServiceReference || EtwDiagnosticTrace.traceAnnotation == null)
			{
				return new TracePayload(traceString, str, DiagnosticTraceBase.AppDomainFriendlyName, str1, string.Empty);
			}
			else
			{
				return new TracePayload(traceString, str, DiagnosticTraceBase.AppDomainFriendlyName, str1, EtwDiagnosticTrace.traceAnnotation());
			}
		}

		public override bool IsEnabled()
		{
			if (TraceCore.TraceCodeEventLogCriticalIsEnabled(this) || TraceCore.TraceCodeEventLogVerboseIsEnabled(this) || TraceCore.TraceCodeEventLogInfoIsEnabled(this) || TraceCore.TraceCodeEventLogWarningIsEnabled(this))
			{
				return true;
			}
			else
			{
				return TraceCore.TraceCodeEventLogErrorIsEnabled(this);
			}
		}

		[SecuritySafeCritical]
		public bool IsEtwEventEnabled(ref EventDescriptor eventDescriptor)
		{
			if (!this.EtwTracingEnabled)
			{
				return false;
			}
			else
			{
				return this.etwProvider.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords);
			}
		}

		private static string LookupChannel(TraceChannel traceChannel)
		{
			string str;
			TraceChannel traceChannel1 = traceChannel;
			if (traceChannel1 == TraceChannel.Application)
			{
				str = "Application";
			}
			else
			{
				switch (traceChannel1)
				{
					case TraceChannel.Admin:
					{
						str = "Admin";
						break;
					}
					case TraceChannel.Operational:
					{
						str = "Operational";
						break;
					}
					case TraceChannel.Analytic:
					{
						str = "Analytic";
						break;
					}
					case TraceChannel.Debug:
					{
						str = "Debug";
						break;
					}
					case TraceChannel.Perf:
					{
						str = "Perf";
						break;
					}
					default:
					{
						str = traceChannel.ToString();
						break;
					}
				}
			}
			return str;
		}

		protected override void OnShutdownTracing()
		{
			this.ShutdownTraceSource();
			this.ShutdownEtwProvider();
		}

		protected override void OnUnhandledException(Exception exception)
		{
			string str;
			EtwDiagnosticTrace etwDiagnosticTrace = this;
			if (exception != null)
			{
				str = exception.ToString();
			}
			else
			{
				str = string.Empty;
			}
			TraceCore.UnhandledException(etwDiagnosticTrace, str, exception);
		}

		public void SetAndTraceTransfer(Guid newId, bool emitTransfer)
		{
			if (emitTransfer)
			{
				this.TraceTransfer(newId);
			}
			DiagnosticTraceBase.ActivityId = newId;
		}

		public void SetAnnotation(Func<string> annotation)
		{
			EtwDiagnosticTrace.traceAnnotation = annotation;
		}

		[SecuritySafeCritical]
		public void SetEnd2EndActivityTracingEnabled(bool isEnd2EndTracingEnabled)
		{
			this.EtwProvider.SetEnd2EndActivityTracingEnabled(isEnd2EndTracingEnabled);
		}

		public override bool ShouldTrace(TraceEventLevel level)
		{
			if (base.ShouldTrace(level))
			{
				return true;
			}
			else
			{
				return this.ShouldTraceToEtw(level);
			}
		}

		[SecuritySafeCritical]
		public bool ShouldTraceToEtw(TraceEventLevel level)
		{
			if (this.EtwProvider == null)
			{
				return false;
			}
			else
			{
				return this.EtwProvider.IsEnabled((byte)level, (long)0);
			}
		}

		[SecuritySafeCritical]
		private void ShutdownEtwProvider()
		{
			try
			{
				if (this.etwProvider != null)
				{
					this.etwProvider.Dispose();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (!Fx.IsFatal(exception))
				{
					base.LogTraceFailure(null, exception);
				}
				else
				{
					throw;
				}
			}
		}

		private void ShutdownTraceSource()
		{
			try
			{
				if (TraceCore.AppDomainUnloadIsEnabled(this))
				{
					int processId = DiagnosticTraceBase.ProcessId;
					TraceCore.AppDomainUnload(this, AppDomain.CurrentDomain.FriendlyName, DiagnosticTraceBase.ProcessName, processId.ToString(CultureInfo.CurrentCulture));
				}
				base.TraceSource.Flush();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (!Fx.IsFatal(exception))
				{
					base.LogTraceFailure(null, exception);
				}
				else
				{
					throw;
				}
			}
		}

		public override void TraceEventLogEvent(TraceEventType type, TraceRecord traceRecord)
		{
			TraceEventType traceEventType = type;
			switch (traceEventType)
			{
				case TraceEventType.Critical:
				{
					TraceCore.TraceCodeEventLogCritical(this, traceRecord);
					return;
				}
				case TraceEventType.Error:
				{
					TraceCore.TraceCodeEventLogError(this, traceRecord);
					return;
				}
				case TraceEventType.Critical | TraceEventType.Error:
				{
					return;
				}
				case TraceEventType.Warning:
				{
					TraceCore.TraceCodeEventLogWarning(this, traceRecord);
					return;
				}
				default:
				{
					if (traceEventType == TraceEventType.Information)
					{
						TraceCore.TraceCodeEventLogInfo(this, traceRecord);
						return;
					}
					else
					{
						if (traceEventType != TraceEventType.Verbose)
						{
							break;
						}
						TraceCore.TraceCodeEventLogVerbose(this, traceRecord);
						return;
					}
				}
			}
		}

		[SecuritySafeCritical]
		public void TraceTransfer(Guid newId)
		{
			string empty;
			Guid activityId = DiagnosticTraceBase.ActivityId;
			if (newId != activityId)
			{
				try
				{
					if (base.HaveListeners)
					{
						base.TraceSource.TraceTransfer(0, null, newId);
					}
					if (this.IsEtwEventEnabled(ref EtwDiagnosticTrace.transferEventDescriptor))
					{
						EtwProvider etwProvider = this.etwProvider;
						EventDescriptor eventDescriptorPointer = EtwDiagnosticTrace.transferEventDescriptor;
						EventTraceActivity eventTraceActivity = new EventTraceActivity(activityId, false);
						Guid guid = newId;
						if (EtwDiagnosticTrace.traceAnnotation == null)
						{
							empty = string.Empty;
						}
						else
						{
							empty = EtwDiagnosticTrace.traceAnnotation();
						}
						etwProvider.WriteTransferEvent(eventDescriptorPointer, eventTraceActivity, guid, empty, DiagnosticTraceBase.AppDomainFriendlyName);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.IsFatal(exception))
					{
						base.LogTraceFailure(null, exception);
					}
					else
					{
						throw;
					}
				}
			}
		}

		private static void WriteExceptionToTraceString(XmlTextWriter xml, Exception exception, int remainingLength, int remainingAllowedRecursionDepth)
		{
			if (remainingAllowedRecursionDepth >= 1)
			{
				if (EtwDiagnosticTrace.WriteStartElement(xml, "Exception", ref remainingLength))
				{
					try
					{
						List<Tuple<string, string>> tuples = new List<Tuple<string, string>>();
						tuples.Add(new Tuple<string, string>("ExceptionType", DiagnosticTraceBase.XmlEncode(exception.GetType().AssemblyQualifiedName)));
						tuples.Add(new Tuple<string, string>("Message", DiagnosticTraceBase.XmlEncode(exception.Message)));
						tuples.Add(new Tuple<string, string>("StackTrace", DiagnosticTraceBase.XmlEncode(DiagnosticTraceBase.StackTraceString(exception))));
						tuples.Add(new Tuple<string, string>("ExceptionString", DiagnosticTraceBase.XmlEncode(exception.ToString())));
						IList<Tuple<string, string>> tuples1 = tuples;
						Win32Exception win32Exception = exception as Win32Exception;
						if (win32Exception != null)
						{
							int nativeErrorCode = win32Exception.NativeErrorCode;
							tuples1.Add(new Tuple<string, string>("NativeErrorCode", nativeErrorCode.ToString("X", CultureInfo.InvariantCulture)));
						}
						foreach (Tuple<string, string> tuple in tuples1)
						{
							if (EtwDiagnosticTrace.WriteXmlElementString(xml, tuple.Item1, tuple.Item2, ref remainingLength))
							{
								continue;
							}
							return;
						}
						if (exception.Data != null && exception.Data.Count > 0)
						{
							string exceptionData = EtwDiagnosticTrace.GetExceptionData(exception);
							if (exceptionData.Length < remainingLength)
							{
								xml.WriteRaw(exceptionData);
								remainingLength = remainingLength - exceptionData.Length;
							}
						}
						if (exception.InnerException != null)
						{
							string innerException = EtwDiagnosticTrace.GetInnerException(exception, remainingLength, remainingAllowedRecursionDepth - 1);
							if (!string.IsNullOrEmpty(innerException) && innerException.Length < remainingLength)
							{
								xml.WriteRaw(innerException);
							}
						}
					}
					finally
					{
						xml.WriteEndElement();
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

		private static bool WriteStartElement(XmlTextWriter xml, string localName, ref int remainingLength)
		{
			int length = localName.Length * 2 + 5;
			if (length > remainingLength)
			{
				return false;
			}
			else
			{
				xml.WriteStartElement(localName);
				remainingLength = remainingLength - length;
				return true;
			}
		}

		[SecurityCritical]
		public void WriteTraceSource(ref EventDescriptor eventDescriptor, string description, TracePayload payload)
		{
			string str = null;
			int num = 0;
			string empty;
			if (base.TracingEnabled)
			{
				XPathNavigator xPathNavigator = null;
				try
				{
					EtwDiagnosticTrace.GenerateLegacyTraceCode(ref eventDescriptor, out str, out num);
					string str1 = EtwDiagnosticTrace.BuildTrace(ref eventDescriptor, description, payload, str);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(str1);
					xPathNavigator = xmlDocument.CreateNavigator();
					base.TraceSource.TraceData(TraceLevelHelper.GetTraceEventType(eventDescriptor.Level, eventDescriptor.Opcode), num, xPathNavigator);
					if (base.CalledShutdown)
					{
						base.TraceSource.Flush();
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.IsFatal(exception))
					{
						EtwDiagnosticTrace etwDiagnosticTrace = this;
						if (xPathNavigator == null)
						{
							empty = string.Empty;
						}
						else
						{
							empty = xPathNavigator.ToString();
						}
						etwDiagnosticTrace.LogTraceFailure(empty, exception);
					}
					else
					{
						throw;
					}
				}
			}
		}

		private static bool WriteXmlElementString(XmlTextWriter xml, string localName, string value, ref int remainingLength)
		{
			int length = localName.Length * 2 + 5 + value.Length;
			if (length > remainingLength)
			{
				return false;
			}
			else
			{
				xml.WriteElementString(localName, value);
				remainingLength = remainingLength - length;
				return true;
			}
		}

		private static class EventIdsWithMsdnTraceCode
		{
			public const int AppDomainUnload = 0xe031;

			public const int ThrowingExceptionWarning = 0xe034;

			public const int ThrowingExceptionVerbose = 0xe03f;

			public const int HandledExceptionInfo = 0xe032;

			public const int HandledExceptionWarning = 0xe03c;

			public const int HandledExceptionError = 0xe03d;

			public const int HandledExceptionVerbose = 0xe03e;

			public const int UnhandledException = 0xe035;

		}

		private static class LegacyTraceEventIds
		{
			public const int Diagnostics = 0x20000;

			public const int AppDomainUnload = 0x20001;

			public const int EventLog = 0x20002;

			public const int ThrowingException = 0x20003;

			public const int TraceHandledException = 0x20004;

			public const int UnhandledException = 0x20005;

		}

		private static class StringBuilderPool
		{
			private const int maxPooledStringBuilders = 64;

			private readonly static ConcurrentQueue<StringBuilder> freeStringBuilders;

			static StringBuilderPool()
			{
				EtwDiagnosticTrace.StringBuilderPool.freeStringBuilders = new ConcurrentQueue<StringBuilder>();
			}

			public static void Return(StringBuilder sb)
			{
				if (EtwDiagnosticTrace.StringBuilderPool.freeStringBuilders.Count <= 64)
				{
					sb.Clear();
					EtwDiagnosticTrace.StringBuilderPool.freeStringBuilders.Enqueue(sb);
				}
			}

			public static StringBuilder Take()
			{
				StringBuilder stringBuilder = null;
				if (!EtwDiagnosticTrace.StringBuilderPool.freeStringBuilders.TryDequeue(out stringBuilder))
				{
					return new StringBuilder();
				}
				else
				{
					return stringBuilder;
				}
			}
		}

		private static class TraceCodes
		{
			public const string AppDomainUnload = "AppDomainUnload";

			public const string TraceHandledException = "TraceHandledException";

			public const string ThrowingException = "ThrowingException";

			public const string UnhandledException = "UnhandledException";

		}
	}
}