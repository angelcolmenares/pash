using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using System;
using System.Management.Automation.Runspaces;

namespace Microsoft.Management.Odata.PS
{
	internal class PSRunspaceFactory : IItemFactory<PSRunspace, UserContext>
	{
		private SharedItemStore<InitialSessionState, UserContext> initialSessionStateStore;

		private bool executeCmdletInSameThread;

		public PSRunspaceFactory(SharedItemStore<InitialSessionState, UserContext> initialSessionStateStore, bool executeCmdletInSameThread = false)
		{
			this.initialSessionStateStore = initialSessionStateStore;
			this.executeCmdletInSameThread = executeCmdletInSameThread;
		}

		public PSRunspace Create(UserContext userContext, string membershipId)
		{
			PSRunspace pSRunspace;
			TraceHelper.Current.BeginOperation0("borrow InitialSessionState");
			Envelope<InitialSessionState, UserContext> envelope = this.initialSessionStateStore.Borrow(userContext, membershipId);
			using (envelope)
			{
				TraceHelper.Current.EndOperation("borrow InitialSessionState");
				using (OperationTracer operationTracer = new OperationTracer(new Action<string>(TraceHelper.Current.PowerShellRunspaceCreationStart), new Action<string>(TraceHelper.Current.PowerShellRunspaceCreationEnd), userContext.Name))
				{
					pSRunspace = new PSRunspace(envelope.Item, this.executeCmdletInSameThread);
				}
			}
			return pSRunspace;
		}
	}
}