using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal abstract class CimSyncEnumerableBase<TItem, TEnumerator> : IEnumerable<TItem>, IEnumerable
	where TItem : class
	where TEnumerator : CimSyncEnumeratorBase<TItem>, IEnumerator<TItem>
	{
		private readonly CancellationToken? _cancellationToken;

		private readonly Func<CimAsyncCallbacksReceiverBase, OperationHandle> _operationStarter;

		internal CimSyncEnumerableBase(CimOperationOptions operationOptions, Func<CimAsyncCallbacksReceiverBase, OperationHandle> operationStarter)
		{
			this._cancellationToken = operationOptions.GetCancellationToken();
			this._operationStarter = operationStarter;
		}

		internal abstract TEnumerator CreateEnumerator();

		public IEnumerator<TItem> GetEnumerator()
		{
			CimOperation cimOperation;
			TEnumerator tEnumerator = this.CreateEnumerator();
			IDisposable disposable = CimApplication.AssertNoPendingShutdown();
			using (disposable)
			{
				OperationHandle operationHandle = this._operationStarter(tEnumerator);
				operationHandle.SetExtraFinalizationAction(new Action(tEnumerator.InternalFinalizationHelper));
				cimOperation = new CimOperation(operationHandle, this._cancellationToken);
			}
			tEnumerator.SetOperation(cimOperation);
			return tEnumerator;
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}