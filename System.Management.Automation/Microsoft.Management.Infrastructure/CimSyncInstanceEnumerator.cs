using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimSyncInstanceEnumerator : CimSyncEnumeratorBase<CimInstance>
	{
		private readonly Guid _CimSessionInstanceID;

		private readonly string _CimSessionComputerName;

		internal CimSyncInstanceEnumerator(Guid cimSessionInstanceID, string cimSessionComputerName, bool shortenLifetimeOfResults) : base(shortenLifetimeOfResults)
		{
			this._CimSessionInstanceID = cimSessionInstanceID;
			this._CimSessionComputerName = cimSessionComputerName;
		}

		internal override MiResult NativeMoveNext(OperationHandle operationHandle, out CimInstance currentItem, out bool moreResults, out MiResult operationResult, out string errorMessage, out InstanceHandle errorDetailsHandle)
		{
			InstanceHandle instanceHandle = null;
			currentItem = null;
			MiResult instance = OperationMethods.GetInstance(operationHandle, out instanceHandle, out moreResults, out operationResult, out errorMessage, out errorDetailsHandle);
			if (instanceHandle != null && !instanceHandle.IsInvalid)
			{
				if (!base.ShortenLifetimeOfResults)
				{
					instanceHandle = instanceHandle.Clone();
				}
				currentItem = new CimInstance(instanceHandle, null);
				currentItem.SetCimSessionComputerName(this._CimSessionComputerName);
				currentItem.SetCimSessionInstanceId(this._CimSessionInstanceID);
			}
			return instance;
		}
	}
}