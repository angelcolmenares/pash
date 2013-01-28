using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimAsyncIndicationObservable : CimAsyncObservableBase<CimAsyncIndicationObserverProxy, CimSubscriptionResult>
	{
		private readonly bool _shortenLifetimeOfResults;

		internal CimAsyncIndicationObservable(CimOperationOptions operationOptions, Func<CimAsyncCallbacksReceiverBase, OperationHandle> operationStarter) : base(operationOptions, operationStarter)
		{
			this._shortenLifetimeOfResults = operationOptions.GetShortenLifetimeOfResults();
		}

		internal override CimAsyncIndicationObserverProxy CreateObserverProxy(IObserver<CimSubscriptionResult> observer)
		{
			return new CimAsyncIndicationObserverProxy(observer, this._shortenLifetimeOfResults);
		}
	}
}