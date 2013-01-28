using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADGroup", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219333", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADGroup : ADRemoveCmdletBase<RemoveADGroupParameterSet, ADGroupFactory<ADGroup>, ADGroup>
	{
		public RemoveADGroup()
		{
		}
	}
}