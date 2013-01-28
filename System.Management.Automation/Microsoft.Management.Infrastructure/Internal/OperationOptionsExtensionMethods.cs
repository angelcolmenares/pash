using Microsoft.Management.Infrastructure.Internal.Operations;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Threading;

namespace Microsoft.Management.Infrastructure.Options.Internal
{
	internal static class OperationOptionsExtensionMethods
	{
		internal static CancellationToken? GetCancellationToken(this CimOperationOptions operationOptions)
		{
			if (operationOptions != null)
			{
				return operationOptions.CancellationToken;
			}
			else
			{
				CancellationToken? nullable = null;
				return nullable;
			}
		}

		internal static bool GetClassNamesOnly(this CimOperationOptions operationOptions)
		{
			if (operationOptions == null)
			{
				return false;
			}
			else
			{
				return operationOptions.ClassNamesOnly;
			}
		}

		internal static bool GetKeysOnly(this CimOperationOptions operationOptions)
		{
			if (operationOptions == null)
			{
				return false;
			}
			else
			{
				return operationOptions.KeysOnly;
			}
		}

		internal static OperationCallbacks GetOperationCallbacks(this CimOperationOptions operationOptions)
		{
			OperationCallbacks operationCallback = new OperationCallbacks();
			if (operationOptions != null)
			{
				operationCallback.WriteErrorCallback = operationOptions.OperationCallback.WriteErrorCallback;
				operationCallback.WriteMessageCallback = operationOptions.OperationCallback.WriteMessageCallback;
				operationCallback.WriteProgressCallback = operationOptions.OperationCallback.WriteProgressCallback;
				operationCallback.PromptUserCallback = operationOptions.OperationCallback.PromptUserCallback;
			}
			return operationCallback;
		}

		internal static OperationCallbacks GetOperationCallbacks(this CimOperationOptions operationOptions, CimAsyncCallbacksReceiverBase acceptCallbacksReceiver)
		{
			OperationCallbacks operationCallbacks = operationOptions.GetOperationCallbacks();
			if (acceptCallbacksReceiver != null)
			{
				acceptCallbacksReceiver.RegisterAcceptedAsyncCallbacks(operationCallbacks, operationOptions);
			}
			return operationCallbacks;
		}

		internal static MiOperationFlags GetOperationFlags(this CimOperationOptions operationOptions)
		{
			if (operationOptions != null)
			{
				return operationOptions.Flags.ToNative();
			}
			else
			{
				return ((CimOperationFlags)0).ToNative();
			}
		}

		internal static OperationOptionsHandle GetOperationOptionsHandle(this CimOperationOptions operationOptions)
		{
			if (operationOptions != null)
			{
				return operationOptions.OperationOptionsHandle;
			}
			else
			{
				return null;
			}
		}

		internal static bool GetReportOperationStarted(this CimOperationOptions operationOptions)
		{
			if (operationOptions != null)
			{
				return operationOptions.ReportOperationStarted;
			}
			else
			{
				return false;
			}
		}

		internal static bool GetShortenLifetimeOfResults(this CimOperationOptions operationOptions)
		{
			if (operationOptions != null)
			{
				return operationOptions.ShortenLifetimeOfResults;
			}
			else
			{
				return false;
			}
		}
	}
}