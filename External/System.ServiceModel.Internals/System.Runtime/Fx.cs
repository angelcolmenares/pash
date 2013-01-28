using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

namespace System.Runtime
{
	internal static class Fx
	{
		private static ExceptionTrace exceptionTrace;

		private static EtwDiagnosticTrace diagnosticTrace;

		[SecurityCritical]
		private static Fx.ExceptionHandler asynchronousThreadExceptionHandler;

		private const string defaultEventSource = "System.Runtime";

		internal static bool AssertsFailFast
		{
			get
			{
				return false;
			}
		}

		public static Fx.ExceptionHandler AsynchronousThreadExceptionHandler
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[SecuritySafeCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return Fx.asynchronousThreadExceptionHandler;
			}
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				Fx.asynchronousThreadExceptionHandler = value;
			}
		}

		internal static Type[] BreakOnExceptionTypes
		{
			get
			{
				return null;
			}
		}

		public static ExceptionTrace Exception
		{
			get
			{
				if (Fx.exceptionTrace == null)
				{
					Fx.exceptionTrace = new ExceptionTrace("System.Runtime", Fx.Trace);
				}
				return Fx.exceptionTrace;
			}
		}

		internal static bool FastDebug
		{
			get
			{
				return false;
			}
		}

		internal static bool StealthDebugger
		{
			get
			{
				return false;
			}
		}

		public static EtwDiagnosticTrace Trace
		{
			get
			{
				if (Fx.diagnosticTrace == null)
				{
					Fx.diagnosticTrace = Fx.InitializeTracing();
				}
				return Fx.diagnosticTrace;
			}
		}

		public static byte[] AllocateByteArray(int size)
		{
			byte[] numArray;
			try
			{
				numArray = new byte[size];
			}
			catch (OutOfMemoryException outOfMemoryException1)
			{
				OutOfMemoryException outOfMemoryException = outOfMemoryException1;
				throw Fx.Exception.AsError(new InsufficientMemoryException(InternalSR.BufferAllocationFailed(size), outOfMemoryException));
			}
			return numArray;
		}

		public static char[] AllocateCharArray(int size)
		{
			char[] chrArray;
			try
			{
				chrArray = new char[size];
			}
			catch (OutOfMemoryException outOfMemoryException1)
			{
				OutOfMemoryException outOfMemoryException = outOfMemoryException1;
				throw Fx.Exception.AsError(new InsufficientMemoryException(InternalSR.BufferAllocationFailed(size * 2), outOfMemoryException));
			}
			return chrArray;
		}

		[Conditional("DEBUG")]
		public static void Assert(bool condition, string description)
		{
		}

		[Conditional("DEBUG")]
		public static void Assert(string description)
		{
			AssertHelper.FireAssert(description);
		}

		public static void AssertAndFailFast(bool condition, string description)
		{
			if (!condition)
			{
				Fx.AssertAndFailFast(description);
			}
		}

		[SecuritySafeCritical]
		public static Exception AssertAndFailFast(string description)
		{
			string str = InternalSR.FailFastMessage(description);
			try
			{
				try
				{
					Fx.Exception.TraceFailFast(str);
				}
				finally
				{
					Environment.FailFast(str);
				}
			}
			catch
			{
				throw;
			}
			return null;
		}

		public static void AssertAndThrow(bool condition, string description)
		{
			if (!condition)
			{
				Fx.AssertAndThrow(description);
			}
		}

		public static Exception AssertAndThrow(string description)
		{
			TraceCore.ShipAssertExceptionMessage(Fx.Trace, description);
			throw new Fx.InternalException(description);
		}

		public static void AssertAndThrowFatal(bool condition, string description)
		{
			if (!condition)
			{
				Fx.AssertAndThrowFatal(description);
			}
		}

		public static Exception AssertAndThrowFatal(string description)
		{
			TraceCore.ShipAssertExceptionMessage(Fx.Trace, description);
			throw new Fx.FatalInternalException(description);
		}

		public static Guid CreateGuid(string guidString)
		{
			bool flag = false;
			Guid empty = Guid.Empty;
			try
			{
				empty = new Guid(guidString);
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					Fx.AssertAndThrow("Creation of the Guid failed.");
				}
			}
			return empty;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static bool HandleAtThreadBase(Exception exception)
		{
			bool flag;
			bool flag1;
			if (exception != null)
			{
				Fx.TraceExceptionNoThrow(exception);
				try
				{
					Fx.ExceptionHandler asynchronousThreadExceptionHandler = Fx.AsynchronousThreadExceptionHandler;
					if (asynchronousThreadExceptionHandler == null)
					{
						flag1 = false;
					}
					else
					{
						flag1 = asynchronousThreadExceptionHandler.HandleException(exception);
					}
					flag = flag1;
				}
				catch (Exception exception2)
				{
					Exception exception1 = exception2;
					Fx.TraceExceptionNoThrow(exception1);
					return false;
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		[SecuritySafeCritical]
		private static EtwDiagnosticTrace InitializeTracing()
		{
			EtwDiagnosticTrace etwDiagnosticTrace = new EtwDiagnosticTrace("System.Runtime", EtwDiagnosticTrace.DefaultEtwProviderId);
			if (etwDiagnosticTrace.EtwProvider != null)
			{
				EtwDiagnosticTrace etwDiagnosticTrace1 = etwDiagnosticTrace;
				Action refreshState = etwDiagnosticTrace1.RefreshState;
				etwDiagnosticTrace1.RefreshState = () => { refreshState(); Fx.UpdateLevel(); };
			}
			Fx.UpdateLevel(etwDiagnosticTrace);
			return etwDiagnosticTrace;
		}

		public static bool IsFatal(Exception exception)
		{
			bool flag;
			while (exception != null)
			{
				if (exception as FatalException != null || exception as OutOfMemoryException != null && exception as InsufficientMemoryException == null || exception as ThreadAbortException != null || exception as Fx.FatalInternalException != null)
				{
					return true;
				}
				else
				{
					if (exception as TypeInitializationException != null || exception as TargetInvocationException != null)
					{
						exception = exception.InnerException;
					}
					else
					{
						if (exception as AggregateException == null)
						{
							break;
						}
						ReadOnlyCollection<Exception> innerExceptions = ((AggregateException)exception).InnerExceptions;
						IEnumerator<Exception> enumerator = innerExceptions.GetEnumerator();
						using (enumerator)
						{
							while (enumerator.MoveNext())
							{
								Exception current = enumerator.Current;
								if (!Fx.IsFatal(current))
								{
									continue;
								}
								flag = true;
								return flag;
							}
							break;
						}
						return flag;
					}
				}
			}
			return false;
		}

		public static Action<T1> ThunkCallback<T1>(Action<T1> callback)
		{
			return (new Fx.ActionThunk<T1>(callback)).ThunkFrame;
		}

		public static AsyncCallback ThunkCallback(AsyncCallback callback)
		{
			return (new Fx.AsyncThunk(callback)).ThunkFrame;
		}

		public static WaitCallback ThunkCallback(WaitCallback callback)
		{
			return (new Fx.WaitThunk(callback)).ThunkFrame;
		}

		public static TimerCallback ThunkCallback(TimerCallback callback)
		{
			return (new Fx.TimerThunk(callback)).ThunkFrame;
		}

		public static WaitOrTimerCallback ThunkCallback(WaitOrTimerCallback callback)
		{
			return (new Fx.WaitOrTimerThunk(callback)).ThunkFrame;
		}

		public static SendOrPostCallback ThunkCallback(SendOrPostCallback callback)
		{
			return (new Fx.SendOrPostThunk(callback)).ThunkFrame;
		}

		[SecurityCritical]
		public static IOCompletionCallback ThunkCallback(IOCompletionCallback callback)
		{
			return (new Fx.IOCompletionThunk(callback)).ThunkFrame;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static void TraceExceptionNoThrow(Exception exception)
		{
			try
			{
				Fx.Exception.TraceUnhandledException(exception);
			}
			catch
			{
			}
		}

		public static bool TryCreateGuid(string guidString, out Guid result)
		{
			bool flag = false;
			result = Guid.Empty;
			try
			{
				result = new Guid(guidString);
				flag = true;
			}
			catch (ArgumentException argumentException)
			{
			}
			catch (FormatException formatException)
			{
			}
			catch (OverflowException overflowException)
			{
			}
			return flag;
		}

		private static void UpdateLevel(EtwDiagnosticTrace trace)
		{
			if (trace != null)
			{
				if (TraceCore.ActionItemCallbackInvokedIsEnabled(trace) || TraceCore.ActionItemScheduledIsEnabled(trace))
				{
					trace.SetEnd2EndActivityTracingEnabled(true);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static void UpdateLevel()
		{
			Fx.UpdateLevel(Fx.Trace);
		}

		private sealed class ActionThunk<T1> : Fx.Thunk<Action<T1>>
		{
			public Action<T1> ThunkFrame
			{
				get
				{
					return new Action<T1>(this.UnhandledExceptionFrame);
				}
			}

			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			public ActionThunk(Action<T1> callback) : base(callback)
			{
			}

			[SecuritySafeCritical]
			private void UnhandledExceptionFrame(T1 result)
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					base.Callback(result);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.HandleAtThreadBase(exception))
					{
						throw;
					}
				}
			}
		}

		private sealed class AsyncThunk : Fx.Thunk<AsyncCallback>
		{
			public AsyncCallback ThunkFrame
			{
				get
				{
					return new AsyncCallback(this.UnhandledExceptionFrame);
				}
			}

			public AsyncThunk(AsyncCallback callback) : base(callback)
			{
			}

			[SecuritySafeCritical]
			private void UnhandledExceptionFrame(IAsyncResult result)
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					base.Callback(result);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.HandleAtThreadBase(exception))
					{
						throw;
					}
				}
			}
		}

		public abstract class ExceptionHandler
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			protected ExceptionHandler()
			{
			}

			public abstract bool HandleException(Exception exception);
		}

		[Serializable]
		private class FatalInternalException : Fx.InternalException
		{
			public FatalInternalException(string description) : base(description)
			{
			}

			protected FatalInternalException(SerializationInfo info, StreamingContext context) : base(info, context)
			{
			}
		}

		[Serializable]
		private class InternalException : SystemException
		{
			public InternalException(string description) : base(InternalSR.ShipAssertExceptionMessage(description))
			{
			}

			protected InternalException(SerializationInfo info, StreamingContext context) : base(info, context)
			{
			}
		}

		[SecurityCritical]
		private sealed class IOCompletionThunk
		{
			private IOCompletionCallback callback;

			public unsafe IOCompletionCallback ThunkFrame
			{
				get
				{
					return new IOCompletionCallback(this.UnhandledExceptionFrame);
				}
			}

			public IOCompletionThunk(IOCompletionCallback callback)
			{
				this.callback = callback;
			}

			private unsafe void UnhandledExceptionFrame(uint error, uint bytesRead, NativeOverlapped* nativeOverlapped)
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					this.callback(error, bytesRead, nativeOverlapped);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.HandleAtThreadBase(exception))
					{
						throw;
					}
				}
			}
		}

		private sealed class SendOrPostThunk : Fx.Thunk<SendOrPostCallback>
		{
			public SendOrPostCallback ThunkFrame
			{
				get
				{
					return new SendOrPostCallback(this.UnhandledExceptionFrame);
				}
			}

			public SendOrPostThunk(SendOrPostCallback callback) : base(callback)
			{
			}

			[SecuritySafeCritical]
			private void UnhandledExceptionFrame(object state)
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					base.Callback(state);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.HandleAtThreadBase(exception))
					{
						throw;
					}
				}
			}
		}

		public static class Tag
		{
			[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited=false)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class BlockingAttribute : Attribute
			{
				public Type CancelDeclaringType
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public string CancelMethod
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public string Conditional
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				public BlockingAttribute()
				{
				}
			}

			[Flags]
			public enum BlocksUsing
			{
				MonitorEnter,
				MonitorWait,
				ManualResetEvent,
				AutoResetEvent,
				AsyncResult,
				IAsyncResult,
				PInvoke,
				InputQueue,
				ThreadNeutralSemaphore,
				PrivatePrimitive,
				OtherInternalPrimitive,
				OtherFrameworkPrimitive,
				OtherInterop,
				Other,
				NonBlocking
			}

			[AttributeUsage(AttributeTargets.Field)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class CacheAttribute : Attribute
			{
				private readonly Type elementType;

				private readonly Fx.Tag.CacheAttrition cacheAttrition;

				public Fx.Tag.CacheAttrition CacheAttrition
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.cacheAttrition;
					}
				}

				public Type ElementType
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.elementType;
					}
				}

				public string Scope
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public string SizeLimit
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public string Timeout
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public CacheAttribute(Type elementType, Fx.Tag.CacheAttrition cacheAttrition)
				{
					this.Scope = "instance of declaring class";
					this.SizeLimit = "unbounded";
					this.Timeout = "infinite";
					if (elementType != null)
					{
						this.elementType = elementType;
						this.cacheAttrition = cacheAttrition;
						return;
					}
					else
					{
						throw Fx.Exception.ArgumentNull("elementType");
					}
				}
			}

			public enum CacheAttrition
			{
				None,
				ElementOnTimer,
				ElementOnGC,
				ElementOnCallback,
				FullPurgeOnTimer,
				FullPurgeOnEachAccess,
				PartialPurgeOnTimer,
				PartialPurgeOnEachAccess
			}

			[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple=true, Inherited=false)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class ExternalResourceAttribute : Attribute
			{
				private readonly Fx.Tag.Location location;

				private readonly string description;

				public string Description
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.description;
					}
				}

				public Fx.Tag.Location Location
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.location;
					}
				}

				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				public ExternalResourceAttribute(Fx.Tag.Location location, string description)
				{
					this.location = location;
					this.description = description;
				}
			}

			[AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple=true, Inherited=false)]
			[Conditional("DEBUG")]
			public sealed class FriendAccessAllowedAttribute : Attribute
			{
				public string AssemblyName
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public FriendAccessAllowedAttribute(string assemblyName)
				{
					this.AssemblyName = assemblyName;
				}
			}

			[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited=false)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class GuaranteeNonBlockingAttribute : Attribute
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				public GuaranteeNonBlockingAttribute()
				{
				}
			}

			[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited=false)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class InheritThrowsAttribute : Attribute
			{
				public string From
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public Type FromDeclaringType
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				public InheritThrowsAttribute()
				{
				}
			}

			[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class KnownXamlExternalAttribute : Attribute
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				public KnownXamlExternalAttribute()
				{
				}
			}

			public enum Location
			{
				InProcess,
				OutOfProcess,
				LocalSystem,
				LocalOrRemoteSystem,
				RemoteSystem
			}

			[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited=false)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class NonThrowingAttribute : Attribute
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				public NonThrowingAttribute()
				{
				}
			}

			[AttributeUsage(AttributeTargets.Field)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class QueueAttribute : Attribute
			{
				private readonly Type elementType;

				public Type ElementType
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.elementType;
					}
				}

				public bool EnqueueThrowsIfFull
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public string Scope
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public string SizeLimit
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public bool StaleElementsRemovedImmediately
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public QueueAttribute(Type elementType)
				{
					this.Scope = "instance of declaring class";
					this.SizeLimit = "unbounded";
					if (elementType != null)
					{
						this.elementType = elementType;
						return;
					}
					else
					{
						throw Fx.Exception.ArgumentNull("elementType");
					}
				}
			}

			[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple=false, Inherited=false)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class SecurityNoteAttribute : Attribute
			{
				public string Critical
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public string Miscellaneous
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public string Safe
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				public SecurityNoteAttribute()
				{
				}
			}

			public static class Strings
			{
				internal const string ExternallyManaged = "externally managed";

				internal const string AppDomain = "AppDomain";

				internal const string DeclaringInstance = "instance of declaring class";

				internal const string Unbounded = "unbounded";

				internal const string Infinite = "infinite";

			}

			public enum SynchronizationKind
			{
				LockStatement,
				MonitorWait,
				MonitorExplicit,
				InterlockedNoSpin,
				InterlockedWithSpin,
				FromFieldType
			}

			[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited=false)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class SynchronizationObjectAttribute : Attribute
			{
				public bool Blocking
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public Fx.Tag.SynchronizationKind Kind
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public string Scope
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public SynchronizationObjectAttribute()
				{
					this.Blocking = true;
					this.Scope = "instance of declaring class";
					this.Kind = Fx.Tag.SynchronizationKind.FromFieldType;
				}
			}

			[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited=true)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class SynchronizationPrimitiveAttribute : Attribute
			{
				private readonly Fx.Tag.BlocksUsing blocksUsing;

				public Fx.Tag.BlocksUsing BlocksUsing
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.blocksUsing;
					}
				}

				public string ReleaseMethod
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public bool Spins
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public bool SupportsAsync
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				public SynchronizationPrimitiveAttribute(Fx.Tag.BlocksUsing blocksUsing)
				{
					this.blocksUsing = blocksUsing;
				}
			}

			public enum ThrottleAction
			{
				Reject,
				Pause
			}

			[AttributeUsage(AttributeTargets.Field)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class ThrottleAttribute : Attribute
			{
				private readonly Fx.Tag.ThrottleAction throttleAction;

				private readonly Fx.Tag.ThrottleMetric throttleMetric;

				private readonly string limit;

				public string Limit
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.limit;
					}
				}

				public string Scope
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					set;
				}

				public Fx.Tag.ThrottleAction ThrottleAction
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.throttleAction;
					}
				}

				public Fx.Tag.ThrottleMetric ThrottleMetric
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.throttleMetric;
					}
				}

				public ThrottleAttribute(Fx.Tag.ThrottleAction throttleAction, Fx.Tag.ThrottleMetric throttleMetric, string limit)
				{
					this.Scope = "AppDomain";
					if (!string.IsNullOrEmpty(limit))
					{
						this.throttleAction = throttleAction;
						this.throttleMetric = throttleMetric;
						this.limit = limit;
						return;
					}
					else
					{
						throw Fx.Exception.ArgumentNullOrEmpty("limit");
					}
				}
			}

			public enum ThrottleMetric
			{
				Count,
				Rate,
				Other
			}

			public static class Throws
			{
				[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
				[Conditional("CODE_ANALYSIS_CDF")]
				public sealed class TimeoutAttribute : Fx.Tag.ThrowsAttribute
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					public TimeoutAttribute() : this("The operation timed out.")
					{
					}

					public TimeoutAttribute(string diagnosis) : base(typeof(TimeoutException), diagnosis)
					{
					}
				}
			}

			[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public class ThrowsAttribute : Attribute
			{
				private readonly Type exceptionType;

				private readonly string diagnosis;

				public string Diagnosis
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.diagnosis;
					}
				}

				public Type ExceptionType
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get
					{
						return this.exceptionType;
					}
				}

				public ThrowsAttribute(Type exceptionType, string diagnosis)
				{
					if (exceptionType != null)
					{
						if (!string.IsNullOrEmpty(diagnosis))
						{
							this.exceptionType = exceptionType;
							this.diagnosis = diagnosis;
							return;
						}
						else
						{
							throw Fx.Exception.ArgumentNullOrEmpty("diagnosis");
						}
					}
					else
					{
						throw Fx.Exception.ArgumentNull("exceptionType");
					}
				}
			}

			[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple=false, Inherited=false)]
			[Conditional("CODE_ANALYSIS_CDF")]
			public sealed class XamlVisibleAttribute : Attribute
			{
				public bool Visible
				{
					[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
					get;
					private set;
				}

				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				public XamlVisibleAttribute() : this(true)
				{
				}

				public XamlVisibleAttribute(bool visible)
				{
					this.Visible = visible;
				}
			}
		}

		private abstract class Thunk<T>
		where T : class
		{
			[SecurityCritical]
			private T callback;

			internal T Callback
			{
				[SecuritySafeCritical]
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.callback;
				}
			}

			[SecuritySafeCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			protected Thunk(T callback)
			{
				this.callback = callback;
			}
		}

		private sealed class TimerThunk : Fx.Thunk<TimerCallback>
		{
			public TimerCallback ThunkFrame
			{
				get
				{
					return new TimerCallback(this.UnhandledExceptionFrame);
				}
			}

			public TimerThunk(TimerCallback callback) : base(callback)
			{
			}

			[SecuritySafeCritical]
			private void UnhandledExceptionFrame(object state)
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					base.Callback(state);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.HandleAtThreadBase(exception))
					{
						throw;
					}
				}
			}
		}

		private sealed class WaitOrTimerThunk : Fx.Thunk<WaitOrTimerCallback>
		{
			public WaitOrTimerCallback ThunkFrame
			{
				get
				{
					return new WaitOrTimerCallback(this.UnhandledExceptionFrame);
				}
			}

			public WaitOrTimerThunk(WaitOrTimerCallback callback) : base(callback)
			{
			}

			[SecuritySafeCritical]
			private void UnhandledExceptionFrame(object state, bool timedOut)
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					base.Callback(state, timedOut);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.HandleAtThreadBase(exception))
					{
						throw;
					}
				}
			}
		}

		private sealed class WaitThunk : Fx.Thunk<WaitCallback>
		{
			public WaitCallback ThunkFrame
			{
				get
				{
					return new WaitCallback(this.UnhandledExceptionFrame);
				}
			}

			public WaitThunk(WaitCallback callback) : base(callback)
			{
			}

			[SecuritySafeCritical]
			private void UnhandledExceptionFrame(object state)
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					base.Callback(state);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!Fx.HandleAtThreadBase(exception))
					{
						throw;
					}
				}
			}
		}
	}
}