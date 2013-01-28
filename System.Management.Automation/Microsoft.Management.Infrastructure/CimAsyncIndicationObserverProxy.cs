using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimAsyncIndicationObserverProxy : CimAsyncObserverProxyBase<CimSubscriptionResult>
	{
		private readonly bool _shortenLifetimeOfResults;

		internal CimAsyncIndicationObserverProxy(IObserver<CimSubscriptionResult> observer, bool shortenLifetimeOfResults) : base(observer)
		{
			this._shortenLifetimeOfResults = shortenLifetimeOfResults;
		}

		internal void IndicationResultCallback(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, InstanceHandle instanceHandle, string bookMark, string machineID, bool moreResults, MiResult operationResult, string errorMessage, InstanceHandle errorDetailsHandle)
		{
			CimSubscriptionResult cimSubscriptionResult = null;
			if (instanceHandle != null && !instanceHandle.IsInvalid)
			{
				if (!this._shortenLifetimeOfResults)
				{
					instanceHandle = instanceHandle.Clone();
				}
				cimSubscriptionResult = new CimSubscriptionResult(instanceHandle, bookMark, machineID);
			}
			using (cimSubscriptionResult)
			{
				if (!this._shortenLifetimeOfResults || cimSubscriptionResult == null)
				{
					base.ProcessNativeCallback(callbackProcessingContext, cimSubscriptionResult, moreResults, operationResult, errorMessage, errorDetailsHandle);
				}
			}
		}

		public override void RegisterAcceptedAsyncCallbacks(OperationCallbacks operationCallbacks, CimOperationOptions operationOptions)
		{
			base.RegisterAcceptedAsyncCallbacks(operationCallbacks, operationOptions);
			operationCallbacks.IndicationResultCallback = this.IndicationResultCallback;
		}
	}
}