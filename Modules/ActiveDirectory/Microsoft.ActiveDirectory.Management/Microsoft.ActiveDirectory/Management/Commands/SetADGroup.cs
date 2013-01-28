using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADGroup", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219347", SupportsShouldProcess=true)]
	public class SetADGroup : ADSetCmdletBase<SetADGroupParameterSet, ADGroupFactory<ADGroup>, ADGroup>
	{
		public SetADGroup()
		{
		}
	}
}