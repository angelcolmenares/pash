using System;
using System.Globalization;
using System.Resources;
using System.Runtime.Diagnostics;
using System.Security;
using System.Threading;

namespace System.Runtime
{
	internal class TraceCore
	{
		private static ResourceManager resourceManager;

		private static CultureInfo resourceCulture;

		[SecurityCritical]
		private static EventDescriptor[] eventDescriptors;

		private static object syncLock;

		private static volatile bool eventDescriptorsCreated;

		internal static CultureInfo Culture
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return TraceCore.resourceCulture;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				TraceCore.resourceCulture = value;
			}
		}

		private static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(TraceCore.resourceManager, null))
				{
					TraceCore.resourceManager = new ResourceManager("System.Runtime.TraceCore", typeof(TraceCore).Assembly);
				}
				return TraceCore.resourceManager;
			}
		}

		static TraceCore()
		{
			TraceCore.syncLock = new object();
		}

		private TraceCore()
		{
		}

		internal static void ActionItemCallbackInvoked(EtwDiagnosticTrace trace, EventTraceActivity eventTraceActivity)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, null);
			if (TraceCore.IsEtwEventEnabled(trace, 14))
			{
				TraceCore.WriteEtwEvent(trace, 14, eventTraceActivity, serializedPayload.AppDomainFriendlyName);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal static bool ActionItemCallbackInvokedIsEnabled(EtwDiagnosticTrace trace)
		{
			return TraceCore.IsEtwEventEnabled(trace, 14);
		}

		internal static void ActionItemScheduled(EtwDiagnosticTrace trace, EventTraceActivity eventTraceActivity)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, null);
			if (TraceCore.IsEtwEventEnabled(trace, 13))
			{
				TraceCore.WriteEtwEvent(trace, 13, eventTraceActivity, serializedPayload.AppDomainFriendlyName);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal static bool ActionItemScheduledIsEnabled(EtwDiagnosticTrace trace)
		{
			return TraceCore.IsEtwEventEnabled(trace, 13);
		}

		internal static void AppDomainUnload(EtwDiagnosticTrace trace, string appdomainName, string processName, string processId)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, null);
			if (TraceCore.IsEtwEventEnabled(trace, 0))
			{
				TraceCore.WriteEtwEvent(trace, 0, null, appdomainName, processName, processId, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
			{
				object[] objArray = new object[3];
				objArray[0] = appdomainName;
				objArray[1] = processName;
				objArray[2] = processId;
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("AppDomainUnload", TraceCore.Culture), objArray);
				TraceCore.WriteTraceSource(trace, 0, str, serializedPayload);
			}
		}

		internal static bool AppDomainUnloadIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Informational))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 0);
			}
		}

		internal static void BufferPoolAllocation(EtwDiagnosticTrace trace, int Size)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, null);
			if (TraceCore.IsEtwEventEnabled(trace, 11))
			{
				TraceCore.WriteEtwEvent(trace, 11, null, Size, serializedPayload.AppDomainFriendlyName);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal static bool BufferPoolAllocationIsEnabled(EtwDiagnosticTrace trace)
		{
			return TraceCore.IsEtwEventEnabled(trace, 11);
		}

		internal static void BufferPoolChangeQuota(EtwDiagnosticTrace trace, int PoolSize, int Delta)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, null);
			if (TraceCore.IsEtwEventEnabled(trace, 12))
			{
				TraceCore.WriteEtwEvent(trace, 12, null, PoolSize, Delta, serializedPayload.AppDomainFriendlyName);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal static bool BufferPoolChangeQuotaIsEnabled(EtwDiagnosticTrace trace)
		{
			return TraceCore.IsEtwEventEnabled(trace, 12);
		}

		[SecuritySafeCritical]
		private static void CreateEventDescriptors()
		{
			EventDescriptor[] eventDescriptor = new EventDescriptor[21];
			eventDescriptor[0] = new EventDescriptor(0xe031, 0, 19, 4, 0, 0, 0x1000000000010000L);
			eventDescriptor[1] = new EventDescriptor(0xe032, 0, 18, 4, 0, 0, 0x2000000000010000L);
			eventDescriptor[2] = new EventDescriptor(0xe033, 0, 18, 2, 0, 0, 0x2000000000010000L);
			eventDescriptor[3] = new EventDescriptor(0xe034, 0, 18, 3, 0, 0, 0x2000000000010000L);
			eventDescriptor[4] = new EventDescriptor(0xe035, 0, 17, 1, 0, 0, 0x4000000000010000L);
			eventDescriptor[5] = new EventDescriptor(0xe037, 0, 19, 1, 0, 0, 0x1000000000010000L);
			eventDescriptor[6] = new EventDescriptor(0xe038, 0, 19, 2, 0, 0, 0x1000000000010000L);
			eventDescriptor[7] = new EventDescriptor(0xe039, 0, 19, 4, 0, 0, 0x1000000000010000L);
			eventDescriptor[8] = new EventDescriptor(0xe03a, 0, 19, 5, 0, 0, 0x1000000000010000L);
			eventDescriptor[9] = new EventDescriptor(0xe03b, 0, 19, 3, 0, 0, 0x1000000000010000L);
			eventDescriptor[10] = new EventDescriptor(0xe03c, 0, 18, 3, 0, 0, 0x2000000000010000L);
			eventDescriptor[11] = new EventDescriptor(131, 0, 19, 5, 12, 0x9cd, 0x1000000000010000L);
			eventDescriptor[12] = new EventDescriptor(132, 0, 19, 5, 13, 0x9cd, 0x1000000000010000L);
			eventDescriptor[13] = new EventDescriptor(133, 0, 19, 5, 1, 0xa21, 0x1000000000200000L);
			eventDescriptor[14] = new EventDescriptor(134, 0, 19, 5, 2, 0xa21, 0x1000000000200000L);
			eventDescriptor[15] = new EventDescriptor(0xe03d, 0, 17, 2, 0, 0, 0x4000000000010000L);
			eventDescriptor[16] = new EventDescriptor(0xe03e, 0, 18, 5, 0, 0, 0x2000000000010000L);
			eventDescriptor[17] = new EventDescriptor(0xe040, 0, 17, 1, 0, 0, 0x4000000000010000L);
			eventDescriptor[18] = new EventDescriptor(0xe042, 0, 18, 3, 0, 0, 0x2000000000010000L);
			eventDescriptor[19] = new EventDescriptor(0xe041, 0, 18, 5, 0, 0, 0x2000000000010000L);
			eventDescriptor[20] = new EventDescriptor(0xe03f, 0, 18, 5, 0, 0, 0x2000000000010000L);
			TraceCore.eventDescriptors = eventDescriptor;
		}

		private static void EnsureEventDescriptors()
		{
			if (TraceCore.eventDescriptorsCreated == null)
			{
				Monitor.Enter(TraceCore.syncLock);
				try
				{
					if (TraceCore.eventDescriptorsCreated == null)
					{
						TraceCore.CreateEventDescriptors();
						TraceCore.eventDescriptorsCreated = true;
					}
				}
				finally
				{
					Monitor.Exit(TraceCore.syncLock);
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal static void EtwUnhandledException(EtwDiagnosticTrace trace, string param0, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 17))
			{
				TraceCore.WriteEtwEvent(trace, 17, null, param0, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal static bool EtwUnhandledExceptionIsEnabled(EtwDiagnosticTrace trace)
		{
			return TraceCore.IsEtwEventEnabled(trace, 17);
		}

		internal static void HandledException(EtwDiagnosticTrace trace, string param0, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 1))
			{
				TraceCore.WriteEtwEvent(trace, 1, null, param0, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
			{
				object[] objArray = new object[1];
				objArray[0] = param0;
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("HandledException", TraceCore.Culture), objArray);
				TraceCore.WriteTraceSource(trace, 1, str, serializedPayload);
			}
		}

		internal static void HandledExceptionError(EtwDiagnosticTrace trace, string param0, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 15))
			{
				TraceCore.WriteEtwEvent(trace, 15, null, param0, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Error))
			{
				object[] objArray = new object[1];
				objArray[0] = param0;
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("HandledExceptionError", TraceCore.Culture), objArray);
				TraceCore.WriteTraceSource(trace, 15, str, serializedPayload);
			}
		}

		internal static bool HandledExceptionErrorIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Error))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 15);
			}
		}

		internal static bool HandledExceptionIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Informational))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 1);
			}
		}

		internal static void HandledExceptionVerbose(EtwDiagnosticTrace trace, string param0, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 16))
			{
				TraceCore.WriteEtwEvent(trace, 16, null, param0, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Verbose))
			{
				object[] objArray = new object[1];
				objArray[0] = param0;
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("HandledExceptionVerbose", TraceCore.Culture), objArray);
				TraceCore.WriteTraceSource(trace, 16, str, serializedPayload);
			}
		}

		internal static bool HandledExceptionVerboseIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Verbose))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 16);
			}
		}

		internal static void HandledExceptionWarning(EtwDiagnosticTrace trace, string param0, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 10))
			{
				TraceCore.WriteEtwEvent(trace, 10, null, param0, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Warning))
			{
				object[] objArray = new object[1];
				objArray[0] = param0;
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("HandledExceptionWarning", TraceCore.Culture), objArray);
				TraceCore.WriteTraceSource(trace, 10, str, serializedPayload);
			}
		}

		internal static bool HandledExceptionWarningIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Warning))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 10);
			}
		}

		[SecuritySafeCritical]
		private static bool IsEtwEventEnabled(EtwDiagnosticTrace trace, int eventIndex)
		{
			if (!trace.IsEtwProviderEnabled)
			{
				return false;
			}
			else
			{
				TraceCore.EnsureEventDescriptors();
				return trace.IsEtwEventEnabled(ref TraceCore.eventDescriptors[eventIndex]);
			}
		}

		internal static void ShipAssertExceptionMessage(EtwDiagnosticTrace trace, string param0)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, null);
			if (TraceCore.IsEtwEventEnabled(trace, 2))
			{
				TraceCore.WriteEtwEvent(trace, 2, null, param0, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Error))
			{
				object[] objArray = new object[1];
				objArray[0] = param0;
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("ShipAssertExceptionMessage", TraceCore.Culture), objArray);
				TraceCore.WriteTraceSource(trace, 2, str, serializedPayload);
			}
		}

		internal static bool ShipAssertExceptionMessageIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Error))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 2);
			}
		}

		internal static void ThrowingEtwException(EtwDiagnosticTrace trace, string param0, string param1, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 18))
			{
				TraceCore.WriteEtwEvent(trace, 18, null, param0, param1, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal static bool ThrowingEtwExceptionIsEnabled(EtwDiagnosticTrace trace)
		{
			return TraceCore.IsEtwEventEnabled(trace, 18);
		}

		internal static void ThrowingEtwExceptionVerbose(EtwDiagnosticTrace trace, string param0, string param1, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 19))
			{
				TraceCore.WriteEtwEvent(trace, 19, null, param0, param1, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal static bool ThrowingEtwExceptionVerboseIsEnabled(EtwDiagnosticTrace trace)
		{
			return TraceCore.IsEtwEventEnabled(trace, 19);
		}

		internal static void ThrowingException(EtwDiagnosticTrace trace, string param0, string param1, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 3))
			{
				TraceCore.WriteEtwEvent(trace, 3, null, param0, param1, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Warning))
			{
				object[] objArray = new object[2];
				objArray[0] = param0;
				objArray[1] = param1;
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("ThrowingException", TraceCore.Culture), objArray);
				TraceCore.WriteTraceSource(trace, 3, str, serializedPayload);
			}
		}

		internal static bool ThrowingExceptionIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Warning))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 3);
			}
		}

		internal static void ThrowingExceptionVerbose(EtwDiagnosticTrace trace, string param0, string param1, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 20))
			{
				TraceCore.WriteEtwEvent(trace, 20, null, param0, param1, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Verbose))
			{
				object[] objArray = new object[2];
				objArray[0] = param0;
				objArray[1] = param1;
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("ThrowingExceptionVerbose", TraceCore.Culture), objArray);
				TraceCore.WriteTraceSource(trace, 20, str, serializedPayload);
			}
		}

		internal static bool ThrowingExceptionVerboseIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Verbose))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 20);
			}
		}

		internal static void TraceCodeEventLogCritical(EtwDiagnosticTrace trace, TraceRecord traceRecord)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, traceRecord, null);
			if (TraceCore.IsEtwEventEnabled(trace, 5))
			{
				TraceCore.WriteEtwEvent(trace, 5, null, serializedPayload.ExtendedData, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Critical))
			{
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("TraceCodeEventLogCritical", TraceCore.Culture), new object[0]);
				TraceCore.WriteTraceSource(trace, 5, str, serializedPayload);
			}
		}

		internal static bool TraceCodeEventLogCriticalIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Critical))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 5);
			}
		}

		internal static void TraceCodeEventLogError(EtwDiagnosticTrace trace, TraceRecord traceRecord)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, traceRecord, null);
			if (TraceCore.IsEtwEventEnabled(trace, 6))
			{
				TraceCore.WriteEtwEvent(trace, 6, null, serializedPayload.ExtendedData, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Error))
			{
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("TraceCodeEventLogError", TraceCore.Culture), new object[0]);
				TraceCore.WriteTraceSource(trace, 6, str, serializedPayload);
			}
		}

		internal static bool TraceCodeEventLogErrorIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Error))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 6);
			}
		}

		internal static void TraceCodeEventLogInfo(EtwDiagnosticTrace trace, TraceRecord traceRecord)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, traceRecord, null);
			if (TraceCore.IsEtwEventEnabled(trace, 7))
			{
				TraceCore.WriteEtwEvent(trace, 7, null, serializedPayload.ExtendedData, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
			{
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("TraceCodeEventLogInfo", TraceCore.Culture), new object[0]);
				TraceCore.WriteTraceSource(trace, 7, str, serializedPayload);
			}
		}

		internal static bool TraceCodeEventLogInfoIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Informational))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 7);
			}
		}

		internal static void TraceCodeEventLogVerbose(EtwDiagnosticTrace trace, TraceRecord traceRecord)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, traceRecord, null);
			if (TraceCore.IsEtwEventEnabled(trace, 8))
			{
				TraceCore.WriteEtwEvent(trace, 8, null, serializedPayload.ExtendedData, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Verbose))
			{
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("TraceCodeEventLogVerbose", TraceCore.Culture), new object[0]);
				TraceCore.WriteTraceSource(trace, 8, str, serializedPayload);
			}
		}

		internal static bool TraceCodeEventLogVerboseIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Verbose))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 8);
			}
		}

		internal static void TraceCodeEventLogWarning(EtwDiagnosticTrace trace, TraceRecord traceRecord)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, traceRecord, null);
			if (TraceCore.IsEtwEventEnabled(trace, 9))
			{
				TraceCore.WriteEtwEvent(trace, 9, null, serializedPayload.ExtendedData, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Warning))
			{
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("TraceCodeEventLogWarning", TraceCore.Culture), new object[0]);
				TraceCore.WriteTraceSource(trace, 9, str, serializedPayload);
			}
		}

		internal static bool TraceCodeEventLogWarningIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Warning))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 9);
			}
		}

		internal static void UnhandledException(EtwDiagnosticTrace trace, string param0, Exception exception)
		{
			TracePayload serializedPayload = trace.GetSerializedPayload(null, null, exception);
			if (TraceCore.IsEtwEventEnabled(trace, 4))
			{
				TraceCore.WriteEtwEvent(trace, 4, null, param0, serializedPayload.SerializedException, serializedPayload.AppDomainFriendlyName);
			}
			if (trace.ShouldTraceToTraceSource(TraceEventLevel.Critical))
			{
				object[] objArray = new object[1];
				objArray[0] = param0;
				string str = string.Format(TraceCore.Culture, TraceCore.ResourceManager.GetString("UnhandledException", TraceCore.Culture), objArray);
				TraceCore.WriteTraceSource(trace, 4, str, serializedPayload);
			}
		}

		internal static bool UnhandledExceptionIsEnabled(EtwDiagnosticTrace trace)
		{
			if (trace.ShouldTrace(TraceEventLevel.Critical))
			{
				return true;
			}
			else
			{
				return TraceCore.IsEtwEventEnabled(trace, 4);
			}
		}

		[SecuritySafeCritical]
		private static bool WriteEtwEvent(EtwDiagnosticTrace trace, int eventIndex, EventTraceActivity eventParam0, string eventParam1, string eventParam2, string eventParam3, string eventParam4)
		{
			TraceCore.EnsureEventDescriptors();
			return trace.EtwProvider.WriteEvent(ref TraceCore.eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4);
		}

		[SecuritySafeCritical]
		private static bool WriteEtwEvent(EtwDiagnosticTrace trace, int eventIndex, EventTraceActivity eventParam0, string eventParam1, string eventParam2, string eventParam3)
		{
			TraceCore.EnsureEventDescriptors();
			return trace.EtwProvider.WriteEvent(ref TraceCore.eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3);
		}

		[SecuritySafeCritical]
		private static bool WriteEtwEvent(EtwDiagnosticTrace trace, int eventIndex, EventTraceActivity eventParam0, string eventParam1, string eventParam2)
		{
			TraceCore.EnsureEventDescriptors();
			return trace.EtwProvider.WriteEvent(ref TraceCore.eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2);
		}

		[SecuritySafeCritical]
		private static bool WriteEtwEvent(EtwDiagnosticTrace trace, int eventIndex, EventTraceActivity eventParam0, int eventParam1, string eventParam2)
		{
			TraceCore.EnsureEventDescriptors();
			object[] objArray = new object[2];
			objArray[0] = eventParam1;
			objArray[1] = eventParam2;
			return trace.EtwProvider.WriteEvent(ref TraceCore.eventDescriptors[eventIndex], eventParam0, objArray);
		}

		[SecuritySafeCritical]
		private static bool WriteEtwEvent(EtwDiagnosticTrace trace, int eventIndex, EventTraceActivity eventParam0, int eventParam1, int eventParam2, string eventParam3)
		{
			TraceCore.EnsureEventDescriptors();
			object[] objArray = new object[3];
			objArray[0] = eventParam1;
			objArray[1] = eventParam2;
			objArray[2] = eventParam3;
			return trace.EtwProvider.WriteEvent(ref TraceCore.eventDescriptors[eventIndex], eventParam0, objArray);
		}

		[SecuritySafeCritical]
		private static bool WriteEtwEvent(EtwDiagnosticTrace trace, int eventIndex, EventTraceActivity eventParam0, string eventParam1)
		{
			TraceCore.EnsureEventDescriptors();
			return trace.EtwProvider.WriteEvent(ref TraceCore.eventDescriptors[eventIndex], eventParam0, eventParam1);
		}

		[SecuritySafeCritical]
		private static void WriteTraceSource(EtwDiagnosticTrace trace, int eventIndex, string description, TracePayload payload)
		{
			TraceCore.EnsureEventDescriptors();
			trace.WriteTraceSource(ref TraceCore.eventDescriptors[eventIndex], description, payload);
		}
	}
}