using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADObject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219298", DefaultParameterSetName="Filter")]
	public class GetADObject : ADGetCmdletBase<GetADObjectParameterSet, ADObjectFactory<ADObject>, ADObject>
	{
		public GetADObject()
		{
		}
	}
}