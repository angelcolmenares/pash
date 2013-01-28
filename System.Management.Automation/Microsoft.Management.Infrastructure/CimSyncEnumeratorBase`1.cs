using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal abstract class CimSyncEnumeratorBase<T> : CimAsyncCallbacksReceiverBase, IEnumerator<T>, IDisposable, IEnumerator
	where T : class
	{
		private bool _moreResultsAreExpected;

		private bool _disposed;

		private readonly object _disposeThreadSafetyLock;

		private Exception _internalErrorWhileProcessingAsyncCallback;

		private readonly object _internalErrorWhileProcessingAsyncCallbackLock;
	

		public T Current
		{
			get;set;
		}

		internal bool ShortenLifetimeOfResults
		{
			get;set;
		}

		object System.Collections.IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}

		internal CimSyncEnumeratorBase(bool shortenLifetimeOfResults)
		{
			this._moreResultsAreExpected = true;
			this._disposeThreadSafetyLock = new object();
			this._internalErrorWhileProcessingAsyncCallbackLock = new object();
			this.ShortenLifetimeOfResults = shortenLifetimeOfResults;
		}

		internal void AssertNotDisposed()
		{
			lock (this._disposeThreadSafetyLock)
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException(this.ToString());
				}
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			lock (this._disposeThreadSafetyLock)
			{
				if (!this._disposed)
				{
					if (!disposing)
					{
						this.DisposeWorker();
					}
					else
					{
						base.CallUnderOriginalExecutionContext(new Action(this.DisposeWorker));
					}
					this._disposed = true;
				}
			}
		}

		private void DisposeCurrentItemIfNeeded()
		{
			if (this.ShortenLifetimeOfResults)
			{
				IDisposable current = (object)this.Current as IDisposable;
				if (current != null)
				{
					current.Dispose();
				}
			}
		}

		private void DisposeWorker()
		{
			if (this._moreResultsAreExpected)
			{
				base.Operation.Cancel(CancellationMode.SilentlyStopProducingResults);
				while (this.MoveNext(true))
				{
				}
			}
			base.DisposeOperationWhenPossible();
			this.DisposeCurrentItemIfNeeded();
		}

		internal void InternalFinalizationHelper()
		{
			this.Dispose(false);
		}

		private bool MoveNext(bool discardResultsAndErrors)
		{
			T t = null;
			MiResult miResult = MiResult.OK;
			string str = null;
			InstanceHandle instanceHandle = null;
			bool flag;
			if (!discardResultsAndErrors)
			{
				lock (this._internalErrorWhileProcessingAsyncCallbackLock)
				{
					if (this._internalErrorWhileProcessingAsyncCallback != null)
					{
						throw this._internalErrorWhileProcessingAsyncCallback;
					}
				}
			}
			if (this._moreResultsAreExpected)
			{
				this.AssertNotDisposed();
				MiResult miResult1 = this.NativeMoveNext(base.Operation.Handle, out t, out this._moreResultsAreExpected, out miResult, out str, out instanceHandle);
				CimException.ThrowIfMiResultFailure(miResult1);
				if (!this._moreResultsAreExpected)
				{
					base.Operation.IgnoreSubsequentCancellationRequests();
				}
				if (!discardResultsAndErrors)
				{
					lock (this._internalErrorWhileProcessingAsyncCallbackLock)
					{
						if (this._internalErrorWhileProcessingAsyncCallback != null)
						{
							throw this._internalErrorWhileProcessingAsyncCallback;
						}
					}
					CimException exceptionIfMiResultFailure = CimException.GetExceptionIfMiResultFailure(miResult, str, instanceHandle);
					if (exceptionIfMiResultFailure == null)
					{
						this.DisposeCurrentItemIfNeeded();
						this.Current = t;
					}
					else
					{
						CancellationMode cancellationMode = base.Operation.CancellationMode;
						if (cancellationMode != CancellationMode.ThrowOperationCancelledException)
						{
							throw exceptionIfMiResultFailure;
						}
						else
						{
							throw new OperationCanceledException(exceptionIfMiResultFailure.Message, exceptionIfMiResultFailure);
						}
					}
				}
				else
				{
					IDisposable disposable = (object)t as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
						t = default(T);
					}
					if (instanceHandle != null)
					{
						instanceHandle.Dispose();
					}
				}
				if (t == null)
				{
					flag = this._moreResultsAreExpected;
				}
				else
				{
					flag = true;
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		public bool MoveNext()
		{
			bool flag;
			lock (this._disposeThreadSafetyLock)
			{
				flag = this.MoveNext(false);
			}
			return flag;
		}

		internal abstract MiResult NativeMoveNext(OperationHandle operationHandle, out T currentItem, out bool moreResults, out MiResult operationResult, out string errorMessage, out InstanceHandle errorDetailsHandle);

		internal override void ReportInternalError(OperationCallbackProcessingContext callbackProcessingContext, Exception internalError)
		{
			lock (this._internalErrorWhileProcessingAsyncCallbackLock)
			{
				if (this._internalErrorWhileProcessingAsyncCallback != null)
				{
					Exception exception = this._internalErrorWhileProcessingAsyncCallback;
					Exception[] exceptionArray = new Exception[2];
					exceptionArray[0] = exception;
					exceptionArray[1] = internalError;
					this._internalErrorWhileProcessingAsyncCallback = new AggregateException(exceptionArray);
				}
				else
				{
					this._internalErrorWhileProcessingAsyncCallback = internalError;
				}
			}
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}
}