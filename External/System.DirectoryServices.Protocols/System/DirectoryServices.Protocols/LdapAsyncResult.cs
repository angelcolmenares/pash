using Microsoft.Win32.SafeHandles;
using System;
using System.Threading;

namespace System.DirectoryServices.Protocols
{
	internal class LdapAsyncResult : IAsyncResult
	{
		private LdapAsyncResult.LdapAsyncWaitHandle asyncWaitHandle;

		internal AsyncCallback callback;

		internal bool completed;

		private bool completedSynchronously;

		internal ManualResetEvent manualResetEvent;

		private object stateObject;

		internal LdapRequestState resultObject;

		internal bool partialResults;

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
					this.asyncWaitHandle = new LdapAsyncResult.LdapAsyncWaitHandle(this.manualResetEvent.SafeWaitHandle);
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

		public LdapAsyncResult(AsyncCallback callbackRoutine, object state, bool partialResults)
		{
			this.stateObject = state;
			this.callback = callbackRoutine;
			this.manualResetEvent = new ManualResetEvent(false);
			this.partialResults = partialResults;
		}

		public override bool Equals(object o)
		{
			if (o as LdapAsyncResult == null || o == null)
			{
				return false;
			}
			else
			{
				return this == (LdapAsyncResult)o;
			}
		}

		public override int GetHashCode()
		{
			return this.manualResetEvent.GetHashCode();
		}

		internal sealed class LdapAsyncWaitHandle : WaitHandle
		{
			public LdapAsyncWaitHandle(SafeWaitHandle handle)
			{
				base.SafeWaitHandle = handle;
			}

			~LdapAsyncWaitHandle()
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