using System;
using System.Activities;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace Microsoft.PowerShell.Activities
{
	public class DisablePSWorkflowConnection : PSCleanupActivity
	{
		private const int DefaultCleanupWaitTimerIntervalMs = 0x493e0;

		private int timeout;

		[RequiredArgument]
		public InArgument<int> TimeoutSec
		{
			get;
			set;
		}

		public DisablePSWorkflowConnection()
		{
			this.timeout = 0x493e0;
			base.DisplayName = "Disable-PSWorkflowConnection";
		}

		internal override void DoCleanup(RunCommandsArguments args, WaitCallback callback)
		{
			PSWorkflowHost workflowHost = args.WorkflowHost;
			WSManConnectionInfo connectionInfo = args.ImplementationContext.PowerShellInstance.Runspace.ConnectionInfo as WSManConnectionInfo;
			args.CleanupTimeout = this.timeout;
			if (connectionInfo != null)
			{
				workflowHost.RemoteRunspaceProvider.RequestCleanup(connectionInfo, callback, args);
			}
			else
			{
				if (callback != null)
				{
					callback(args);
					return;
				}
			}
		}

		protected override ActivityImplementationContext GetPowerShell(NativeActivityContext context)
		{
			if (this.TimeoutSec.Expression != null)
			{
				this.timeout = (int)this.TimeoutSec.Get(context);
			}
			return base.GetPowerShell(context);
		}
	}
}