using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimSyncClassEnumerator : CimSyncEnumeratorBase<CimClass>
	{
		internal CimSyncClassEnumerator(bool shortenLifetimeOfResults) : base(shortenLifetimeOfResults)
		{
		}

		internal override MiResult NativeMoveNext(OperationHandle operationHandle, out CimClass currentItem, out bool moreResults, out MiResult operationResult, out string errorMessage, out InstanceHandle errorDetailsHandle)
		{
			ClassHandle classHandle = null;
			currentItem = null;
			MiResult @class = OperationMethods.GetClass(operationHandle, out classHandle, out moreResults, out operationResult, out errorMessage, out errorDetailsHandle);
			if (classHandle != null && !classHandle.IsInvalid)
			{
				if (!base.ShortenLifetimeOfResults)
				{
					classHandle = classHandle.Clone();
				}
				currentItem = new CimClass(classHandle);
			}
			return @class;
		}
	}
}