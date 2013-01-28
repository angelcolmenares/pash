using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADComputer", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219332", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADComputer : ADRemoveCmdletBase<RemoveADComputerParameterSet, ADComputerFactory<ADComputer>, ADComputer>
	{
		public RemoveADComputer()
		{
		}
	}
}