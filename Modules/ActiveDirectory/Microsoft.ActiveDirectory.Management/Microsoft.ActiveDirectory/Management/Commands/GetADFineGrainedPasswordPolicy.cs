using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADFineGrainedPasswordPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219303", DefaultParameterSetName="Filter")]
	public class GetADFineGrainedPasswordPolicy : ADGetCmdletBase<GetADFineGrainedPasswordPolicyParameterSet, ADFineGrainedPasswordPolicyFactory<ADFineGrainedPasswordPolicy>, ADFineGrainedPasswordPolicy>
	{
		public GetADFineGrainedPasswordPolicy()
		{
		}
	}
}