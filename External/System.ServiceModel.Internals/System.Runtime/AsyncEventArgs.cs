using System;

namespace System.Runtime
{
	internal abstract class AsyncEventArgs : IAsyncEventArgs
	{
		private AsyncEventArgs.OperationState state;

		private object asyncState;

		private AsyncEventArgsCallback callback;

		private Exception exception;

		public object AsyncState
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.asyncState;
			}
		}

		public Exception Exception
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.exception;
			}
		}

		private AsyncEventArgs.OperationState State
		{
			set
			{
				AsyncEventArgs.OperationState operationState = value;
				switch (operationState)
				{
					case AsyncEventArgs.OperationState.PendingCompletion:
					{
						if (this.state != AsyncEventArgs.OperationState.PendingCompletion)
						{
							break;
						}
						throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.AsyncEventArgsCompletionPending(this.GetType())));
					}
					case AsyncEventArgs.OperationState.CompletedSynchronously:
					case AsyncEventArgs.OperationState.CompletedAsynchronously:
					{
						if (this.state == AsyncEventArgs.OperationState.PendingCompletion)
						{
							break;
						}
						throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.AsyncEventArgsCompletedTwice(this.GetType())));
					}
				}
				this.state = value;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected AsyncEventArgs()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Complete(bool completedSynchronously)
		{
			this.Complete(completedSynchronously, null);
		}

		public virtual void Complete(bool completedSynchronously, Exception exception)
		{
			this.exception = exception;
			if (!completedSynchronously)
			{
				this.State = AsyncEventArgs.OperationState.CompletedAsynchronously;
				this.callback(this);
				return;
			}
			else
			{
				this.State = AsyncEventArgs.OperationState.CompletedSynchronously;
				return;
			}
		}

		protected void SetAsyncState(AsyncEventArgsCallback callback, object state)
		{
			if (callback != null)
			{
				this.State = AsyncEventArgs.OperationState.PendingCompletion;
				this.asyncState = state;
				this.callback = callback;
				return;
			}
			else
			{
				throw Fx.Exception.ArgumentNull("callback");
			}
		}

		private enum OperationState
		{
			Created,
			PendingCompletion,
			CompletedSynchronously,
			CompletedAsynchronously
		}
	}
}