using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADDefaultDomainPasswordPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219316", DefaultParameterSetName="Current")]
	public class GetADDefaultDomainPasswordPolicy : ADGetDomainCmdletBase<GetADDefaultDomainPasswordPolicyParameterSet, ADDefaultDomainPasswordPolicyFactory<ADDefaultDomainPasswordPolicy>, ADDefaultDomainPasswordPolicy>
	{
		public GetADDefaultDomainPasswordPolicy()
		{
		}

		protected internal override ADDefaultDomainPasswordPolicy ConstructObjectFromIdentity(string currentDomain)
		{
			return new ADDefaultDomainPasswordPolicy(currentDomain);
		}
	}
}