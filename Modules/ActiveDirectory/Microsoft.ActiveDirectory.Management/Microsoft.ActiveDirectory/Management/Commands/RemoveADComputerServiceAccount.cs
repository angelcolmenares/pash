using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADComputerServiceAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219340", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADComputerServiceAccount : SetADComputerServiceAccount<RemoveADComputerServiceAccountParameterSet>
	{
		public RemoveADComputerServiceAccount() : base(((PropertyModifyOp)1).ToString())
		{
		}
	}
}