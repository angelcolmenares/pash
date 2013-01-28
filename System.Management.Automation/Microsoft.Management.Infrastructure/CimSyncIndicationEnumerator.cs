using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimSyncIndicationEnumerator : CimSyncEnumeratorBase<CimSubscriptionResult>
	{
		internal CimSyncIndicationEnumerator(bool shortenLifetimeOfResults) : base(shortenLifetimeOfResults)
		{
		}

		internal override MiResult NativeMoveNext(OperationHandle operationHandle, out CimSubscriptionResult currentItem, out bool moreResults, out MiResult operationResult, out string errorMessage, out InstanceHandle errorDetailsHandle)
		{
			InstanceHandle instanceHandle = null;
			string str = null;
			string str1 = null;
			currentItem = null;
			MiResult indication = OperationMethods.GetIndication(operationHandle, out instanceHandle, out str, out str1, out moreResults, out operationResult, out errorMessage, out errorDetailsHandle);
			if (instanceHandle != null && !instanceHandle.IsInvalid)
			{
				if (!base.ShortenLifetimeOfResults)
				{
					instanceHandle = instanceHandle.Clone();
				}
				currentItem = new CimSubscriptionResult(instanceHandle, str, str1);
			}
			return indication;
		}
	}
}