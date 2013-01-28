using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure.Options.Internal;
using System;
using System.Threading;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal abstract class CimAsyncObservableBase<TObserverProxy, TResult> : IObservable<TResult>
	where TObserverProxy : CimAsyncObserverProxyBase<TResult>
	where TResult : class
	{
		private readonly Func<CimAsyncCallbacksReceiverBase, OperationHandle> _operationStarter;

		private readonly CancellationToken? _cancellationToken;

		private readonly bool _reportOperationStarted;

		internal CimAsyncObservableBase(CimOperationOptions operationOptions, Func<CimAsyncCallbacksReceiverBase, OperationHandle> operationStarter)
		{
			this._operationStarter = operationStarter;
			this._cancellationToken = operationOptions.GetCancellationToken();
			this._reportOperationStarted = operationOptions.GetReportOperationStarted();
		}

		internal abstract TObserverProxy CreateObserverProxy(IObserver<TResult> observer);

		public IDisposable Subscribe(IObserver<TResult> observer)
		{
			CimOperation cimOperation;
			if (observer != null)
			{
				TObserverProxy tObserverProxy = this.CreateObserverProxy(observer);
				tObserverProxy.SetReportOperationStarted(this._reportOperationStarted);
				IDisposable disposable = CimApplication.AssertNoPendingShutdown();
				using (disposable)
				{
					OperationHandle operationHandle = this._operationStarter(tObserverProxy);
					cimOperation = new CimOperation(operationHandle, this._cancellationToken);
				}
				tObserverProxy.SetOperation(cimOperation);
				return new CimAsyncCancellationDisposable(cimOperation);
			}
			else
			{
				throw new ArgumentNullException("observer");
			}
		}
	}
}