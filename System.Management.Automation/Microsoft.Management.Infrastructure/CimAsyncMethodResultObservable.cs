using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimAsyncMethodResultObservable : CimAsyncObservableBase<CimAsyncMethodResultObserverProxy, CimMethodResultBase>
	{
		private readonly bool _shortenLifetimeOfResults;

		private readonly Guid _CimSessionInstanceID;

		private readonly string _CimSessionComputerName;

		internal CimAsyncMethodResultObservable(CimOperationOptions operationOptions, Guid cimSessionInstanceID, string cimSessionComputerName, Func<CimAsyncCallbacksReceiverBase, OperationHandle> operationStarter) : base(operationOptions, operationStarter)
		{
			this._shortenLifetimeOfResults = operationOptions.GetShortenLifetimeOfResults();
			this._CimSessionInstanceID = cimSessionInstanceID;
			this._CimSessionComputerName = cimSessionComputerName;
		}

		internal override CimAsyncMethodResultObserverProxy CreateObserverProxy(IObserver<CimMethodResultBase> observer)
		{
			return new CimAsyncMethodResultObserverProxy(observer, this._CimSessionInstanceID, this._CimSessionComputerName, this._shortenLifetimeOfResults);
		}
	}
}