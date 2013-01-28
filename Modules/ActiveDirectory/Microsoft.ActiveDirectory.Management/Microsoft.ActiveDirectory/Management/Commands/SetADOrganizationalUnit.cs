using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADOrganizationalUnit", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219351", SupportsShouldProcess=true)]
	public class SetADOrganizationalUnit : ADSetCmdletBase<SetADOrganizationalUnitParameterSet, ADOrganizationalUnitFactory<ADOrganizationalUnit>, ADOrganizationalUnit>
	{
		public SetADOrganizationalUnit()
		{
		}
	}
}