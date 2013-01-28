using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Unlock", "ADAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219360", SupportsShouldProcess=true)]
	public class UnlockADAccount : SetADAccountControlBase<UnlockADAccountParameterSet>
	{
		public UnlockADAccount() : base(0)
		{
		}
	}
}