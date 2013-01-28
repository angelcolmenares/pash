using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADFineGrainedPasswordPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219348", SupportsShouldProcess=true)]
	public class SetADFineGrainedPasswordPolicy : ADSetCmdletBase<SetADFineGrainedPasswordPolicyParameterSet, ADFineGrainedPasswordPolicyFactory<ADFineGrainedPasswordPolicy>, ADFineGrainedPasswordPolicy>
	{
		public SetADFineGrainedPasswordPolicy()
		{
		}
	}
}