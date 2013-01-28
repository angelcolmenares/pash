using System;
using System.Threading;

namespace Microsoft.PowerShell.Workflow
{
	internal class ConnectionAsyncResult : IAsyncResult
	{
		private readonly object _state;

		private readonly AsyncCallback _callback;

		private bool _isCompleted;

		private ManualResetEvent _completedWaitHandle;

		private readonly object _syncObject;

		private Exception _exception;

		private readonly Guid _ownerId;

		private bool _completedSynchronously;

		public object AsyncState
		{
			get
			{
				return this._state;
			}
		}

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				if (this._completedWaitHandle == null)
				{
					lock (this._syncObject)
					{
						if (this._completedWaitHandle == null)
						{
							this._completedWaitHandle = new ManualResetEvent(this._isCompleted);
						}
					}
				}
				return this._completedWaitHandle;
			}
		}

		internal AsyncCallback Callback
		{
			get
			{
				return this._callback;
			}
		}

		public bool JustDecompileGenerated_get_CompletedSynchronously()
		{
			return this._completedSynchronously;
		}

		public void JustDecompileGenerated_set_CompletedSynchronously(bool value)
		{
			this._completedSynchronously = value;
		}

		public bool CompletedSynchronously
		{
			get
			{
				return JustDecompileGenerated_get_CompletedSynchronously();
			}
			set
			{
				JustDecompileGenerated_set_CompletedSynchronously(value);
			}
		}

		internal ActivityInvoker Invoker
		{
			get;
			set;
		}

		public bool IsCompleted
		{
			get
			{
				return this._isCompleted;
			}
		}

		internal Guid OwnerId
		{
			get
			{
				return this._ownerId;
			}
		}

		internal object State
		{
			get
			{
				return this._state;
			}
		}

		internal ConnectionAsyncResult(object state, AsyncCallback callback, Guid ownerId)
		{
			this._syncObject = new object();
			this._state = state;
			this._ownerId = ownerId;
			this._callback = callback;
		}

		internal void EndInvoke()
		{
			this.AsyncWaitHandle.WaitOne();
			this.AsyncWaitHandle.Close();
			this._completedWaitHandle = null;
			if (this._exception == null)
			{
				return;
			}
			else
			{
				throw this._exception;
			}
		}

		internal void SetAsCompleted(Exception exception)
		{
			if (!this._isCompleted)
			{
				lock (this._syncObject)
				{
					if (!this._isCompleted)
					{
						this._isCompleted = true;
						this._exception = exception;
						if (this._completedWaitHandle != null)
						{
							this._completedWaitHandle.Set();
						}
					}
					else
					{
						return;
					}
				}
				if (this._callback != null)
				{
					this._callback(this);
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}