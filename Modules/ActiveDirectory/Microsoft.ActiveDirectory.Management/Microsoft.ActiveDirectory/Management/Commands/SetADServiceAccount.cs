using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADServiceAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219355", SupportsShouldProcess=true)]
	public class SetADServiceAccount : ADSetCmdletBase<SetADServiceAccountParameterSet, ADServiceAccountFactory<ADServiceAccount>, ADServiceAccount>
	{
		public SetADServiceAccount()
		{
		}
	}
}