using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimAsyncClassObservable : CimAsyncObservableBase<CimAsyncClassObserverProxy, CimClass>
	{
		private readonly bool _shortenLifetimeOfResults;

		internal CimAsyncClassObservable(CimOperationOptions operationOptions, Func<CimAsyncCallbacksReceiverBase, OperationHandle> operationStarter) : base(operationOptions, operationStarter)
		{
			this._shortenLifetimeOfResults = operationOptions.GetShortenLifetimeOfResults();
		}

		internal override CimAsyncClassObserverProxy CreateObserverProxy(IObserver<CimClass> observer)
		{
			return new CimAsyncClassObserverProxy(observer, this._shortenLifetimeOfResults);
		}
	}
}