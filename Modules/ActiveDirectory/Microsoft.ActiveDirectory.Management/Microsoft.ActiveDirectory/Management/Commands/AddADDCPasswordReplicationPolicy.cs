using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Add", "ADDomainControllerPasswordReplicationPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219290", SupportsShouldProcess=true)]
	public class AddADDCPasswordReplicationPolicy : SetADDCPasswordReplicationPolicy<AddADDomainControllerPasswordReplicationPolicyParameterSet>
	{
		public AddADDCPasswordReplicationPolicy() : base(0)
		{
		}
	}
}