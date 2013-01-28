using System;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace Microsoft.PowerShell.Activities
{
	public abstract class RunspaceProvider
	{
		protected RunspaceProvider()
		{
		}

		public virtual IAsyncResult BeginGetRunspace(WSManConnectionInfo connectionInfo, uint retryCount, uint retryInterval, AsyncCallback callback, object state)
		{
			throw new NotImplementedException();
		}

		public virtual Runspace EndGetRunspace(IAsyncResult asyncResult)
		{
			throw new NotImplementedException();
		}

		public virtual Runspace GetRunspace(WSManConnectionInfo connectionInfo, uint retryCount, uint retryInterval)
		{
			throw new NotImplementedException();
		}

		public virtual bool IsDisconnectedByRunspaceProvider(Runspace runspace)
		{
			throw new NotImplementedException();
		}

		public virtual void ReadyForDisconnect(Runspace runspace)
		{
			throw new NotImplementedException();
		}

		public virtual void ReleaseRunspace(Runspace runspace)
		{
			throw new NotImplementedException();
		}

		public virtual void RequestCleanup(WSManConnectionInfo connectionInfo, WaitCallback callback, object state)
		{
			throw new NotImplementedException();
		}
	}
}