using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimAsyncInstanceObserverProxy : CimAsyncObserverProxyBase<CimInstance>
	{
		private readonly bool _shortenLifetimeOfResults;

		private readonly Guid _CimSessionInstanceID;

		private readonly string _CimSessionComputerName;

		internal CimAsyncInstanceObserverProxy(IObserver<CimInstance> observer, Guid cimSessionInstanceID, string cimSessionComputerName, bool shortenLifetimeOfResults) : base(observer)
		{
			this._shortenLifetimeOfResults = shortenLifetimeOfResults;
			this._CimSessionInstanceID = cimSessionInstanceID;
			this._CimSessionComputerName = cimSessionComputerName;
		}

		internal void InstanceResultCallback(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, InstanceHandle instanceHandle, bool moreResults, MiResult operationResult, string errorMessage, InstanceHandle errorDetailsHandle)
		{
			CimInstance cimInstance = null;
			if (instanceHandle != null && !instanceHandle.IsInvalid)
			{
				if (!this._shortenLifetimeOfResults)
				{
					instanceHandle = instanceHandle.Clone();
				}
				cimInstance = new CimInstance(instanceHandle, null);
				cimInstance.SetCimSessionComputerName(this._CimSessionComputerName);
				cimInstance.SetCimSessionInstanceId(this._CimSessionInstanceID);
			}
			//TODO: using (cimInstance)
			{
				if (!this._shortenLifetimeOfResults || cimInstance == null)
				{
					base.ProcessNativeCallback(callbackProcessingContext, cimInstance, moreResults, operationResult, errorMessage, errorDetailsHandle);
				}
			}
		}

		public override void RegisterAcceptedAsyncCallbacks(OperationCallbacks operationCallbacks, CimOperationOptions operationOptions)
		{
			base.RegisterAcceptedAsyncCallbacks(operationCallbacks, operationOptions);
			operationCallbacks.InstanceResultCallback = this.InstanceResultCallback;
		}
	}
}