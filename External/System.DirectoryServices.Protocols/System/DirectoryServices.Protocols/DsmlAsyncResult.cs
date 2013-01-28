using Microsoft.Win32.SafeHandles;
using System;
using System.Threading;

namespace System.DirectoryServices.Protocols
{
	internal class DsmlAsyncResult : IAsyncResult
	{
		private DsmlAsyncResult.DsmlAsyncWaitHandle asyncWaitHandle;

		internal AsyncCallback callback;

		internal bool completed;

		private bool completedSynchronously;

		internal ManualResetEvent manualResetEvent;

		private object stateObject;

		internal RequestState resultObject;

		internal bool hasValidRequest;

		object System.IAsyncResult.AsyncState
		{
			get
			{
				return this.stateObject;
			}
		}

		WaitHandle System.IAsyncResult.AsyncWaitHandle
		{
			get
			{
				if (this.asyncWaitHandle == null)
				{
					this.asyncWaitHandle = new DsmlAsyncResult.DsmlAsyncWaitHandle(this.manualResetEvent.SafeWaitHandle);
				}
				return this.asyncWaitHandle;
			}
		}

		bool System.IAsyncResult.CompletedSynchronously
		{
			get
			{
				return this.completedSynchronously;
			}
		}

		bool System.IAsyncResult.IsCompleted
		{
			get
			{
				return this.completed;
			}
		}

		public DsmlAsyncResult(AsyncCallback callbackRoutine, object state)
		{
			this.stateObject = state;
			this.callback = callbackRoutine;
			this.manualResetEvent = new ManualResetEvent(false);
		}

		public override bool Equals(object o)
		{
			if (o as DsmlAsyncResult == null || o == null)
			{
				return false;
			}
			else
			{
				return this == (DsmlAsyncResult)o;
			}
		}

		public override int GetHashCode()
		{
			return this.manualResetEvent.GetHashCode();
		}

		internal sealed class DsmlAsyncWaitHandle : WaitHandle
		{
			public DsmlAsyncWaitHandle(SafeWaitHandle handle)
			{
				base.SafeWaitHandle = handle;
			}

			~DsmlAsyncWaitHandle()
			{
				try
				{
					base.SafeWaitHandle = null;
				}
				finally
				{
					//base.Finalize();
				}
			}
		}
	}
}