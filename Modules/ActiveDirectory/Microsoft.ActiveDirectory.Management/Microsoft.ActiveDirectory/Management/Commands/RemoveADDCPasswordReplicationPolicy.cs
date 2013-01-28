using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADDomainControllerPasswordReplicationPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216390", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADDCPasswordReplicationPolicy : SetADDCPasswordReplicationPolicy<RemoveADDomainControllerPasswordReplicationPolicyParameterSet>
	{
		public RemoveADDCPasswordReplicationPolicy() : base((SetADDCPasswordReplicationPolicyOperationType)1)
		{
		}
	}
}