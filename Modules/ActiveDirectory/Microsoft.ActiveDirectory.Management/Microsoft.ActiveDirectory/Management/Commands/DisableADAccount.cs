using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Disable", "ADAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219294", SupportsShouldProcess=true)]
	public class DisableADAccount : SetADAccountControlBase<DisableADAccountParameterSet>
	{
		public DisableADAccount() : base((SetADAccountControlAction)1)
		{
		}
	}
}