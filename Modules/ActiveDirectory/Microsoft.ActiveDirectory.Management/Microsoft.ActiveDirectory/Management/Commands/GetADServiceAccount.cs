using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADServiceAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219314", DefaultParameterSetName="Filter")]
	public class GetADServiceAccount : ADGetCmdletBase<GetADServiceAccountParameterSet, ADServiceAccountFactory<ADServiceAccount>, ADServiceAccount>
	{
		public GetADServiceAccount()
		{
		}
	}
}