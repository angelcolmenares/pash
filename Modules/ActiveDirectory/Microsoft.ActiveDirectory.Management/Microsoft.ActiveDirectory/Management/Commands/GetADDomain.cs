using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADDomain", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219304", DefaultParameterSetName="Current")]
	public class GetADDomain : ADGetDomainCmdletBase<GetADDomainParameterSet, ADDomainFactory<ADDomain>, ADDomain>
	{
		public GetADDomain()
		{
		}

		protected internal override ADDomain ConstructObjectFromIdentity(string currentDomain)
		{
			return new ADDomain(currentDomain);
		}
	}
}