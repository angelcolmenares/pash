using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADPrincipalGroupMembership", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219337", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADPrincipalGroupMembership : SetADPrincipalGroupMembership<RemoveADPrincipalGroupMembershipParameterSet>
	{
		public RemoveADPrincipalGroupMembership() : base((SetADPrincipalGroupMembershipOperationType)1)
		{
		}
	}
}