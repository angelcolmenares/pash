using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADAccountControl", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219352", SupportsShouldProcess=true)]
	public class SetADAccountControl : SetADAccountControlBase<SetADAccountControlParameterSet>
	{
		public SetADAccountControl()
		{
		}
	}
}