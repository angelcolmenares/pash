using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADDefaultDomainPasswordPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219358", SupportsShouldProcess=true)]
	public class SetADDefaultDomainPasswordPolicy : ADSetDomainCmdletBase<SetADDefaultDomainPasswordPolicyParameterSet, ADDefaultDomainPasswordPolicyFactory<ADDefaultDomainPasswordPolicy>, ADDefaultDomainPasswordPolicy>
	{
		public SetADDefaultDomainPasswordPolicy()
		{
		}
	}
}