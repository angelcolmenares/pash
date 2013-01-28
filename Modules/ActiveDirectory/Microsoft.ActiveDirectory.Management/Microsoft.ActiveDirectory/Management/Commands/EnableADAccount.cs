using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Enable", "ADAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219296", SupportsShouldProcess=true)]
	public class EnableADAccount : SetADAccountControlBase<EnableADAccountParameterSet>
	{
		public EnableADAccount() : base((SetADAccountControlAction)2)
		{
		}
	}
}