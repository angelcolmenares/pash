using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADGroupMember", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219336", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADGroupMember : SetADGroupMember<RemoveADGroupMemberParameterSet>
	{
		public RemoveADGroupMember() : base((SetADGroupMemberOperationType)1)
		{
		}
	}
}