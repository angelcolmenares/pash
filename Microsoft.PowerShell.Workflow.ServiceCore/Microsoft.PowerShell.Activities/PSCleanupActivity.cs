using System;
using System.Activities;
using System.Management.Automation;
using System.Threading;

namespace Microsoft.PowerShell.Activities
{
	public abstract class PSCleanupActivity : PSRemotingActivity
	{
		protected PSCleanupActivity()
		{
		}

		internal virtual void DoCleanup(RunCommandsArguments args, WaitCallback callback)
		{
			throw new NotImplementedException();
		}

		protected override ActivityImplementationContext GetPowerShell(NativeActivityContext context)
		{
			ActivityImplementationContext activityImplementationContext = new ActivityImplementationContext();
			activityImplementationContext.PowerShellInstance = PowerShell.Create();
			ActivityImplementationContext activityImplementationContext1 = activityImplementationContext;
			return activityImplementationContext1;
		}
	}
}