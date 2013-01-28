using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADOrganizationalUnit", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219335", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADOrganizationalUnit : ADRemoveCmdletBase<RemoveADOrganizationalUnitParameterSet, ADOrganizationalUnitFactory<ADOrganizationalUnit>, ADOrganizationalUnit>
	{
		public RemoveADOrganizationalUnit()
		{
		}
	}
}