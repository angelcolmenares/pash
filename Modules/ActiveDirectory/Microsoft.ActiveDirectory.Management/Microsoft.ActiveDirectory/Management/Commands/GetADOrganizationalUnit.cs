using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADOrganizationalUnit", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219307", DefaultParameterSetName="Filter")]
	public class GetADOrganizationalUnit : ADGetCmdletBase<GetADOrganizationalUnitParameterSet, ADOrganizationalUnitFactory<ADOrganizationalUnit>, ADOrganizationalUnit>
	{
		public GetADOrganizationalUnit()
		{
		}
	}
}