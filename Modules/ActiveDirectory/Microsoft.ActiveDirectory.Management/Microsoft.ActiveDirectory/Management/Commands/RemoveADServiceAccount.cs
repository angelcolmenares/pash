using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADServiceAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219339", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADServiceAccount : ADRemoveCmdletBase<RemoveADServiceAccountParameterSet, ADServiceAccountFactory<ADServiceAccount>, ADServiceAccount>
	{
		public RemoveADServiceAccount()
		{
		}
	}
}