using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADObject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219330", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADObject : ADRemoveCmdletBase<RemoveADObjectParameterSet, ADObjectFactory<ADObject>, ADObject>
	{
		public RemoveADObject()
		{
		}
	}
}