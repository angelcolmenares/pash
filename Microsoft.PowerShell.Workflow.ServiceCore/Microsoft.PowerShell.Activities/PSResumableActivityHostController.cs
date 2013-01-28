using Microsoft.PowerShell.Workflow;
using System;
using System.Activities;
using System.Management.Automation;

namespace Microsoft.PowerShell.Activities
{
	public abstract class PSResumableActivityHostController : PSActivityHostController
	{
		public virtual bool SupportDisconnectedPSStreams
		{
			get
			{
				return true;
			}
		}

		protected PSResumableActivityHostController(PSWorkflowRuntime runtime) : base(runtime)
		{
		}

		public virtual void StartResumablePSCommand(Guid jobInstanceId, Bookmark bookmark, System.Management.Automation.PowerShell command, PowerShellStreams<PSObject, PSObject> streams, PSActivityEnvironment environment, PSActivity activityInstance)
		{
			throw new NotImplementedException();
		}

		public virtual void StopAllResumablePSCommands(Guid jobInstanceId)
		{
			throw new NotImplementedException();
		}
	}
}