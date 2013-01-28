using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADFineGrainedPasswordPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219334", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADFineGrainedPasswordPolicy : ADRemoveCmdletBase<RemoveADFineGrainedPasswordPolicyParameterSet, ADFineGrainedPasswordPolicyFactory<ADFineGrainedPasswordPolicy>, ADFineGrainedPasswordPolicy>
	{
		public RemoveADFineGrainedPasswordPolicy()
		{
		}
	}
}