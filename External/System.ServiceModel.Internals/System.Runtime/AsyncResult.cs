using System;
using System.Threading;

namespace System.Runtime
{
	internal abstract class AsyncResult : IAsyncResult
	{
		private static AsyncCallback asyncCompletionWrapperCallback;

		private AsyncCallback callback;

		private bool completedSynchronously;

		private bool endCalled;

		private Exception exception;

		private bool isCompleted;

		private AsyncResult.AsyncCompletion nextAsyncCompletion;

		private object state;

		private Action beforePrepareAsyncCompletionAction;

		private Func<IAsyncResult, bool> checkSyncValidationFunc;

		private ManualResetEvent manualResetEvent;

		private object thisLock;

		public object AsyncState
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.state;
			}
		}

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				if (this.manualResetEvent == null)
				{
					lock (this.ThisLock)
					{
						if (this.manualResetEvent == null)
						{
							this.manualResetEvent = new ManualResetEvent(this.isCompleted);
						}
					}
					return this.manualResetEvent;
				}
				else
				{
					return this.manualResetEvent;
				}
			}
		}

		public bool CompletedSynchronously
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.completedSynchronously;
			}
		}

		public bool HasCallback
		{
			get
			{
				return this.callback != null;
			}
		}

		public bool IsCompleted
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.isCompleted;
			}
		}

		protected Action<AsyncResult, Exception> OnCompleting
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set;
		}

		private object ThisLock
		{
			get
			{
				return this.thisLock;
			}
		}

		protected Action<AsyncCallback, IAsyncResult> VirtualCallback
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set;
		}

		protected AsyncResult(AsyncCallback callback, object state)
		{
			this.callback = callback;
			this.state = state;
			this.thisLock = new object();
		}

		private static void AsyncCompletionWrapperCallback(IAsyncResult result)
		{
			if (result != null)
			{
				if (!result.CompletedSynchronously)
				{
					AsyncResult asyncState = (AsyncResult)result.AsyncState;
					if (asyncState.OnContinueAsyncCompletion(result))
					{
						AsyncResult.AsyncCompletion nextCompletion = asyncState.GetNextCompletion();
						if (nextCompletion == null)
						{
							AsyncResult.ThrowInvalidAsyncResult(result);
						}
						bool flag = false;
						Exception exception = null;
						try
						{
							flag = nextCompletion(result);
						}
						catch (Exception exception2)
						{
							Exception exception1 = exception2;
							if (!Fx.IsFatal(exception1))
							{
								flag = true;
								exception = exception1;
							}
							else
							{
								throw;
							}
						}
						if (flag)
						{
							asyncState.Complete(false, exception);
						}
						return;
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.InvalidNullAsyncResult));
			}
		}

		protected bool CheckSyncContinue(IAsyncResult result)
		{
			AsyncResult.AsyncCompletion asyncCompletion = null;
			return this.TryContinueHelper(result, out asyncCompletion);
		}

		protected void Complete(bool completedSynchronously)
		{
			if (!this.isCompleted)
			{
				this.completedSynchronously = completedSynchronously;
				if (this.OnCompleting != null)
				{
					try
					{
						this.OnCompleting(this, this.exception);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						if (!Fx.IsFatal(exception))
						{
							this.exception = exception;
						}
						else
						{
							throw;
						}
					}
				}
				if (!completedSynchronously)
				{
					lock (this.ThisLock)
					{
						this.isCompleted = true;
						if (this.manualResetEvent != null)
						{
							this.manualResetEvent.Set();
						}
					}
				}
				else
				{
					this.isCompleted = true;
				}
				if (this.callback != null)
				{
					try
					{
						if (this.VirtualCallback == null)
						{
							this.callback(this);
						}
						else
						{
							this.VirtualCallback(this.callback, this);
						}
					}
					catch (Exception exception3)
					{
						Exception exception2 = exception3;
						if (!Fx.IsFatal(exception2))
						{
							throw Fx.Exception.AsError(new CallbackException(InternalSR.AsyncCallbackThrewException, exception2));
						}
						else
						{
							throw;
						}
					}
				}
				return;
			}
			else
			{
				throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.AsyncResultCompletedTwice(this.GetType())));
			}
		}

		protected void Complete(bool completedSynchronously, Exception exception)
		{
			this.exception = exception;
			this.Complete(completedSynchronously);
		}

		protected static TAsyncResult End<TAsyncResult>(IAsyncResult result)
		where TAsyncResult : AsyncResult
		{
			if (result != null)
			{
				TAsyncResult tAsyncResult = (TAsyncResult)(result as TAsyncResult);
				if (tAsyncResult != null)
				{
					if (!tAsyncResult.endCalled)
					{
						tAsyncResult.endCalled = true;
						if (!tAsyncResult.isCompleted)
						{
							tAsyncResult.AsyncWaitHandle.WaitOne();
						}
						if (tAsyncResult.manualResetEvent != null)
						{
							tAsyncResult.manualResetEvent.Close();
						}
						if (tAsyncResult.exception == null)
						{
							return tAsyncResult;
						}
						else
						{
							throw Fx.Exception.AsError(tAsyncResult.exception);
						}
					}
					else
					{
						throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.AsyncResultAlreadyEnded));
					}
				}
				else
				{
					throw Fx.Exception.Argument("result", InternalSR.InvalidAsyncResult);
				}
			}
			else
			{
				throw Fx.Exception.ArgumentNull("result");
			}
		}

		private AsyncResult.AsyncCompletion GetNextCompletion()
		{
			AsyncResult.AsyncCompletion asyncCompletion = this.nextAsyncCompletion;
			this.nextAsyncCompletion = null;
			return asyncCompletion;
		}

		protected virtual bool OnContinueAsyncCompletion(IAsyncResult result)
		{
			return true;
		}

		protected AsyncCallback PrepareAsyncCompletion(AsyncResult.AsyncCompletion callback)
		{
			if (this.beforePrepareAsyncCompletionAction != null)
			{
				this.beforePrepareAsyncCompletionAction();
			}
			this.nextAsyncCompletion = callback;
			if (AsyncResult.asyncCompletionWrapperCallback == null)
			{
				AsyncResult.asyncCompletionWrapperCallback = Fx.ThunkCallback(new AsyncCallback(AsyncResult.AsyncCompletionWrapperCallback));
			}
			return AsyncResult.asyncCompletionWrapperCallback;
		}

		protected void SetBeforePrepareAsyncCompletionAction(Action beforePrepareAsyncCompletionAction)
		{
			this.beforePrepareAsyncCompletionAction = beforePrepareAsyncCompletionAction;
		}

		protected void SetCheckSyncValidationFunc(Func<IAsyncResult, bool> checkSyncValidationFunc)
		{
			this.checkSyncValidationFunc = checkSyncValidationFunc;
		}

		protected bool SyncContinue(IAsyncResult result)
		{
			AsyncResult.AsyncCompletion asyncCompletion = null;
			if (!this.TryContinueHelper(result, out asyncCompletion))
			{
				return false;
			}
			else
			{
				return asyncCompletion(result);
			}
		}

		protected static void ThrowInvalidAsyncResult(IAsyncResult result)
		{
			throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.InvalidAsyncResultImplementation(result.GetType())));
		}

		protected static void ThrowInvalidAsyncResult(string debugText)
		{
			string invalidAsyncResultImplementationGeneric = InternalSR.InvalidAsyncResultImplementationGeneric;
			throw Fx.Exception.AsError(new InvalidOperationException(invalidAsyncResultImplementationGeneric));
		}

		private bool TryContinueHelper(IAsyncResult result, out AsyncResult.AsyncCompletion callback)
		{
			if (result != null)
			{
				callback = null;
				if (this.checkSyncValidationFunc == null)
				{
					if (!result.CompletedSynchronously)
					{
						return false;
					}
				}
				else
				{
					if (!this.checkSyncValidationFunc(result))
					{
						return false;
					}
				}
				callback = this.GetNextCompletion();
				if (callback == null)
				{
					AsyncResult.ThrowInvalidAsyncResult("Only call Check/SyncContinue once per async operation (once per PrepareAsyncCompletion).");
				}
				return true;
			}
			else
			{
				throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.InvalidNullAsyncResult));
			}
		}

		protected delegate bool AsyncCompletion(IAsyncResult result);
	}
}