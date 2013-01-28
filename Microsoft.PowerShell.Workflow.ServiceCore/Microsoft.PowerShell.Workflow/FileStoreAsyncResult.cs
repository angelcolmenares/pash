using System;
using System.Threading;

namespace Microsoft.PowerShell.Workflow
{
	internal abstract class FileStoreAsyncResult : IAsyncResult
	{
		private AsyncCallback callback;

		private object state;

		private bool completedSynchronously;

		private bool endCalled;

		private Exception exception;

		private bool isCompleted;

		private ManualResetEvent manualResetEvent;

		private object thisLock;

		public object AsyncState
		{
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
			get
			{
				return this.completedSynchronously;
			}
		}

		public bool IsCompleted
		{
			get
			{
				return this.isCompleted;
			}
		}

		private object ThisLock
		{
			get
			{
				return this.thisLock;
			}
		}

		protected FileStoreAsyncResult(AsyncCallback callback, object state)
		{
			this.callback = callback;
			this.state = state;
			this.thisLock = new object();
		}

		protected void Complete(bool completedSynchronously)
		{
			if (!this.isCompleted)
			{
				this.completedSynchronously = completedSynchronously;
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
					this.callback(this);
				}
				return;
			}
			else
			{
				throw new InvalidOperationException(Resources.AsyncResultAlreadyCompleted);
			}
		}

		protected void Complete(bool completedSynchronously, Exception exception)
		{
			this.exception = exception;
			this.Complete(completedSynchronously);
		}

		protected static TAsyncResult End<TAsyncResult>(IAsyncResult result)
		where TAsyncResult : FileStoreAsyncResult
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
							throw tAsyncResult.exception;
						}
					}
					else
					{
						throw new InvalidOperationException(Resources.AsyncResultAlreadyEnded);
					}
				}
				else
				{
					throw new ArgumentException(Resources.InvalidAsyncResult);
				}
			}
			else
			{
				throw new ArgumentNullException("result");
			}
		}
	}
}