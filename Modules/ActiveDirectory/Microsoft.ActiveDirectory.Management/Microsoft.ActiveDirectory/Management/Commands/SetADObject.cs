using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADObject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219344", SupportsShouldProcess=true)]
	public class SetADObject : ADSetCmdletBase<SetADObjectParameterSet, ADObjectFactory<ADObject>, ADObject>
	{
		public SetADObject()
		{
		}
	}
}