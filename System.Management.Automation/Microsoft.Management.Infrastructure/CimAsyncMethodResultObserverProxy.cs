using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimAsyncMethodResultObserverProxy : CimAsyncObserverProxyBase<CimMethodResultBase>
	{
		private readonly bool _shortenLifetimeOfResults;

		private readonly Guid _CimSessionInstanceID;

		private readonly string _CimSessionComputerName;

		internal CimAsyncMethodResultObserverProxy(IObserver<CimMethodResultBase> observer, Guid cimSessionInstanceID, string cimSessionComputerName, bool shortenLifetimeOfResults) : base(observer)
		{
			this._shortenLifetimeOfResults = shortenLifetimeOfResults;
			this._CimSessionInstanceID = cimSessionInstanceID;
			this._CimSessionComputerName = cimSessionComputerName;
		}

		internal void InstanceResultCallback(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, InstanceHandle instanceHandle, bool moreResults, MiResult operationResult, string errorMessage, InstanceHandle errorDetailsHandle)
		{
			CimMethodResult cimMethodResult = null;
			if (instanceHandle != null && !instanceHandle.IsInvalid)
			{
				if (!this._shortenLifetimeOfResults)
				{
					instanceHandle = instanceHandle.Clone();
				}
				CimInstance cimInstance = new CimInstance(instanceHandle, null);
				cimInstance.SetCimSessionComputerName(this._CimSessionComputerName);
				cimInstance.SetCimSessionInstanceId(this._CimSessionInstanceID);
				cimMethodResult = new CimMethodResult(cimInstance);
			}
			using (cimMethodResult)
			{
				if (!this._shortenLifetimeOfResults || cimMethodResult == null)
				{
					base.ProcessNativeCallback(callbackProcessingContext, cimMethodResult, moreResults, operationResult, errorMessage, errorDetailsHandle);
				}
			}
		}

		public override void RegisterAcceptedAsyncCallbacks(OperationCallbacks operationCallbacks, CimOperationOptions operationOptions)
		{
			base.RegisterAcceptedAsyncCallbacks(operationCallbacks, operationOptions);
			operationCallbacks.InstanceResultCallback = this.InstanceResultCallback;
			if (operationOptions != null && operationOptions.EnableMethodResultStreaming)
			{
				operationCallbacks.StreamedParameterCallback = this.StreamedParameterCallback;
			}
		}

		internal void StreamedParameterCallback(OperationCallbackProcessingContext callbackProcessingContext, OperationHandle operationHandle, string parameterName, object parameterValue, MiType parameterType)
		{
			parameterValue = CimInstance.ConvertFromNativeLayer(parameterValue, null, null, !this._shortenLifetimeOfResults);
			CimInstance cimInstance = parameterValue as CimInstance;
			if (cimInstance != null)
			{
				cimInstance.SetCimSessionComputerName(this._CimSessionComputerName);
				cimInstance.SetCimSessionInstanceId(this._CimSessionInstanceID);
			}
			CimInstance[] cimInstanceArray = parameterValue as CimInstance[];
			if (cimInstanceArray != null)
			{
				CimInstance[] cimInstanceArray1 = cimInstanceArray;
				for (int i = 0; i < (int)cimInstanceArray1.Length; i++)
				{
					CimInstance cimInstance1 = cimInstanceArray1[i];
					if (cimInstance1 != null)
					{
						cimInstance1.SetCimSessionComputerName(this._CimSessionComputerName);
						cimInstance1.SetCimSessionInstanceId(this._CimSessionInstanceID);
					}
				}
			}
			try
			{
				CimMethodResultBase cimMethodStreamedResult = new CimMethodStreamedResult(parameterName, parameterValue, parameterType.ToCimType());
				base.ProcessNativeCallback(callbackProcessingContext, cimMethodStreamedResult, true, MiResult.OK, null, null);
			}
			finally
			{
				if (this._shortenLifetimeOfResults)
				{
					CimInstance cimInstance2 = parameterValue as CimInstance;
					if (cimInstance2 != null)
					{
						cimInstance2.Dispose();
					}
					CimInstance[] cimInstanceArray2 = parameterValue as CimInstance[];
					if (cimInstanceArray2 != null)
					{
						CimInstance[] cimInstanceArray3 = cimInstanceArray2;
						for (int j = 0; j < (int)cimInstanceArray3.Length; j++)
						{
							CimInstance cimInstance3 = cimInstanceArray3[j];
							if (cimInstance3 != null)
							{
								cimInstance3.Dispose();
							}
						}
					}
				}
			}
		}
	}
}