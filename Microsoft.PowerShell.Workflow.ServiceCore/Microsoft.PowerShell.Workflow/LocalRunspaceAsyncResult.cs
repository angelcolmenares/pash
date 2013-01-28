using System;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Workflow
{
	internal class LocalRunspaceAsyncResult : ConnectionAsyncResult
	{
		internal Runspace Runspace
		{
			get;
			set;
		}

		internal LocalRunspaceAsyncResult(object state, AsyncCallback callback, Guid ownerId) : base(state, callback, ownerId)
		{
		}
	}
}