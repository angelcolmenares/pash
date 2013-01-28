using System;

namespace Microsoft.PowerShell.Activities
{
	public abstract class PSWorkflowHost
	{
		public virtual RunspaceProvider LocalRunspaceProvider
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual PSActivityHostController PSActivityHostController
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual RunspaceProvider RemoteRunspaceProvider
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual RunspaceProvider UnboundedLocalRunspaceProvider
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		protected PSWorkflowHost()
		{
		}
	}
}