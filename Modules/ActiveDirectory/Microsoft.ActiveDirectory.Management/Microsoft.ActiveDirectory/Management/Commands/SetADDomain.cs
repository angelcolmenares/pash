using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADDomain", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219349", SupportsShouldProcess=true)]
	public class SetADDomain : ADSetDomainCmdletBase<SetADDomainParameterSet, ADDomainFactory<ADDomain>, ADDomain>
	{
		public SetADDomain()
		{
		}
	}
}