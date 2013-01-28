using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADOrganizationalUnit", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219328", SupportsShouldProcess=true)]
	public class NewADOrganizationalUnit : ADNewCmdletBase<NewADOrganizationalUnitParameterSet, ADOrganizationalUnitFactory<ADOrganizationalUnit>, ADOrganizationalUnit>
	{
		public NewADOrganizationalUnit()
		{
		}
	}
}