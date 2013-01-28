using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADUser", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219300", DefaultParameterSetName="Filter")]
	public class GetADUser : ADGetCmdletBase<GetADUserParameterSet, ADUserFactory<ADUser>, ADUser>
	{
		public GetADUser()
		{
		}
	}
}