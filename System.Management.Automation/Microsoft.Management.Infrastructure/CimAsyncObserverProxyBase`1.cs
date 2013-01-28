using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimAsyncObserverProxyBase<T> : CimAsyncCallbacksReceiverBase
	where T : class
	{
		private readonly IObserver<T> _observer;

		private bool _reportOperationStarted;

		internal CimAsyncObserverProxyBase(IObserver<T> observer)
		{
			this._observer = observer;
		}

		private void OnCompletedInternal(OperationCallbackProcessingContext callbackProcessingContext)
		{
			IObserver<T> observer = this._observer;
			base.CallIntoUserCallback(callbackProcessingContext, new Action(observer.OnCompleted), false, true);
		}

		private void OnErrorInternal(OperationCallbackProcessingContext callbackProcessingContext, Exception exception)
		{
			base.CallIntoUserCallback(callbackProcessingContext, () => ((CimAsyncObserverProxyBase<T>)this)._observer.OnError(exception), false, true);
		}

		private void OnNextInternal(OperationCallbackProcessingContext callbackProcessingContext, T item)
		{
			base.CallIntoUserCallback(callbackProcessingContext, () => ((CimAsyncObserverProxyBase<T>)this)._observer.OnNext(item), false, false);
		}

		private void ProcessEndOfResultsWorker(OperationCallbackProcessingContext callbackProcessingContext, CimOperation cimOperation, Exception exception)
		{
			if (exception != null)
			{
				CancellationMode cancellationMode = cimOperation.CancellationMode;
				CancellationMode cancellationMode1 = cancellationMode;
				switch (cancellationMode1)
				{
					case CancellationMode.NoCancellationOccured:
					case CancellationMode.IgnoreCancellationRequests:
					{
						this.OnErrorInternal(callbackProcessingContext, exception);
						return;
					}
					case CancellationMode.ThrowOperationCancelledException:
					{
						this.OnErrorInternal(callbackProcessingContext, new OperationCanceledException(exception.Message, exception));
						return;
					}
					case CancellationMode.SilentlyStopProducingResults:
					{
						return;
					}
					default:
					{
						return;
					}
				}
			}
			else
			{
				this.OnCompletedInternal(callbackProcessingContext);
				return;
			}
		}

		internal void ProcessNativeCallback(OperationCallbackProcessingContext callbackProcessingContext, T currentItem, bool moreResults, MiResult operationResult, string errorMessage, InstanceHandle errorDetailsHandle)
		{
			Action<CimOperation> action = null;
			if (!moreResults)
			{
				base.DisposeOperationWhenPossible();
			}
			if (currentItem != null || operationResult != MiResult.OK)
			{
				if (currentItem != null)
				{
					this.OnNextInternal(callbackProcessingContext, currentItem);
				}
			}
			else
			{
				if (this._reportOperationStarted)
				{
					this.OnNextInternal(callbackProcessingContext, currentItem);
				}
			}
			CimException exceptionIfMiResultFailure = CimException.GetExceptionIfMiResultFailure(operationResult, errorMessage, errorDetailsHandle);
			if (exceptionIfMiResultFailure != null)
			{
				try
				{
					throw exceptionIfMiResultFailure;
				}
				catch (CimException cimException1)
				{
					CimException cimException = cimException1;
					exceptionIfMiResultFailure = cimException;
				}
			}
			if (!moreResults)
			{
				CimAsyncObserverProxyBase<T> cimAsyncObserverProxyBase = this;
				if (action == null)
				{
					action = (CimOperation cimOperation) => this.ProcessEndOfResultsWorker(callbackProcessingContext, cimOperation, exceptionIfMiResultFailure);
				}
				cimAsyncObserverProxyBase.InvokeWhenOperationIsSet(action);
			}
		}

		internal override void ReportInternalError(OperationCallbackProcessingContext callbackProcessingContext, Exception internalError)
		{
			this.OnErrorInternal(callbackProcessingContext, internalError);
		}

		internal void SetReportOperationStarted(bool reportOperationStarted)
		{
			this._reportOperationStarted = reportOperationStarted;
		}
	}
}