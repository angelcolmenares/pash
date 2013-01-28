using System;
using System.Globalization;
using System.Resources;

namespace System.Runtime
{
	internal class InternalSR
	{
		private static ResourceManager resourceManager;

		private static CultureInfo resourceCulture;

		internal static string ActionItemIsAlreadyScheduled
		{
			get
			{
				return InternalSR.ResourceManager.GetString("ActionItemIsAlreadyScheduled", InternalSR.Culture);
			}
		}

		internal static string AsyncCallbackThrewException
		{
			get
			{
				return InternalSR.ResourceManager.GetString("AsyncCallbackThrewException", InternalSR.Culture);
			}
		}

		internal static string AsyncResultAlreadyEnded
		{
			get
			{
				return InternalSR.ResourceManager.GetString("AsyncResultAlreadyEnded", InternalSR.Culture);
			}
		}

		internal static string BadCopyToArray
		{
			get
			{
				return InternalSR.ResourceManager.GetString("BadCopyToArray", InternalSR.Culture);
			}
		}

		internal static string BufferIsNotRightSizeForBufferManager
		{
			get
			{
				return InternalSR.ResourceManager.GetString("BufferIsNotRightSizeForBufferManager", InternalSR.Culture);
			}
		}

		internal static CultureInfo Culture
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return InternalSR.resourceCulture;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				InternalSR.resourceCulture = value;
			}
		}

		internal static string DictionaryIsReadOnly
		{
			get
			{
				return InternalSR.ResourceManager.GetString("DictionaryIsReadOnly", InternalSR.Culture);
			}
		}

		internal static string InvalidAsyncResult
		{
			get
			{
				return InternalSR.ResourceManager.GetString("InvalidAsyncResult", InternalSR.Culture);
			}
		}

		internal static string InvalidAsyncResultImplementationGeneric
		{
			get
			{
				return InternalSR.ResourceManager.GetString("InvalidAsyncResultImplementationGeneric", InternalSR.Culture);
			}
		}

		internal static string InvalidNullAsyncResult
		{
			get
			{
				return InternalSR.ResourceManager.GetString("InvalidNullAsyncResult", InternalSR.Culture);
			}
		}

		internal static string InvalidSemaphoreExit
		{
			get
			{
				return InternalSR.ResourceManager.GetString("InvalidSemaphoreExit", InternalSR.Culture);
			}
		}

		internal static string KeyCollectionUpdatesNotAllowed
		{
			get
			{
				return InternalSR.ResourceManager.GetString("KeyCollectionUpdatesNotAllowed", InternalSR.Culture);
			}
		}

		internal static string KeyNotFoundInDictionary
		{
			get
			{
				return InternalSR.ResourceManager.GetString("KeyNotFoundInDictionary", InternalSR.Culture);
			}
		}

		internal static string MustCancelOldTimer
		{
			get
			{
				return InternalSR.ResourceManager.GetString("MustCancelOldTimer", InternalSR.Culture);
			}
		}

		internal static string NullKeyAlreadyPresent
		{
			get
			{
				return InternalSR.ResourceManager.GetString("NullKeyAlreadyPresent", InternalSR.Culture);
			}
		}

		internal static string ReadNotSupported
		{
			get
			{
				return InternalSR.ResourceManager.GetString("ReadNotSupported", InternalSR.Culture);
			}
		}

		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(InternalSR.resourceManager, null))
				{
					ResourceManager resourceManager = new ResourceManager("System.Runtime.InternalSR", typeof(InternalSR).Assembly);
					InternalSR.resourceManager = resourceManager;
				}
				return InternalSR.resourceManager;
			}
		}

		internal static string SeekNotSupported
		{
			get
			{
				return InternalSR.ResourceManager.GetString("SeekNotSupported", InternalSR.Culture);
			}
		}

		internal static string SFxTaskNotStarted
		{
			get
			{
				return InternalSR.ResourceManager.GetString("SFxTaskNotStarted", InternalSR.Culture);
			}
		}

		internal static string ThreadNeutralSemaphoreAborted
		{
			get
			{
				return InternalSR.ResourceManager.GetString("ThreadNeutralSemaphoreAborted", InternalSR.Culture);
			}
		}

		internal static string ValueCollectionUpdatesNotAllowed
		{
			get
			{
				return InternalSR.ResourceManager.GetString("ValueCollectionUpdatesNotAllowed", InternalSR.Culture);
			}
		}

		internal static string ValueMustBeNonNegative
		{
			get
			{
				return InternalSR.ResourceManager.GetString("ValueMustBeNonNegative", InternalSR.Culture);
			}
		}

		private InternalSR()
		{
		}

		internal static string ArgumentNullOrEmpty(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("ArgumentNullOrEmpty", InternalSR.Culture), objArray);
		}

		internal static string AsyncEventArgsCompletedTwice(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("AsyncEventArgsCompletedTwice", InternalSR.Culture), objArray);
		}

		internal static string AsyncEventArgsCompletionPending(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("AsyncEventArgsCompletionPending", InternalSR.Culture), objArray);
		}

		internal static string AsyncResultCompletedTwice(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("AsyncResultCompletedTwice", InternalSR.Culture), objArray);
		}

		internal static string BufferAllocationFailed(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("BufferAllocationFailed", InternalSR.Culture), objArray);
		}

		internal static string BufferedOutputStreamQuotaExceeded(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("BufferedOutputStreamQuotaExceeded", InternalSR.Culture), objArray);
		}

		internal static string CannotConvertObject(object param0, object param1)
		{
			object[] objArray = new object[2];
			objArray[0] = param0;
			objArray[1] = param1;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("CannotConvertObject", InternalSR.Culture), objArray);
		}

		internal static string EtwAPIMaxStringCountExceeded(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("EtwAPIMaxStringCountExceeded", InternalSR.Culture), objArray);
		}

		internal static string EtwMaxNumberArgumentsExceeded(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("EtwMaxNumberArgumentsExceeded", InternalSR.Culture), objArray);
		}

		internal static string EtwRegistrationFailed(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("EtwRegistrationFailed", InternalSR.Culture), objArray);
		}

		internal static string FailFastMessage(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("FailFastMessage", InternalSR.Culture), objArray);
		}

		internal static string IncompatibleArgumentType(object param0, object param1)
		{
			object[] objArray = new object[2];
			objArray[0] = param0;
			objArray[1] = param1;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("IncompatibleArgumentType", InternalSR.Culture), objArray);
		}

		internal static string InvalidAsyncResultImplementation(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("InvalidAsyncResultImplementation", InternalSR.Culture), objArray);
		}

		internal static string LockTimeoutExceptionMessage(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("LockTimeoutExceptionMessage", InternalSR.Culture), objArray);
		}

		internal static string ShipAssertExceptionMessage(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("ShipAssertExceptionMessage", InternalSR.Culture), objArray);
		}

		internal static string TaskTimedOutError(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("TaskTimedOutError", InternalSR.Culture), objArray);
		}

		internal static string TimeoutInputQueueDequeue(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("TimeoutInputQueueDequeue", InternalSR.Culture), objArray);
		}

		internal static string TimeoutMustBeNonNegative(object param0, object param1)
		{
			object[] objArray = new object[2];
			objArray[0] = param0;
			objArray[1] = param1;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("TimeoutMustBeNonNegative", InternalSR.Culture), objArray);
		}

		internal static string TimeoutMustBePositive(object param0, object param1)
		{
			object[] objArray = new object[2];
			objArray[0] = param0;
			objArray[1] = param1;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("TimeoutMustBePositive", InternalSR.Culture), objArray);
		}

		internal static string TimeoutOnOperation(object param0)
		{
			object[] objArray = new object[1];
			objArray[0] = param0;
			return string.Format(InternalSR.Culture, InternalSR.ResourceManager.GetString("TimeoutOnOperation", InternalSR.Culture), objArray);
		}
	}
}