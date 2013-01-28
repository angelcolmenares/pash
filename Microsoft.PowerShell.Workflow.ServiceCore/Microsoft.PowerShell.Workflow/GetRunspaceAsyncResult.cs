using System;

namespace Microsoft.PowerShell.Workflow
{
	internal class GetRunspaceAsyncResult : ConnectionAsyncResult
	{
		internal Connection Connection
		{
			get;
			set;
		}

		internal GetRunspaceAsyncResult(object state, AsyncCallback callback, Guid ownerId) : base(state, callback, ownerId)
		{
		}
	}
}