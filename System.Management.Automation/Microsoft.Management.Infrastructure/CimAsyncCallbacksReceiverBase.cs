using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal abstract class CimAsyncCallbacksReceiverBase
	{
		private CimOperation _operation;

		private readonly object _operationLock;

		private List<Action<CimOperation>> _operationPendingActions;

		private readonly object _suppressFurtherUserCallbacksLock;

		private bool _suppressFurtherUserCallbacks;

		private readonly ExecutionContext _threadExecutionContext;

		protected CimOperation Operation
		{
			get
			{
				CimOperation cimOperation;
				lock (this._operationLock)
				{
					cimOperation = this._operation;
				}
				return cimOperation;
			}
		}

		protected CimAsyncCallbacksReceiverBase()
		{
			this._operationLock = new object();
			this._operationPendingActions = new List<Action<CimOperation>>();
			this._suppressFurtherUserCallbacksLock = new object();
			this._threadExecutionContext = ExecutionContext.Capture();
		}

		internal void CallIntoUserCallback(OperationCallbackProcessingContext callbackProcessingContext, Action userCallback, bool serializeCallbacks = false, bool suppressFurtherUserCallbacks = false)
		{
			lock (this._suppressFurtherUserCallbacksLock)
			{
				if (!this._suppressFurtherUserCallbacks)
				{
					if (suppressFurtherUserCallbacks)
					{
						this._suppressFurtherUserCallbacks = true;
					}
					callbackProcessingContext.InUserCode = true;
					this.CallUnderOriginalExecutionContext(userCallback);
					callbackProcessingContext.InUserCode = false;
				}
			}
		}

		internal void CallUnderOriginalExecutionContext(Action action)
		{
			ExecutionContext.Run(this._threadExecutionContext.CreateCopy(), (object argument0) => action(), null);
		}

		protected void DisposeOperationWhenPossible()
		{
			this.InvokeWhenOperationIsSet(new Action<CimOperation>(CimAsyncCallbacksReceiverBase.DisposeOperationWhenPossibleWorker));
		}

		private static void DisposeOperationWhenPossibleWorker(CimOperation cimOperation)
		{
			cimOperation.Handle.SetExtraFinalizationAction(null);
			cimOperation.IgnoreSubsequentCancellationRequests();
			cimOperation.Dispose();
		}

		protected void InvokeWhenOperationIsSet(Action<CimOperation> action)
		{
			lock (this._operationLock)
			{
				if (this._operation == null)
				{
					if (this._operationPendingActions == null)
					{
						this._operationPendingActions = new List<Action<CimOperation>>();
					}
					this._operationPendingActions.Add(action);
					return;
				}
			}
			action(this._operation);
		}

		public virtual void RegisterAcceptedAsyncCallbacks(OperationCallbacks operationCallbacks, CimOperationOptions operationOptions)
		{
			operationCallbacks.InternalErrorCallback = new OperationCallbacks.InternalErrorCallbackDelegate(this.ReportInternalErrorCore);
			operationCallbacks.ManagedOperationContext = this;
		}

		internal abstract void ReportInternalError(OperationCallbackProcessingContext callbackProcessingContext, Exception internalError);

		private void ReportInternalErrorCore(OperationCallbackProcessingContext callbackProcessingContext, Exception internalError)
		{
			this.InvokeWhenOperationIsSet((CimOperation cimOperation) => {
				lock (this)
				{
					try
					{
						cimOperation.Cancel(CancellationMode.SilentlyStopProducingResults);
					}
					catch (Exception exception2)
					{
						Exception exception = exception2;
						Exception exception1 = internalError;
						Exception[] exceptionArray = new Exception[2];
						exceptionArray[0] = exception1;
						exceptionArray[1] = exception;
						internalError = new AggregateException(exceptionArray);
					}
					this.ReportInternalError(callbackProcessingContext, internalError);
				}
			}
			);
		}

		internal void SetOperation(CimOperation operation)
		{
			List<Action<CimOperation>> actions = this._operationPendingActions;
			lock (this._operationLock)
			{
				this._operation = operation;
				this._operationPendingActions = null;
			}
			operation.Cancelled += new EventHandler<EventArgs>(this.SupressCallbacksWhenRequestedViaCancellation);
			this.SupressCallbacksWhenRequestedViaCancellation(operation, EventArgs.Empty);
			if (actions != null)
			{
				foreach (Action<CimOperation> action in actions)
				{
					action(operation);
				}
			}
		}

		private void SupressCallbacksWhenRequestedViaCancellation(object sender, EventArgs e)
		{
			lock (this._operationLock)
			{
				if (this._operation.CancellationMode == CancellationMode.SilentlyStopProducingResults)
				{
					lock (this._suppressFurtherUserCallbacksLock)
					{
						this._suppressFurtherUserCallbacks = true;
					}
				}
			}
		}
	}
}