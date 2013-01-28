using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADGroup", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219302", DefaultParameterSetName="Filter")]
	public class GetADGroup : ADGetCmdletBase<GetADGroupParameterSet, ADGroupFactory<ADGroup>, ADGroup>
	{
		public GetADGroup()
		{
		}
	}
}