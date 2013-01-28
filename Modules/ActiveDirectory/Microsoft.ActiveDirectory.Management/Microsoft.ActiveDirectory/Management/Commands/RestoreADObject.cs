using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Restore", "ADObject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219342", SupportsShouldProcess=true)]
	public class RestoreADObject : ADRestoreCmdletBase<RestoreADObjectParameterSet, ADObjectFactory<ADObject>, ADObject>
	{
		public RestoreADObject()
		{
		}
	}
}