using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADUser", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219331", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADUser : ADRemoveCmdletBase<RemoveADUserParameterSet, ADUserFactory<ADUser>, ADUser>
	{
		public RemoveADUser()
		{
		}
	}
}