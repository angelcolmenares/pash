using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimAsyncClassObserverProxy : CimAsyncObserverProxyBase<CimClass>
	{
		private readonly bool _shortenLifetimeOfResults;

		internal CimAsyncClassObserverProxy(IObserver<CimClass> observer, bool shortenLifetimeOfResults) : base(observer)
		{
			this._shortenLifetimeOfResults = shortenLifetimeOfResults;
		}

		internal void ClassCallback(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, ClassHandle ClassHandle, bool moreResults, MiResult operationResult, string errorMessage, InstanceHandle errorDetailsHandle)
		{
			CimClass cimClass = null;
			if (ClassHandle != null && !ClassHandle.IsInvalid)
			{
				if (!this._shortenLifetimeOfResults)
				{
					ClassHandle = ClassHandle.Clone();
				}
				cimClass = new CimClass(ClassHandle);
			}
			//using (cimClass)
			{
				if (!this._shortenLifetimeOfResults || cimClass == null)
				{
					base.ProcessNativeCallback(callbackProcessingContext, cimClass, moreResults, operationResult, errorMessage, errorDetailsHandle);
				}
			}
		}

		public override void RegisterAcceptedAsyncCallbacks(OperationCallbacks operationCallbacks, CimOperationOptions operationOptions)
		{
			base.RegisterAcceptedAsyncCallbacks(operationCallbacks, operationOptions);
			operationCallbacks.ClassCallback = this.ClassCallback;
		}
	}
}